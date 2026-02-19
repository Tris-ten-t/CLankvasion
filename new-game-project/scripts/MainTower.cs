using Godot;

public partial class MainTower : AnimatedSprite2D
{
	// Exported variables – visible & editable in the Inspector
	[Export] public PackedScene BulletScene;
	[Export] public float FireRate = 0.5f;      // seconds between shots
	[Export] public float BulletSpeed = 800f;   // pixels per second

	// Class-level variables
	private double lastFireTime = 0.0;
	private Marker2D muzzle;

	public override void _Ready()
	{
		// Get or create the Muzzle marker (where bullets spawn from)
		muzzle = GetNodeOrNull<Marker2D>("Muzzle");
		if (muzzle == null)
		{
			GD.Print("Muzzle node not found – creating one automatically");
			muzzle = new Marker2D();
			muzzle.Name = "Muzzle";
			muzzle.Position = new Vector2(30, 0);   // ← change this to match your barrel/tip length
			AddChild(muzzle);
		}
	}

	public override void _Process(double delta)
	{
		// Get mouse position in world coordinates
		Vector2 mousePos = GetGlobalMousePosition();

		// Make the sprite look at the mouse (makes +X axis point toward mouse)
		LookAt(mousePos);

		// IMPORTANT FIX: your sprite was -90° off → most top-down tower sprites face UP by default
		Rotation -= Mathf.Pi / 2;   // -90 degrees → front now points correctly at mouse

		// Optional: if you ever want the opposite direction, use +90° instead
		// Rotation += Mathf.Pi / 2;

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
			GD.PrintErr("BulletScene is not assigned in the Inspector!");
			return;
		}

		// Spawn the bullet instance
		var bullet = BulletScene.Instantiate<RigidBody2D>();
		if (bullet == null)
		{
			GD.PrintErr("Failed to instantiate bullet – check that Bullet.tscn is valid");
			return;
		}

		// Add it to the current scene (so it moves in world space)
		GetTree().CurrentScene.AddChild(bullet);

		// Set starting position to the muzzle (barrel tip)
		bullet.GlobalPosition = muzzle.GlobalPosition;

		// Calculate direction toward where the mouse was when fired
		Vector2 direction = (targetPos - muzzle.GlobalPosition).Normalized();
		bullet.LinearVelocity = direction * BulletSpeed;

		// Debug print (visible in Output panel) – remove later if you want
		GD.Print($"Bullet fired from {muzzle.GlobalPosition} toward {targetPos}");
	}
}
