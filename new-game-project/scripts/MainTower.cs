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
	[Export] public float ShieldRegenRate = 5f;

	private double lastFireTime = 0.0;
	private Marker2D muzzle;
	private Timer shootFlashTimer;

	private float currentShield;
	private float currentHealth;

	// UI
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

		// UI references
		shieldBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldBar");
		healthBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/TowerStatus/StatusContainer/HealthContainer/HealthBar");
		shieldIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/ShieldContainer/ShieldIcon");
		heartIcon = GetTree().CurrentScene.GetNodeOrNull<AnimatedSprite2D>("UI/TowerStatus/StatusContainer/HealthContainer/HeartIcon");

		UpdateUI();
	}

	public override void _Process(double delta)
	{
		Vector2 mousePos = GetGlobalMousePosition();
		LookAt(mousePos);
		Rotation -= Mathf.Pi / 2;

		if (Input.IsMouseButtonPressed(MouseButton.Left))
			TryFire(mousePos);

		// Shield regen
		if (currentShield < MaxShield)
		{
			currentShield += ShieldRegenRate * (float)delta;
			if (currentShield > MaxShield) currentShield = MaxShield;
			UpdateUI();
		}
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

	// === NEW: Take Damage from Enemies ===
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
			// TODO: Add Game Over screen later
		}
	}

	private void UpdateUI()
	{
		if (shieldBar != null) shieldBar.Value = currentShield;
		if (healthBar != null) healthBar.Value = currentHealth;

		// Shield Icon Animation
		if (shieldIcon != null && shieldIcon.SpriteFrames != null)
		{
			if (currentShield > 75) shieldIcon.Play("shield_full");
			else if (currentShield > 50) shieldIcon.Play("shield_75");
			else if (currentShield > 25) shieldIcon.Play("shield_50");
			else if (currentShield > 0) shieldIcon.Play("shield_25");
			else shieldIcon.Play("shield_broken");
		}

		// Heart Icon Animation
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
