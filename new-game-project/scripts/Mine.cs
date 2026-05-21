using Godot;

public partial class Mine : Area2D
{
	[Export] public int Damage = 40;
	[Export] public float ExplosionRadius = 180f;
	[Export] public float ArmTime = 0.6f;

	private AnimatedSprite2D animatedSprite;
	private bool isArmed = false;
	private bool isExploding = false;   // ← NEW: Prevents multiple explosions

	public override void _Ready()
	{
		animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (animatedSprite == null)
		{
			GD.PrintErr("[Mine] AnimatedSprite2D not found!");
			return;
		}

		// Connect signal
		BodyEntered += OnBodyEntered;

		// Play idle animation
		if (animatedSprite.SpriteFrames.HasAnimation("Idle"))
			animatedSprite.Play("Idle");

		// Arm the mine after delay
		var timer = new Timer
		{
			OneShot = true,
			WaitTime = ArmTime
		};
		timer.Timeout += () => 
		{ 
			isArmed = true; 
			GD.Print("[Mine] ARMED"); 
		};
		AddChild(timer);
		timer.Start();

		GD.Print($"[Mine] Placed - Radius: {ExplosionRadius}");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!isArmed || isExploding) return;

		var space = GetWorld2D().DirectSpaceState;
		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = new CircleShape2D { Radius = ExplosionRadius },
			Transform = GlobalTransform,
			CollisionMask = 2, // Make sure this matches your enemy layer
		};

		var hits = space.IntersectShape(query, 32);

		foreach (var hit in hits)
		{
			if (hit["collider"].AsGodotObject() is CharacterBody2D enemy && enemy.IsInGroup("enemies"))
			{
				TriggerExplosion(enemy);
				return;
			}
		}
	}

	private void OnBodyEntered(Node body)
	{
		if (!isArmed || isExploding) return;

		if (body is CharacterBody2D enemy && enemy.IsInGroup("enemies"))
		{
			GD.Print($"[Mine] Signal triggered by {enemy.Name}");
			TriggerExplosion(enemy);
		}
	}

	private void TriggerExplosion(CharacterBody2D enemy)
	{
		if (isExploding) return;   // ← Safety check

		isExploding = true;

		GD.Print($"[Mine] 💥 EXPLODING on {enemy.Name}!");

		// Damage the enemy that triggered it
		if (enemy.HasMethod("TakeDamage"))
			enemy.Call("TakeDamage", Damage);

		// Play explosion animation
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
