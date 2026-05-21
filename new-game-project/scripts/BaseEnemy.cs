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

	[Signal] public delegate void EnemyDiedEventHandler(int enemyType, int coinsAwarded);

	protected int _currentHealth;
	protected ProgressBar _healthBarInstance;
	protected AnimatedSprite2D _animatedSprite;
	protected Node2D _tower;
	protected bool _isDying = false;

	// Stun
	protected bool isStunned = false;
	protected double stunEndTime = 0.0;

	// Slow
	private bool isSlowed = false;
	private double slowEndTime = 0.0;
	private float _originalSpeed;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_originalSpeed = Speed; // Save original speed on spawn

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
		GD.Print($"[{EnemyType}] Spawned - Speed: {Speed}");
	}

	public void Stun(float duration)
	{
		isStunned = true;
		stunEndTime = Time.GetTicksMsec() / 1000.0 + duration;
		Velocity = Vector2.Zero;
		GD.Print($"[{Name}] Stunned for {duration} seconds");
	}

	public void ApplySlow(float duration, float speedMultiplier = 0.5f)
	{
		GD.Print($"[{Name}] ApplySlow called! Current speed: {Speed}, Original: {_originalSpeed}");

		if (!isSlowed)
			_originalSpeed = Speed;

		isSlowed = true;
		slowEndTime = Time.GetTicksMsec() / 1000.0 + duration;
		Speed = _originalSpeed * speedMultiplier;

		GD.Print($"[{Name}] Speed set to: {Speed}");
	}

	protected bool CheckModifiers()
	{
		double now = Time.GetTicksMsec() / 1000.0;

		// Handle stun
		if (isStunned)
		{
			if (now > stunEndTime)
				isStunned = false;
			else
			{
				Velocity = Vector2.Zero;
				return true;
			}
		}

		// Handle slow
		if (isSlowed)
		{
			if (now > slowEndTime)
			{
				isSlowed = false;
				Speed = _originalSpeed;
				GD.Print($"[{Name}] Slow expired, speed restored to {Speed}");
			}
		}

		return false;
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
