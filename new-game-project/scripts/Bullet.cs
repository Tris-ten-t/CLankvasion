using Godot;

public partial class Bullet : RigidBody2D
{
	[Export] public float Lifetime = 5.0f;
	[Export] public float Speed = 800f;  // optional constant speed enforcement

	private bool _hasSetRotation = false;

	public override void _Ready()
	{
		// Timer for auto-destroy
		var timer = new Timer();
		timer.WaitTime = Lifetime;
		timer.OneShot = true;
		timer.Timeout += QueueFree;
		AddChild(timer);
		timer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		// Enforce constant speed (optional but good for bullets)
		if (LinearVelocity.LengthSquared() > 0.1f)
		{
			LinearVelocity = LinearVelocity.Normalized() * Speed;
		}

		// Rotate only once, on the first physics frame (velocity is reliable here)
		if (!_hasSetRotation && LinearVelocity.LengthSquared() > 0.1f)
		{
			LookAt(GlobalPosition + LinearVelocity);

			// If after testing the line points 90° wrong (e.g. vertical line when shooting horizontal):
			// Try one of these offsets (uncomment only ONE at a time):
			// Rotation -= Mathf.Pi / 2;   // -90° — if your line was drawn facing UP in editor
			// Rotation += Mathf.Pi / 2;   // +90° — if facing DOWN
			// Rotation += Mathf.Pi;       // 180° — if facing LEFT

			_hasSetRotation = true;
		}

		// Optional: off-screen cleanup
		if (GlobalPosition.Length() > 3000)
			QueueFree();
	}
}
