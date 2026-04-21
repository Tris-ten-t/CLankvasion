using Godot;

public partial class TrashCompactor : Area2D
{
	[Export] public int Cost = 120;
	[Export] public float PullStrength = 800f;
	[Export] public float KillDelay = 0.6f;

	private AnimatedSprite2D animatedSprite;
	private bool hasBeenUsed = false;
	private CharacterBody2D currentVictim;

	public override void _Ready()
	{
		ZIndex = -5;
		animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

		if (animatedSprite != null && animatedSprite.SpriteFrames.HasAnimation("Idle"))
			animatedSprite.Play("Idle");

		BodyEntered += OnBodyEntered;
		SetPhysicsProcess(false);

		GD.Print("[TrashCompactor] Deployed and ready");
	}

	private void OnBodyEntered(Node body)
	{
		if (hasBeenUsed) return;

		if (body is CharacterBody2D enemy && enemy.IsInGroup("enemies"))
		{
			GD.Print($"[TrashCompactor] Caught {enemy.Name} - pulling in!");
			currentVictim = enemy;
			hasBeenUsed = true;

			// Disable enemy's own AI so it doesn't fight the pull
			enemy.SetPhysicsProcess(false);
			enemy.SetProcess(false);

			if (animatedSprite != null && animatedSprite.SpriteFrames.HasAnimation("Activate"))
				animatedSprite.Play("Activate");

			var pullTimer = new Timer();
			pullTimer.OneShot = true;
			pullTimer.WaitTime = KillDelay;
			pullTimer.Timeout += KillVictim;
			AddChild(pullTimer);
			pullTimer.Start();

			SetPhysicsProcess(true);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (currentVictim == null || !IsInstanceValid(currentVictim))
		{
			SetPhysicsProcess(false);
			return;
		}

		// Pull enemy toward compactor
		Vector2 direction = (GlobalPosition - currentVictim.GlobalPosition).Normalized();
		currentVictim.Velocity = direction * PullStrength;
		currentVictim.MoveAndSlide();
	}

	private void KillVictim()
	{
		if (currentVictim != null && IsInstanceValid(currentVictim))
		{
			GD.Print($"[TrashCompactor] Eliminated {currentVictim.Name}");

			// Use the enemy's own death pipeline so health bar gets cleaned up
			// and coins are awarded properly
			if (currentVictim is IDamageable damageable)
				damageable.TakeDamage(99999);
			else
				currentVictim.QueueFree(); // fallback if not IDamageable
		}

		SetPhysicsProcess(false);

		if (animatedSprite != null && animatedSprite.SpriteFrames.HasAnimation("Death"))
		{
			animatedSprite.Play("Death");
			animatedSprite.AnimationFinished += () => QueueFree();
		}
		else
		{
			QueueFree();
		}
	}
}
