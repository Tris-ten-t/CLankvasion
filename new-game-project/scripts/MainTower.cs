using Godot;

public partial class MainTower : AnimatedSprite2D
{
	// Exported variables – adjustable in the Inspector
	[Export] public PackedScene BulletScene;
	[Export] public float FireRate = 0.5f;          // seconds between shots
	[Export] public float BulletSpeed = 800f;       // pixels/second

	// Animation names – change these if your SpriteFrames use different names
	[Export] public string IdleAnimation = "idle";
	[Export] public string ShootAnimation = "shoot";
	[Export] public float ShootFlashDuration = 0.15f;  // how long the smoke frame shows

	// Internal state
	private double lastFireTime = 0.0;
	private Marker2D muzzle;
	private Timer shootFlashTimer;

	public override void _Ready()
	{
		// Setup muzzle (barrel tip) – create if missing
		muzzle = GetNodeOrNull<Marker2D>("Muzzle");
		if (muzzle == null)
		{
			GD.Print("Creating automatic Muzzle node");
			muzzle = new Marker2D();
			muzzle.Name = "Muzzle";
			muzzle.Position = new Vector2(30, 0);   // ← adjust this value to match your sprite's barrel length
			AddChild(muzzle);
		}

		// Setup timer for shoot flash (smoke → idle)
		shootFlashTimer = new Timer();
		shootFlashTimer.OneShot = true;
		shootFlashTimer.Timeout += () => Play(IdleAnimation);
		AddChild(shootFlashTimer);

		// Start in idle state
		Play(IdleAnimation);
	}

	public override void _Process(double delta)
	{
		// Aim at mouse cursor
		Vector2 mousePos = GetGlobalMousePosition();
		LookAt(mousePos);

		// Offset because your sprite is drawn facing up (most common top-down case)
		Rotation -= Mathf.Pi / 2;   // -90 degrees

		// Fire while holding left mouse button
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			TryFire(mousePos);
		}
	}

	private void TryFire(Vector2 targetPos)
	{
		double now = Time.GetTicksMsec() / 1000.0;
		if (now - lastFireTime < FireRate)
			return;

		lastFireTime = now;

		if (BulletScene == null)
		{
			GD.PrintErr("BulletScene export is not set! Drag your Bullet.tscn into the Inspector.");
			return;
		}

		// Spawn bullet
		var bullet = BulletScene.Instantiate<RigidBody2D>();
		if (bullet == null)
		{
			GD.PrintErr("Failed to instantiate bullet scene");
			return;
		}

		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = muzzle.GlobalPosition;

		Vector2 direction = (targetPos - muzzle.GlobalPosition).Normalized();
		bullet.LinearVelocity = direction * BulletSpeed;

		// Play shoot animation (smoke puff)
		Play(ShootAnimation);
		shootFlashTimer.Start(ShootFlashDuration);

		// Optional debug output
		// GD.Print($"Shot fired at {now:F2}s from {muzzle.GlobalPosition}");
	}
}
