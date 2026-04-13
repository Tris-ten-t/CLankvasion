using Godot;

public partial class MainTower : AnimatedSprite2D
{
	// Shooting
	[Export] public PackedScene BulletScene;
	[Export] public float FireRate = 0.5f;
	[Export] public float BulletSpeed = 800f;
	[Export] public string IdleAnimation = "idle";
	[Export] public string ShootAnimation = "shoot";
	[Export] public float ShootFlashDuration = 0.15f;

	// Shield & Health
	[Export] public float MaxShield = 100f;
	[Export] public float MaxHealth = 100f;

	private double lastFireTime = 0.0;
	private Marker2D muzzle;
	private Timer shootFlashTimer;

	private float currentShield;
	private float currentHealth;

	// UI References
	private ProgressBar shieldBar;
	private ProgressBar healthBar;
	private AnimatedSprite2D shieldIcon;
	private AnimatedSprite2D heartIcon;

	private bool gameOverTriggered = false;

	public override void _Ready()
	{
		muzzle = GetNodeOrNull<Marker2D>("Muzzle");
		if (muzzle == null)
		{
			muzzle = new Marker2D { Name = "Muzzle", Position = new Vector2(30, 0) };
			AddChild(muzzle);
		}

		shootFlashTimer = new Timer();
		shootFlashTimer.OneShot = true;
		shootFlashTimer.Timeout += () => Play(IdleAnimation);
		AddChild(shootFlashTimer);

		Play(IdleAnimation);

		currentShield = MaxShield;
		currentHealth = MaxHealth;

		shieldBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldBar");
		healthBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/HealthContainer/HealthBar");
		shieldIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldIcon");
		heartIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/HealthContainer/HeartIcon");

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
			TryFire(mousePos);

		UpdateUI();
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

		var gameOverScene = GD.Load<PackedScene>("res://GameOverMenu.tscn");
		if (gameOverScene != null)
		{
			var menu = gameOverScene.Instantiate<CanvasLayer>();
			GetTree().CurrentScene.AddChild(menu);

			// Optional: Pause the game
			GetTree().Paused = true;
		}
		else
		{
			GD.PrintErr("GameOverMenu.tscn not found! Make sure the scene exists at res://GameOverMenu.tscn");
		}
	}

	private void UpdateUI()
	{
		if (shieldBar != null) shieldBar.Value = currentShield;
		if (healthBar != null) healthBar.Value = currentHealth;

		// Shield Bar
		if (shieldBar != null)
		{
			StyleBoxFlat fillStyle = shieldBar.GetThemeStylebox("fill") as StyleBoxFlat;
			if (fillStyle != null)
			{
				float pct = currentShield / MaxShield;
				Color lightBlue = new Color(0.31f, 0.78f, 0.97f);
				fillStyle.BgColor = lightBlue;
			}

			StyleBoxFlat bgStyle = shieldBar.GetThemeStylebox("background") as StyleBoxFlat;
			if (bgStyle != null)
			{
				float pct = currentShield / MaxShield;
				if (pct <= 0.05f)
					bgStyle.BgColor = new Color(0.08f, 0.25f, 0.65f);
				else
					bgStyle.BgColor = new Color(0.15f, 0.35f, 0.75f);
			}
		}

		// Health Bar
		if (healthBar != null)
		{
			StyleBoxFlat fillStyle = healthBar.GetThemeStylebox("fill") as StyleBoxFlat;
			if (fillStyle != null)
			{
				float pct = currentHealth / MaxHealth;
				if (pct > 0.6f)
					fillStyle.BgColor = Colors.LimeGreen;
				else if (pct > 0.3f)
					fillStyle.BgColor = Colors.Yellow;
				else
					fillStyle.BgColor = Colors.Red;
			}

			StyleBoxFlat bgStyle = healthBar.GetThemeStylebox("background") as StyleBoxFlat;
			if (bgStyle != null)
			{
				float pct = currentHealth / MaxHealth;
				if (pct <= 0.05f)
					bgStyle.BgColor = Colors.DarkRed;
				else
					bgStyle.BgColor = new Color(0.2f, 0.2f, 0.2f);
			}
		}

		// Icons
		if (shieldIcon != null && shieldIcon.SpriteFrames != null)
		{
			string target = "shield_full";
			if (currentShield <= 0) target = "shield_broken";
			else if (currentShield <= 25) target = "shield_25";
			else if (currentShield <= 50) target = "shield_50";
			else if (currentShield <= 75) target = "shield_75";

			if (shieldIcon.Animation != target)
				shieldIcon.Play(target);
		}

		if (heartIcon != null && heartIcon.SpriteFrames != null)
		{
			string target = "heart_full";
			if (currentHealth <= 0) target = "heart_broken";
			else if (currentHealth <= 25) target = "heart_25";
			else if (currentHealth <= 50) target = "heart_50";
			else if (currentHealth <= 75) target = "heart_75";

			if (heartIcon.Animation != target)
				heartIcon.Play(target);
		}
	}
}
