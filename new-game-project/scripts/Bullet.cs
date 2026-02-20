using Godot;

public partial class Bullet : RigidBody2D
{
	[Export] public float Lifetime = 5.0f;
	[Export] public float Speed = 800f;

	private bool _hasSetRotation = false;

	public override void _Ready()
	{
		// Enable contact monitoring for better signal reliability
		ContactMonitor = true;
		MaxContactsReported = 4;

		BodyEntered += OnBodyEntered;

		var timer = new Timer();
		timer.WaitTime = Lifetime;
		timer.OneShot = true;
		timer.Timeout += QueueFree;
		AddChild(timer);
		timer.Start();

		GD.Print("Bullet spawned");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (LinearVelocity.LengthSquared() > 0.1f)
		{
			LinearVelocity = LinearVelocity.Normalized() * Speed;
		}

		if (!_hasSetRotation && LinearVelocity.LengthSquared() > 0.1f)
		{
			LookAt(GlobalPosition + LinearVelocity);
			_hasSetRotation = true;
		}

		if (GlobalPosition.Length() > 3000)
			QueueFree();
	}

	private void OnBodyEntered(Node body)
	{
		GD.Print($"[Bullet] BodyEntered fired - hit: {body.Name} (type: {body.GetType().Name})");

		if (body is Enemy enemy)
		{
			GD.Print("[Bullet] → Confirmed Enemy - calling TakeDamage(1)");
			enemy.TakeDamage(1);
		}
		else
		{
			GD.Print("[Bullet] Hit something, but not an Enemy");
		}

		QueueFree();
	}
}
