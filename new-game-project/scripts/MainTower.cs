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

	// Upgrade system
	private int _towerLevel = 1;
	private float _baseDamage = 10f;
	private float _baseFireRate = 0.5f;
	private int _shieldCost = 30;
	private const float UpgradeMultiplier = 1.35f;
	private const float SpreadAngle = 15f;

	private readonly int[] _upgradeCosts = new int[] { 0, 50, 100, 200, 400 };

	public float CurrentDamage => _baseDamage * Mathf.Pow(UpgradeMultiplier, _towerLevel - 1);
	public float CurrentFireRate => _baseFireRate / Mathf.Pow(UpgradeMultiplier, _towerLevel - 1);
	private bool IsShotgunUnlocked => _towerLevel >= 3;

	private double lastFireTime = 0.0;
	private double lastShotgunTime = 0.0;
	private Marker2D muzzle;
	private Timer shootFlashTimer;

	private float currentShield;
	private float currentHealth;

	private ProgressBar shieldBar;
	private ProgressBar healthBar;
	private AnimatedSprite2D shieldIcon;
	private AnimatedSprite2D heartIcon;
	private Label coinLabel;
	private Label waveLabel;
	private Label levelLabel;
	private Label fireModeLabel;

	// Placement system
	private bool isPlacingMine = false;
	private bool isPlacingWaterTank = false;
	private bool isPlacingEmp = false;
	private bool isPlacingTrashCompactor = false;
	private Node2D _placementIndicator;
	private ColorRect _indicatorCircle;
	private Area2D _invalidZones;
	private bool _isValidPlacement = true;
	private Timer _flashTimer;
	private bool _wasLeftMousePressed = false;

	private bool gameOverTriggered = false;

	private PackedScene mineScene;
	private PackedScene waterTankScene;
	private PackedScene empScene;
	private PackedScene trashCompactorScene;

	public override void _Ready()
	{
		muzzle = GetNodeOrNull<Marker2D>("Muzzle");
		if (muzzle == null)
		{
			muzzle = new Marker2D { Name = "Muzzle", Position = new Vector2(30, 0) };
			AddChild(muzzle);
		}

		shootFlashTimer = new Timer { OneShot = true };
		shootFlashTimer.Timeout += () => Play(GetIdleAnimation());
		AddChild(shootFlashTimer);

		_flashTimer = new Timer { OneShot = true };
		_flashTimer.Timeout += OnFlashTimerTimeout;
		AddChild(_flashTimer);

		Play(GetIdleAnimation());

		currentShield = MaxShield;
		currentHealth = MaxHealth;

		shieldBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldBar");
		healthBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/HealthContainer/HealthBar");
		shieldIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldIcon");
		heartIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/HealthContainer/HeartIcon");
		coinLabel = GetTree().CurrentScene.GetNodeOrNull<Label>("UI/TowerStatus/CoinContainer/CoinLabel");
		waveLabel = GetTree().CurrentScene.GetNodeOrNull<Label>("UI/TowerStatus/WaveContainer/WaveLabel");
		levelLabel = GetTree().CurrentScene.GetNodeOrNull<Label>("UI/TowerStatus/LevelLabel");
		fireModeLabel = GetTree().CurrentScene.GetNodeOrNull<Label>("UI/TowerStatus/FireModeLabel");

		_placementIndicator = GetTree().CurrentScene.GetNodeOrNull<Node2D>("PlacementIndicator");
		if (_placementIndicator != null)
			_indicatorCircle = _placementIndicator.GetNodeOrNull<ColorRect>("IndicatorCircle");

		_invalidZones = GetTree().CurrentScene.GetNodeOrNull<Area2D>("InvalidZones");

		ForceSeparateStyles();
		UpdateUI();
	}

	private string GetIdleAnimation()
	{
		return _towerLevel switch
		{
			2 => "idle2",
			3 => "idle3",
			4 => "idle4",
			5 => "idle4",
			_ => "idle"
		};
	}

	private string GetShootAnimation()
	{
		return _towerLevel switch
		{
			2 => "shoot2",
			3 => "shoot3",
			4 => "shoot4",
			5 => "shoot4",
			_ => "shoot"
		};
	}

	public bool CanUpgrade() => _towerLevel < 5;
	public int GetUpgradeCost() => _towerLevel < 5 ? _upgradeCosts[_towerLevel] : 0;
	public int GetShieldCost() => _shieldCost;
	public int GetTowerLevel() => _towerLevel;

	public void UpgradeTower()
	{
		if (!CanUpgrade()) return;
		int cost = GetUpgradeCost();
		if (Economy.Coins < cost) return;

		Economy.AddCoins(-cost);
		_towerLevel++;
		FireRate = CurrentFireRate;
		Play(GetIdleAnimation());

		GD.Print($"Tower upgraded to level {_towerLevel}!");
		UpdateUI();
	}

	public void BuyShield()
	{
		if (Economy.Coins < _shieldCost) return;
		Economy.AddCoins(-_shieldCost);
		currentShield = Mathf.Min(currentShield + 25f, MaxShield);
		_shieldCost += 20;
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

	private bool IsPositionValid(Vector2 position)
{
	if (_invalidZones == null) return true;

	// Check each collision shape in the invalid zones
	foreach (Node child in _invalidZones.GetChildren())
	{
		if (child is CollisionShape2D shape && shape.Shape != null)
		{
			// Check if position is inside this shape
			if (shape.Shape is RectangleShape2D rect)
			{
				Vector2 localPos = _invalidZones.ToLocal(position);
				Vector2 shapePos = shape.Position;
				Vector2 extents = rect.Size / 2f;

				if (localPos.X >= shapePos.X - extents.X &&
					localPos.X <= shapePos.X + extents.X &&
					localPos.Y >= shapePos.Y - extents.Y &&
					localPos.Y <= shapePos.Y + extents.Y)
				{
					return false;
				}
			}
		}
	}
	return true;
}

	private void UpdatePlacementIndicator(Vector2 mousePos)
	{
		bool isPlacing = isPlacingMine || isPlacingWaterTank || isPlacingEmp || isPlacingTrashCompactor;

		if (_placementIndicator == null) return;

		if (!isPlacing)
		{
			_placementIndicator.Visible = false;
			return;
		}

		_placementIndicator.Visible = true;
		_placementIndicator.GlobalPosition = mousePos;

		_isValidPlacement = IsPositionValid(mousePos);

		if (_indicatorCircle != null)
			_indicatorCircle.Color = _isValidPlacement
				? new Color(0, 1, 0, 0.5f)
				: new Color(1, 0, 0, 0.5f);
	}

	private void FlashInvalidPlacement()
	{
		if (_indicatorCircle == null) return;
		_indicatorCircle.Color = new Color(1, 0, 0, 0.9f);
		_flashTimer.Start(0.2f);
		GD.Print("Invalid placement location!");
	}

	private void OnFlashTimerTimeout()
	{
		if (_indicatorCircle != null)
			_indicatorCircle.Color = new Color(1, 0, 0, 0.5f);
	}

	public override void _Process(double delta)
	{
		Vector2 mousePos = GetGlobalMousePosition();
		LookAt(mousePos);
		Rotation -= Mathf.Pi / 2;

		bool isPlacing = isPlacingMine || isPlacingWaterTank || isPlacingEmp || isPlacingTrashCompactor;
		bool leftMouseJustPressed = Input.IsMouseButtonPressed(MouseButton.Left) && !_wasLeftMousePressed;

		UpdatePlacementIndicator(mousePos);

		if (!isPlacing)
		{
			if (Input.IsMouseButtonPressed(MouseButton.Left))
				TryFire(mousePos);

			if (Input.IsMouseButtonPressed(MouseButton.Right) && IsShotgunUnlocked)
				TryShotgunFire(mousePos);
		}
		else
		{
			if (leftMouseJustPressed)
			{
				if (!_isValidPlacement)
				{
					FlashInvalidPlacement();
				}
				else
				{
					if (isPlacingMine) PlaceMine(mousePos);
					else if (isPlacingWaterTank) PlaceWaterTank(mousePos);
					else if (isPlacingEmp) PlaceEmp(mousePos);
					else if (isPlacingTrashCompactor) PlaceTrashCompactor(mousePos);
				}
			}
		}

		if (Input.IsActionJustPressed("open_shop") && !isPlacing)
			OpenShopMenu();

		if (Input.IsActionJustPressed("ui_cancel") && isPlacing)
			CancelPlacement();

		// Track left mouse state for next frame
		_wasLeftMousePressed = Input.IsMouseButtonPressed(MouseButton.Left);

		UpdateUI();
	}

	private void TryFire(Vector2 targetPos)
	{
		double now = Time.GetTicksMsec() / 1000.0;
		if (now - lastFireTime < FireRate) return;
		lastFireTime = now;

		FireBullet(targetPos, 0f);
		Play(GetShootAnimation());
		shootFlashTimer.Start(ShootFlashDuration);
	}

	private void TryShotgunFire(Vector2 targetPos)
	{
		double now = Time.GetTicksMsec() / 1000.0;
		if (now - lastShotgunTime < FireRate * 1.5f) return;
		lastShotgunTime = now;

		FireBullet(targetPos, 0f);
		FireBullet(targetPos, SpreadAngle);
		FireBullet(targetPos, -SpreadAngle);

		Play(GetShootAnimation());
		shootFlashTimer.Start(ShootFlashDuration);
	}

	private void FireBullet(Vector2 targetPos, float angleOffset)
	{
		if (BulletScene == null) return;

		var bullet = BulletScene.Instantiate<RigidBody2D>();
		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = muzzle.GlobalPosition;

		Vector2 direction = (targetPos - muzzle.GlobalPosition).Normalized();

		if (angleOffset != 0f)
		{
			float rad = Mathf.DegToRad(angleOffset);
			direction = new Vector2(
				direction.X * Mathf.Cos(rad) - direction.Y * Mathf.Sin(rad),
				direction.X * Mathf.Sin(rad) + direction.Y * Mathf.Cos(rad)
			);
		}

		bullet.LinearVelocity = direction * BulletSpeed;

		if (bullet.HasMethod("SetDamage"))
			bullet.Call("SetDamage", (int)CurrentDamage);
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
		}
	}

	public void StartMinePlacement()
	{
		ResetPlacement();
		isPlacingMine = true;
		mineScene = GD.Load<PackedScene>("res://scenes/Towers/Mine.tscn");
	}

	public void StartWaterTankPlacement()
	{
		ResetPlacement();
		isPlacingWaterTank = true;
		waterTankScene = GD.Load<PackedScene>("res://scenes/Towers/WaterTank.tscn");
	}

	public void StartEmpPlacement()
	{
		ResetPlacement();
		isPlacingEmp = true;
		empScene = GD.Load<PackedScene>("res://scenes/Towers/Emp.tscn");
	}

	public void StartTrashCompactorPlacement()
	{
		ResetPlacement();
		isPlacingTrashCompactor = true;
		trashCompactorScene = GD.Load<PackedScene>("res://scenes/Towers/TrashCompactor.tscn");
	}

	private void ResetPlacement()
	{
		isPlacingMine = false;
		isPlacingWaterTank = false;
		isPlacingEmp = false;
		isPlacingTrashCompactor = false;
	}

	private void PlaceMine(Vector2 position)
	{
		if (mineScene == null) return;
		var mine = mineScene.Instantiate<Area2D>();
		GetTree().CurrentScene.AddChild(mine);
		mine.GlobalPosition = position;
		isPlacingMine = false;
		if (_placementIndicator != null) _placementIndicator.Visible = false;
	}

	private void PlaceWaterTank(Vector2 position)
	{
		if (waterTankScene == null) return;
		var tank = waterTankScene.Instantiate<Area2D>();
		GetTree().CurrentScene.AddChild(tank);
		tank.GlobalPosition = position;
		isPlacingWaterTank = false;
		if (_placementIndicator != null) _placementIndicator.Visible = false;
	}

	private void PlaceEmp(Vector2 position)
	{
		if (empScene == null) return;
		var emp = empScene.Instantiate<Area2D>();
		GetTree().CurrentScene.AddChild(emp);
		emp.GlobalPosition = position;
		isPlacingEmp = false;
		if (_placementIndicator != null) _placementIndicator.Visible = false;
	}

	private void PlaceTrashCompactor(Vector2 position)
	{
		if (trashCompactorScene == null) return;
		var compactor = trashCompactorScene.Instantiate<Area2D>();
		GetTree().CurrentScene.AddChild(compactor);
		compactor.GlobalPosition = position;
		isPlacingTrashCompactor = false;
		if (_placementIndicator != null) _placementIndicator.Visible = false;
	}

	private void CancelPlacement()
	{
		ResetPlacement();
		if (_placementIndicator != null)
			_placementIndicator.Visible = false;
		GD.Print("Placement cancelled");
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
		if (waveLabel != null) waveLabel.Text = $"Wave: {GameData.Instance.CurrentWave + 1}";
		if (levelLabel != null) levelLabel.Text = $"Tower Lvl: {_towerLevel}";

		if (fireModeLabel != null)
			fireModeLabel.Text = IsShotgunUnlocked
				? "LMB: Single | RMB: Shotgun"
				: "Shotgun unlocks at level 3";

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
