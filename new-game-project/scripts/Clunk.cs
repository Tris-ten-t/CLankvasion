using Godot;

public partial class Clunk : CharacterBody2D, IDamageable
{
	[Export] public float Speed = 58f;
	[Export] public int MaxHealth = 20;
	[Export] public string WalkAnimation = "walk";
	[Export] public string DeathAnimation = "death";
	[Export] public int DamageToTower = 10;

	private int _currentHealth;
	private ProgressBar _healthBarInstance;
	private AnimatedSprite2D _animatedSprite;
	private Node2D _tower;
	private bool _isDead = false;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");

		if (_animatedSprite == null)
		{
			GD.Print("[Clunk] ERROR: No Sprite found!");
			return;
		}

		_animatedSprite.Rotation = 0f;
		_animatedSprite.FlipV = false;

		if (_animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation(WalkAnimation))
		{
			_animatedSprite.Play(WalkAnimation);
		}

		_animatedSprite.AnimationFinished += OnAnimationFinished;

		var template = GetTree().Root.GetNodeOrNull<ProgressBar>("Area/HealthBarTemplate");
		if (template != null)
		{
			_healthBarInstance = (ProgressBar)template.Duplicate();
			GetTree().CurrentScene.AddChild(_healthBarInstance);
			_healthBarInstance.Visible = true;
			_healthBarInstance.ZIndex = 10;
			_healthBarInstance.Rotation = Mathf.Pi / 2;
		}

		_tower = GetTree().GetFirstNodeInGroup("towers") as Node2D;

		UpdateHealthBar();
		GD.Print("[Clunk] Spawned and ready. Monitoring distance to tower...");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead || _tower == null) 
		{
			Velocity = Vector2.Zero;
			return;
		}

		Vector2 direction = (_tower.GlobalPosition - GlobalPosition).Normalized();
		Velocity = direction * Speed;
		MoveAndSlide();

		if (_animatedSprite != null)
			_animatedSprite.FlipH = direction.X < 0;

		// Health bar
		if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
		{
			_healthBarInstance.Rotation = Mathf.Pi / 2;
			Vector2 offset = new Vector2(-50, -40);
			_healthBarInstance.GlobalPosition = GlobalPosition + offset;
		}

		// NEW: Simple distance check - much more reliable than signals
		float distanceToTower = GlobalPosition.DistanceTo(_tower.GlobalPosition);
		if (distanceToTower < 40f)   // Adjust this number if needed (40 = roughly enemy size)
		{
			GD.Print("[Clunk] Close enough to tower! Dealing damage...");
			if (_tower is MainTower tower)
			{
				tower.TakeDamage(DamageToTower);
			}
			_isDead = true;
			Die();
		}
	}

	public void TakeDamage(int damage)
	{
		if (_isDead) return;

		_currentHealth -= damage;
		if (_currentHealth < 0) _currentHealth = 0;
		UpdateHealthBar();

		if (_currentHealth <= 0)
		{
			_isDead = true;
			Die();
		}
	}

	private void Die()
	{
		GD.Print("[Clunk] Starting death animation");
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

	private void OnAnimationFinished()
	{
		if (_animatedSprite.Animation == DeathAnimation)
		{
			CleanupAndDie();
		}
	}

	private void CleanupAndDie()
	{
		GD.Print("[Clunk] Enemy removed");
		if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
			_healthBarInstance.QueueFree();

		QueueFree();
	}

	private void UpdateHealthBar()
	{
		if (_healthBarInstance == null || !IsInstanceValid(_healthBarInstance)) return;

		float healthPct = (float)_currentHealth / MaxHealth;
		_healthBarInstance.Value = Mathf.Lerp(0, 100, healthPct);

		Color barColor = healthPct > 0.6f ? Colors.Green : healthPct > 0.3f ? Colors.Yellow : Colors.Red;

		StyleBoxFlat currentStyle = _healthBarInstance.GetThemeStylebox("fill") as StyleBoxFlat;
		StyleBoxFlat newStyle = currentStyle != null ? (StyleBoxFlat)currentStyle.Duplicate() : new StyleBoxFlat();
		newStyle.BgColor = barColor;
		_healthBarInstance.AddThemeStyleboxOverride("fill", newStyle);
	}
}
