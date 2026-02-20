using Godot;

public partial class MainTower : AnimatedSprite2D
{
	[Export] public PackedScene BulletScene;
	[Export] public float FireRate = 0.5f;
	[Export] public float BulletSpeed = 800f;

	[Export] public string IdleAnimation = "idle";
	[Export] public string ShootAnimation = "shoot";
	[Export] public float ShootFlashDuration = 0.15f;

	private double lastFireTime = 0.0;
	private Marker2D muzzle;
	private Timer shootFlashTimer;

	public override void _Ready()
	{
		muzzle = GetNodeOrNull<Marker2D>("Muzzle");
		if (muzzle == null)
		{
			GD.Print("Creating automatic Muzzle node");
			muzzle = new Marker2D();
			muzzle.Name = "Muzzle";
			muzzle.Position = new Vector2(30, 0);   // adjust to your barrel tip
			AddChild(muzzle);
		}

		shootFlashTimer = new Timer();
		shootFlashTimer.OneShot = true;
		shootFlashTimer.Timeout += () => Play(IdleAnimation);
		AddChild(shootFlashTimer);

		Play(IdleAnimation);
	}

	public override void _Process(double delta)
	{
		Vector2 mousePos = GetGlobalMousePosition();
		LookAt(mousePos);
		Rotation -= Mathf.Pi / 2;   // -90° offset for upward-facing sprite

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
			GD.PrintErr("BulletScene export is not set!");
			return;
		}

		var bullet = BulletScene.Instantiate<RigidBody2D>();
		if (bullet == null)
		{
			GD.PrintErr("Failed to instantiate bullet");
			return;
		}

		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = muzzle.GlobalPosition;

		Vector2 direction = (targetPos - muzzle.GlobalPosition).Normalized();
		bullet.LinearVelocity = direction * BulletSpeed;

		Play(ShootAnimation);
		shootFlashTimer.Start(ShootFlashDuration);
	}
}
