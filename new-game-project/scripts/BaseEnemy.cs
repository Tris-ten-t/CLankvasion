using Godot;

public enum EnemyType
{
	Roller,
	Clank,
	Bull,
	Clink,
	Clunk
}

public abstract partial class BaseEnemy : CharacterBody2D, IDamageable
{
	[Export] public float Speed = 60f;
	[Export] public int MaxHealth = 3;
	[Export] public string WalkAnimation = "walk";
	[Export] public string DeathAnimation = "death";
	[Export] public int DamageToTower = 5;

	public abstract EnemyType EnemyType { get; }
	public abstract int CoinValue { get; }
	public abstract Vector2 HealthBarOffset { get; }
	public abstract float HealthBarRotation { get; }

	// Custom signal
	[Signal] public delegate void EnemyDiedEventHandler(int enemyType, int coinsAwarded);

	protected int _currentHealth;
	protected ProgressBar _healthBarInstance;
	protected AnimatedSprite2D _animatedSprite;
	protected Node2D _tower;
	protected bool _isDying = false;
	protected bool isStunned = false;
	protected double stunEndTime = 0.0;

	public override void _Ready()
	{
		var deathParticles = GetNodeOrNull<GpuParticles2D>("DeathParticles");
		if (deathParticles != null)
		deathParticles.Emitting = false;
		_currentHealth = MaxHealth;
		_animatedSprite = GetNode<AnimatedSprite2D>("Sprite");

		if (_animatedSprite != null)
		{
			_animatedSprite.Rotation = 0f;
			_animatedSprite.FlipV = false;
		}

		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null)
		{
			if (_animatedSprite.SpriteFrames.HasAnimation(WalkAnimation))
				_animatedSprite.Play(WalkAnimation);
			_animatedSprite.AnimationFinished += OnAnimationFinished;
		}

		var template = GetTree().Root.GetNodeOrNull<ProgressBar>("Area/HealthBarTemplate");
		if (template != null)
		{
			_healthBarInstance = (ProgressBar)template.Duplicate();
			GetTree().CurrentScene.AddChild(_healthBarInstance);
			_healthBarInstance.Visible = true;
			_healthBarInstance.ZIndex = 10;
			_healthBarInstance.Rotation = HealthBarRotation;
		}

		_tower = GetTree().GetFirstNodeInGroup("towers") as Node2D;

		UpdateHealthBar();
		GD.Print($"[{EnemyType}] Spawned");
	}

	public void Stun(float duration)
	{
		isStunned = true;
		stunEndTime = Time.GetTicksMsec() / 1000.0 + duration;
		Velocity = Vector2.Zero;
		GD.Print($"[{Name}] Stunned for {duration} seconds");
	}

	protected bool CheckStun()
	{
		if (Time.GetTicksMsec() / 1000.0 > stunEndTime)
		{
			isStunned = false;
			return false;
		}
		Velocity = Vector2.Zero;
		return true;
	}

	protected bool CheckTowerDistance()
	{
		if (_tower == null) return false;
		float distanceToTower = GlobalPosition.DistanceTo(_tower.GlobalPosition);
		if (distanceToTower < 40f)
		{
			if (_tower is MainTower tower)
				tower.TakeDamage(DamageToTower);
			_isDying = true;
			Die();
			return true;
		}
		return false;
	}

	protected void UpdateHealthBarPosition()
	{
		if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
		{
			_healthBarInstance.Rotation = HealthBarRotation;
			_healthBarInstance.GlobalPosition = GlobalPosition + HealthBarOffset;
		}
	}

	public void TakeDamage(int damage)
	{
		if (_isDying) return;
		_currentHealth -= damage;
		if (_currentHealth < 0) _currentHealth = 0;
		UpdateHealthBar();
		if (_currentHealth <= 0)
		{
			_isDying = true;
			Die();
		}
	}

	protected void Die()
	{
		Velocity = Vector2.Zero;
		SetPhysicsProcess(false);

		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null &&
			_animatedSprite.SpriteFrames.HasAnimation(DeathAnimation))
		{
			_animatedSprite.Play(DeathAnimation);
		}
		else
		{
			CleanupAndDie();
		}
	}

	protected virtual void OnAnimationFinished()
	{
		if (_animatedSprite.Animation == DeathAnimation)
			CleanupAndDie();
	}

	protected void CleanupAndDie()
{
	// Spawn death particles before freeing
	var particles = GetNodeOrNull<GpuParticles2D>("DeathParticles");
	if (particles != null)
	{
		// Detach particles so they survive after enemy is freed
		RemoveChild(particles);
		GetTree().CurrentScene.AddChild(particles);
		particles.GlobalPosition = GlobalPosition;
		particles.Emitting = true;

		// Auto-clean particles after they finish
		var timer = new Timer();
		timer.OneShot = true;
		timer.WaitTime = particles.Lifetime * 2;
		timer.Timeout += () => particles.QueueFree();
		GetTree().CurrentScene.AddChild(timer);
		timer.Start();
	}

	// Emit custom signal
	EmitSignal(SignalName.EnemyDied, (int)EnemyType, CoinValue);
	Economy.AddCoins(CoinValue);

	if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
		_healthBarInstance.QueueFree();

	QueueFree();
}

	protected void UpdateHealthBar()
	{
		if (_healthBarInstance == null || !IsInstanceValid(_healthBarInstance)) return;

		float healthPct = (float)_currentHealth / MaxHealth;
		_healthBarInstance.Value = Mathf.Lerp(0, 100, healthPct);

		Color barColor = healthPct > 0.6f ? Colors.Green : healthPct > 0.3f ? Colors.Yellow : Colors.Red;

		var currentStyle = _healthBarInstance.GetThemeStylebox("fill") as StyleBoxFlat;
		var newStyle = currentStyle != null ? (StyleBoxFlat)currentStyle.Duplicate() : new StyleBoxFlat();
		newStyle.BgColor = barColor;
		_healthBarInstance.AddThemeStyleboxOverride("fill", newStyle);
	}
}
