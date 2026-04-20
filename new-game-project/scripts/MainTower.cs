using Godot;

public partial class MainTower : AnimatedSprite2D
{
	[Export] public PackedScene BulletScene;
	[Export] public float FireRate = 0.5f;
	[Export] public float BulletSpeed = 800f;
	[Export] public string IdleAnimation = "idle";
	[Export] public string ShootAnimation = "shoot";
	[Export] public float ShootFlashDuration = 0.15f;

	[Export] public float MaxShield = 100f;
	[Export] public float MaxHealth = 100f;

	private double lastFireTime = 0.0;
	private Marker2D muzzle;
	private Timer shootFlashTimer;

	private float currentShield;
	private float currentHealth;

	private ProgressBar shieldBar;
	private ProgressBar healthBar;
	private AnimatedSprite2D shieldIcon;
	private AnimatedSprite2D heartIcon;
	private Label coinLabel;

	private bool gameOverTriggered = false;

	// Placement System
	private bool isPlacingMine = false;
	private bool isPlacingWaterTank = false;
	private bool isPlacingEmp = false;
	private PackedScene mineScene;
	private PackedScene waterTankScene;
	private PackedScene empScene;

	public override void _Ready()
	{
		muzzle = GetNodeOrNull<Marker2D>("Muzzle");
		if (muzzle == null)
		{
			muzzle = new Marker2D { Name = "Muzzle", Position = new Vector2(30, 0) };
			AddChild(muzzle);
		}

		shootFlashTimer = new Timer { OneShot = true };
		shootFlashTimer.Timeout += () => Play(IdleAnimation);
		AddChild(shootFlashTimer);

		Play(IdleAnimation);

		currentShield = MaxShield;
		currentHealth = MaxHealth;

		shieldBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldBar");
		healthBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/HealthContainer/HealthBar");
		shieldIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldIcon");
		heartIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/HealthContainer/HeartIcon");
		coinLabel = GetTree().CurrentScene.GetNodeOrNull<Label>("UI/TowerStatus/CoinContainer/CoinLabel");

		ForceSeparateStyles();
		UpdateUI();
	}

	private void ForceSeparateStyles()
	{
		if (shieldBar != null)
		{
			shieldBar.AddThemeStyleboxOverride("fill", new StyleBoxFlat());
			shieldBar.AddThemeStyleboxOverride("background", new StyleBoxFlat());
		}
		if (healthBar != null)
		{
			healthBar.AddThemeStyleboxOverride("fill", new StyleBoxFlat());
			healthBar.AddThemeStyleboxOverride("background", new StyleBoxFlat());
		}
	}

	public override void _Process(double delta)
	{
		Vector2 mousePos = GetGlobalMousePosition();
		LookAt(mousePos);
		Rotation -= Mathf.Pi / 2;

		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			if (isPlacingMine)
				PlaceMine(mousePos);
			else if (isPlacingWaterTank)
				PlaceWaterTank(mousePos);
			else if (isPlacingEmp)
				PlaceEmp(mousePos);
			else
				TryFire(mousePos);
		}

		if (Input.IsActionJustPressed("open_shop") && !isPlacingMine && !isPlacingWaterTank && !isPlacingEmp)
		{
			OpenShopMenu();
		}

		if (Input.IsActionJustPressed("ui_cancel") && (isPlacingMine || isPlacingWaterTank || isPlacingEmp))
			CancelPlacement();

		UpdateUI();
	}

	private void OpenShopMenu()
	{
		if (GetTree().CurrentScene.GetNodeOrNull<CanvasLayer>("ShopMenu") != null)
			return;

		var shopScene = GD.Load<PackedScene>("res://scenes/shop_menu.tscn");
		if (shopScene != null)
		{
			var shopMenu = shopScene.Instantiate<CanvasLayer>();
			shopMenu.Name = "ShopMenu";
			GetTree().CurrentScene.AddChild(shopMenu);
			GD.Print("[Tower] Shop menu opened successfully!");
		}
	}

	public void StartMinePlacement()
	{
		isPlacingMine = true;
		isPlacingWaterTank = false;
		isPlacingEmp = false;
		mineScene = GD.Load<PackedScene>("res://scenes/Mine.tscn");
		GD.Print("Mine placement mode activated");
	}

	public void StartWaterTankPlacement()
	{
		isPlacingWaterTank = true;
		isPlacingMine = false;
		isPlacingEmp = false;
		waterTankScene = GD.Load<PackedScene>("res://scenes/Towers/WaterTank.tscn");
		GD.Print("Water Tank placement mode activated");
	}

	public void StartEmpPlacement()
	{
		isPlacingEmp = true;
		isPlacingMine = false;
		isPlacingWaterTank = false;
		empScene = GD.Load<PackedScene>("res://scenes/Towers/Emp.tscn");
		GD.Print("EMP placement mode activated");
	}

	private void PlaceMine(Vector2 position)
	{
		if (mineScene == null) return;
		var mine = mineScene.Instantiate<Area2D>();
		GetTree().CurrentScene.AddChild(mine);
		mine.GlobalPosition = position;
		isPlacingMine = false;
	}

	private void PlaceWaterTank(Vector2 position)
	{
		if (waterTankScene == null) return;
		var tank = waterTankScene.Instantiate<Area2D>();
		GetTree().CurrentScene.AddChild(tank);
		tank.GlobalPosition = position;
		isPlacingWaterTank = false;
	}

	private void PlaceEmp(Vector2 position)
	{
		if (empScene == null) return;
		var emp = empScene.Instantiate<Area2D>();
		GetTree().CurrentScene.AddChild(emp);
		emp.GlobalPosition = position;
		isPlacingEmp = false;
	}

	private void CancelPlacement()
	{
		isPlacingMine = false;
		isPlacingWaterTank = false;
		isPlacingEmp = false;
		GD.Print("Placement cancelled");
	}

	private void TryFire(Vector2 targetPos)
	{
		double now = Time.GetTicksMsec() / 1000.0;
		if (now - lastFireTime < FireRate) return;

		lastFireTime = now;

		if (BulletScene == null) return;

		var bullet = BulletScene.Instantiate<RigidBody2D>();
		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = muzzle.GlobalPosition;

		Vector2 direction = (targetPos - muzzle.GlobalPosition).Normalized();
		bullet.LinearVelocity = direction * BulletSpeed;

		Play(ShootAnimation);
		shootFlashTimer.Start(ShootFlashDuration);
	}

	public void TakeDamage(float damage)
	{
		if (currentShield > 0)
		{
			float dmgToShield = Mathf.Min(damage, currentShield);
			currentShield -= dmgToShield;
			damage -= dmgToShield;
		}

		if (damage > 0 && currentHealth > 0)
		{
			currentHealth -= damage;
			if (currentHealth < 0) currentHealth = 0;
		}

		UpdateUI();

		if (currentHealth <= 0 && !gameOverTriggered)
		{
			gameOverTriggered = true;
			TriggerGameOver();
		}
	}

	private void TriggerGameOver()
	{
		GD.Print("Tower Destroyed - Showing Game Over Menu");
		var gameOverScene = GD.Load<PackedScene>("res://scenes/GameOverMenu.tscn");
		if (gameOverScene != null)
		{
			var menu = gameOverScene.Instantiate<CanvasLayer>();
			GetTree().CurrentScene.AddChild(menu);
			GetTree().Paused = true;
		}
	}

	private void UpdateUI()
	{
		if (shieldBar != null) shieldBar.Value = currentShield;
		if (healthBar != null) healthBar.Value = currentHealth;
		if (coinLabel != null) coinLabel.Text = Economy.Coins.ToString();

		if (shieldBar != null)
		{
			var fill = shieldBar.GetThemeStylebox("fill") as StyleBoxFlat;
			if (fill != null) fill.BgColor = new Color(0.31f, 0.78f, 0.97f);

			var bg = shieldBar.GetThemeStylebox("background") as StyleBoxFlat;
			if (bg != null)
				bg.BgColor = (currentShield / MaxShield <= 0.05f) ? new Color(0.08f, 0.25f, 0.65f) : new Color(0.15f, 0.35f, 0.75f);
		}

		if (healthBar != null)
		{
			var fill = healthBar.GetThemeStylebox("fill") as StyleBoxFlat;
			if (fill != null)
				fill.BgColor = (currentHealth / MaxHealth > 0.6f) ? Colors.LimeGreen : (currentHealth / MaxHealth > 0.3f) ? Colors.Yellow : Colors.Red;

			var bg = healthBar.GetThemeStylebox("background") as StyleBoxFlat;
			if (bg != null)
				bg.BgColor = (currentHealth / MaxHealth <= 0.05f) ? Colors.DarkRed : new Color(0.2f, 0.2f, 0.2f);
		}

		if (shieldIcon?.SpriteFrames != null)
		{
			string anim = currentShield > 75 ? "shield_full" : currentShield > 50 ? "shield_75" : currentShield > 25 ? "shield_50" : currentShield > 0 ? "shield_25" : "shield_broken";
			if (shieldIcon.Animation != anim) shieldIcon.Play(anim);
		}

		if (heartIcon?.SpriteFrames != null)
		{
			string anim = currentHealth > 75 ? "heart_full" : currentHealth > 50 ? "heart_75" : currentHealth > 25 ? "heart_50" : currentHealth > 0 ? "heart_25" : "heart_broken";
			if (heartIcon.Animation != anim) heartIcon.Play(anim);
		}
	}
}
