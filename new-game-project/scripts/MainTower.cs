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

		// Find UI
		shieldBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldBar");
		healthBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/HealthContainer/HealthBar");
		shieldIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldIcon");
		heartIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/HealthContainer/HeartIcon");

		// Force completely separate styles
		ForceSeparateStyles();

		UpdateUI();
	}

	private void ForceSeparateStyles()
	{
		// Shield Bar - Force new style
		if (shieldBar != null)
		{
			var style = new StyleBoxFlat();
			shieldBar.AddThemeStyleboxOverride("fill", style);
		}

		// Health Bar - Force new style
		if (healthBar != null)
		{
			var style = new StyleBoxFlat();
			healthBar.AddThemeStyleboxOverride("fill", style);
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

		if (currentHealth <= 0)
		{
			GD.Print("Tower Destroyed - Game Over!");
		}
	}

	private void UpdateUI()
{
	if (shieldBar != null) shieldBar.Value = currentShield;
	if (healthBar != null) healthBar.Value = currentHealth;

	// ====================== SHIELD BAR ======================
	if (shieldBar != null)
	{
		StyleBoxFlat style = shieldBar.GetThemeStylebox("fill") as StyleBoxFlat;
		if (style == null)
		{
			style = new StyleBoxFlat();
			shieldBar.AddThemeStyleboxOverride("fill", style);
		}

		float pct = currentShield / MaxShield;

		// Light Blue (full) → Dark Blue (empty)
		Color lightBlue = new Color(0.31f, 0.78f, 0.97f);   // Starting light blue
		Color darkBlue  = new Color(0.08f, 0.25f, 0.65f);   // Fully dark blue when empty

		style.BgColor = lightBlue.Lerp(darkBlue, 1f - pct);

		// Border & rounding
		style.BorderWidthLeft = 2;
		style.BorderWidthTop = 2;
		style.BorderWidthRight = 2;
		style.BorderWidthBottom = 2;
		style.BorderColor = new Color(0.05f, 0.18f, 0.45f);
		style.CornerRadiusTopLeft = 8;
		style.CornerRadiusTopRight = 8;
		style.CornerRadiusBottomLeft = 8;
		style.CornerRadiusBottomRight = 8;
	}

	// ====================== HEALTH BAR ======================
	if (healthBar != null)
	{
		StyleBoxFlat style = healthBar.GetThemeStylebox("fill") as StyleBoxFlat;
		if (style == null)
		{
			style = new StyleBoxFlat();
			healthBar.AddThemeStyleboxOverride("fill", style);
		}

		float pct = currentHealth / MaxHealth;

		// Green (full) → Red (empty)
		Color green = Colors.LimeGreen;
		Color red   = Colors.Red;

		style.BgColor = green.Lerp(red, 1f - pct);

		// Border & rounding
		style.BorderWidthLeft = 2;
		style.BorderWidthTop = 2;
		style.BorderWidthRight = 2;
		style.BorderWidthBottom = 2;
		style.BorderColor = new Color(0.1f, 0.1f, 0.1f);
		style.CornerRadiusTopLeft = 8;
		style.CornerRadiusTopRight = 8;
		style.CornerRadiusBottomLeft = 8;
		style.CornerRadiusBottomRight = 8;
	}

	// Update Icons
	if (shieldIcon != null && shieldIcon.SpriteFrames != null)
	{
		if (currentShield > 75) shieldIcon.Play("shield_full");
		else if (currentShield > 50) shieldIcon.Play("shield_75");
		else if (currentShield > 25) shieldIcon.Play("shield_50");
		else if (currentShield > 0) shieldIcon.Play("shield_25");
		else shieldIcon.Play("shield_broken");
	}

	if (heartIcon != null && heartIcon.SpriteFrames != null)
	{
		if (currentHealth > 75) heartIcon.Play("heart_full");
		else if (currentHealth > 50) heartIcon.Play("heart_75");
		else if (currentHealth > 25) heartIcon.Play("heart_50");
		else if (currentHealth > 0) heartIcon.Play("heart_25");
		else heartIcon.Play("heart_broken");
		}
	}
}
