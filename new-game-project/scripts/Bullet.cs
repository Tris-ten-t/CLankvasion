using Godot;

public partial class Bullet : RigidBody2D
{
	[Export] public float Lifetime = 5.0f;     // auto-destroy after X seconds
	[Export] public float Speed = 800f;         // should match tower's BulletSpeed

	public override void _Ready()
	{
		// Make bullet face its travel direction (optional – visual only)
		if (LinearVelocity.LengthSquared() > 0.1f)
		{
			LookAt(GlobalPosition + LinearVelocity);
			// If your bullet sprite faces UP in editor, uncomment:
			// Rotation -= Mathf.Pi / 2;
		}

		// Auto-destroy timer
		var timer = new Timer();
		timer.WaitTime = Lifetime;
		timer.OneShot = true;
		timer.Timeout += QueueFree;
		AddChild(timer);
		timer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		// Keep constant speed (useful if there's friction or collisions)
		if (LinearVelocity.LengthSquared() > 0.1f)
		{
			LinearVelocity = LinearVelocity.Normalized() * Speed;
		}

		// Optional: destroy if way off-screen
		if (GlobalPosition.Length() > 3000)
			QueueFree();
	}
}
