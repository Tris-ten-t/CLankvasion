using Godot;

public partial class Mine : Area2D
{
	[Export] public int Damage = 40;
	[Export] public float ExplosionRadius = 180f;     // Bigger default
	[Export] public float ArmTime = 0.6f;

	private AnimatedSprite2D animatedSprite;
	private bool isArmed = false;

	public override void _Ready()
	{
		animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (animatedSprite == null)
		{
			GD.PrintErr("[Mine] AnimatedSprite2D not found!");
			return;
		}

		// Primary method: Signal
		BodyEntered += OnBodyEntered;

		// Play idle
		if (animatedSprite.SpriteFrames.HasAnimation("Idle"))
			animatedSprite.Play("Idle");

		// Arm the mine
		var timer = new Timer();
		timer.OneShot = true;
		timer.WaitTime = ArmTime;
		timer.Timeout += () => { isArmed = true; GD.Print("[Mine] ARMED"); };
		AddChild(timer);
		timer.Start();

		GD.Print("[Mine] Placed - radius: " + ExplosionRadius);
	}

	// Backup method: Check every physics frame (very reliable)
	public override void _PhysicsProcess(double delta)
	{
		if (!isArmed) return;

		var space = GetWorld2D().DirectSpaceState;
		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = new CircleShape2D { Radius = ExplosionRadius },
			Transform = GlobalTransform,
			CollisionMask = 2,           // Change this number if your enemies are on a different layer
		};

		var hits = space.IntersectShape(query, 32);

		foreach (var hit in hits)
		{
			if (hit["collider"].AsGodotObject() is CharacterBody2D enemy && enemy.IsInGroup("enemies"))
			{
				GD.Print($"[Mine] Backup detection triggered by {enemy.Name}");
				TriggerExplosion(enemy);
				return;
			}
		}
	}

	private void OnBodyEntered(Node body)
	{
		GD.Print($"[Mine] Signal triggered by {body.Name}");
		if (isArmed && body is CharacterBody2D enemy && enemy.IsInGroup("enemies"))
		{
			TriggerExplosion(enemy);
		}
	}

	private void TriggerExplosion(CharacterBody2D enemy)
	{
		GD.Print($"[Mine] 💥 EXPLODING on {enemy.Name}!");

		if (enemy.HasMethod("TakeDamage"))
			enemy.Call("TakeDamage", Damage);

		if (animatedSprite != null && animatedSprite.SpriteFrames.HasAnimation("Boom"))
		{
			animatedSprite.Play("Boom");
			animatedSprite.AnimationFinished += OnBoomFinished;
		}
		else
		{
			QueueFree();
		}
	}

	private void OnBoomFinished()
	{
		if (animatedSprite.Animation == "Boom")
			QueueFree();
	}
}
