using Godot;

public partial class WaterTank : Area2D
{
	[Export] public int DamagePerTick = 2;
	[Export] public float TickInterval = 1.5f;
	[Export] public float Lifetime = 25.0f;
	[Export] public float SlowDuration = 3.0f;
	[Export] public float SlowMultiplier = 0.5f;

	private AnimatedSprite2D animatedSprite;
	private Timer damageTimer;
	private Timer lifetimeTimer;

	public override void _Ready()
	{
		ZIndex = -5;
		animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (animatedSprite != null && animatedSprite.SpriteFrames.HasAnimation("Idle"))
			animatedSprite.Play("Idle");

		damageTimer = new Timer();
		damageTimer.WaitTime = TickInterval;
		damageTimer.Autostart = true;
		damageTimer.Timeout += DealDamageTick;
		AddChild(damageTimer);

		lifetimeTimer = new Timer();
		lifetimeTimer.OneShot = true;
		lifetimeTimer.WaitTime = Lifetime;
		lifetimeTimer.Timeout += OnLifetimeExpired;
		AddChild(lifetimeTimer);
		lifetimeTimer.Start();

		GD.Print("[WaterTank] Deployed");
	}

	private void DealDamageTick()
	{
		var space = GetWorld2D().DirectSpaceState;
		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = new CircleShape2D { Radius = 110f },
			Transform = GlobalTransform,
			CollisionMask = 2,
		};

		var hits = space.IntersectShape(query);
		foreach (var hit in hits)
		{
			if (hit["collider"].AsGodotObject() is CharacterBody2D enemy && enemy.IsInGroup("enemies"))
			{
				// Deal damage
				if (enemy.HasMethod("TakeDamage"))
					enemy.Call("TakeDamage", DamagePerTick);

				// Apply slow
				if (enemy is BaseEnemy baseEnemy)
					baseEnemy.ApplySlow(SlowDuration, SlowMultiplier);
			}
		}
	}

	private void OnLifetimeExpired()
	{
		GD.Print("[WaterTank] Lifetime expired - removing");
		QueueFree();
	}
}
