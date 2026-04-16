using Godot;

public partial class Emp : Area2D
{
	[Export] public float StunDuration = 5.0f;
	[Export] public float EffectRadius = 140f;

	private AnimatedSprite2D animatedSprite;
	private bool hasBeenUsed = false;

	public override void _Ready()
	{
		ZIndex = -5;

		animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (animatedSprite != null && animatedSprite.SpriteFrames.HasAnimation("Idle"))
			animatedSprite.Play("Idle");

		BodyEntered += OnBodyEntered;

		GD.Print("[Emp] Deployed and ready");
	}

	private void OnBodyEntered(Node body)
	{
		if (hasBeenUsed) return;

		if (body is CharacterBody2D enemy && enemy.IsInGroup("enemies"))
		{
			GD.Print($"[Emp] Triggered by {enemy.Name} - stunning area!");
			StunEnemiesInArea();
			hasBeenUsed = true;

			if (animatedSprite != null && animatedSprite.SpriteFrames.HasAnimation("Boom"))
			{
				animatedSprite.Play("Boom");
				animatedSprite.AnimationFinished += () => QueueFree();
			}
			else
			{
				QueueFree();
			}
		}
	}

	private void StunEnemiesInArea()
	{
		var space = GetWorld2D().DirectSpaceState;
		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = new CircleShape2D { Radius = EffectRadius },
			Transform = GlobalTransform,
			CollisionMask = 2,
		};

		var hits = space.IntersectShape(query);

		int stunnedCount = 0;
		foreach (var hit in hits)
		{
			if (hit["collider"].AsGodotObject() is CharacterBody2D enemy && enemy.IsInGroup("enemies"))
			{
				if (enemy.HasMethod("Stun"))
				{
					enemy.Call("Stun", StunDuration);
					stunnedCount++;
				}
			}
		}

		GD.Print($"[Emp] Stunned {stunnedCount} enemies for {StunDuration} seconds");
	}
}
