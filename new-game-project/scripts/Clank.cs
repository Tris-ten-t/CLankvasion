using Godot;

public partial class Clank : CharacterBody2D, IDamageable
{
	[Export] public float Speed = 45f;
	[Export] public int MaxHealth = 10;
	[Export] public string WalkAnimation = "walk";
	[Export] public string DeathAnimation = "death";

	private int _currentHealth;
	private ProgressBar _healthBarInstance;
	private AnimatedSprite2D _animatedSprite;
	private Vector2 _targetPos;
	private Node2D _tower;
	private bool _isDying = false;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_animatedSprite = GetNode<AnimatedSprite2D>("Sprite");

		// Force upright
		if (_animatedSprite != null)
		{
			_animatedSprite.Rotation = 0f;
			_animatedSprite.FlipV = false;
		}

		// Setup animations
		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null)
		{
			// Start walking immediately if the animation exists
			if (_animatedSprite.SpriteFrames.HasAnimation(WalkAnimation))
			{
				_animatedSprite.Play(WalkAnimation);
			}

			// Connect finished signal once — we'll handle both walk (loop) and death (one-shot) here
			_animatedSprite.AnimationFinished += OnAnimationFinished;
		}

		// Health bar
		var template = GetTree().Root.GetNodeOrNull<ProgressBar>("Area/HealthBarTemplate");
		if (template != null)
		{
			_healthBarInstance = (ProgressBar)template.Duplicate();
			GetTree().CurrentScene.AddChild(_healthBarInstance);
			_healthBarInstance.Visible = true;
			_healthBarInstance.ZIndex = 10;
			_healthBarInstance.Rotation = Mathf.Pi / 2;  // horizontal
		}
		else
		{
			GD.Print("ERROR: HealthBarTemplate not found");
		}

		_tower = GetTree().GetFirstNodeInGroup("towers") as Node2D;
		if (_tower != null)
			_targetPos = _tower.GlobalPosition;

		UpdateHealthBar();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDying || _tower == null) return;

		Vector2 direction = (_targetPos - GlobalPosition).Normalized();
		Velocity = direction * Speed;
		MoveAndSlide();

		// Upright sprite + horizontal flip only
		if (_animatedSprite != null && direction.LengthSquared() > 0.1f)
		{
			_animatedSprite.FlipH = direction.X < 0;
			_animatedSprite.FlipV = false;
			_animatedSprite.Rotation = 0f;
		}

		// Health bar
		if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
		{
			_healthBarInstance.Rotation = Mathf.Pi / 2;
			Vector2 offset = new Vector2(-50, -40);  // tune this
			_healthBarInstance.GlobalPosition = GlobalPosition + offset;
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

	private void Die()
	{
		Velocity = Vector2.Zero;

		if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
		{
			_healthBarInstance.QueueFree();
			_healthBarInstance = null;
		}

		// Switch to death animation
		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null &&
			_animatedSprite.SpriteFrames.HasAnimation(DeathAnimation))
		{
			_animatedSprite.Play(DeathAnimation);
			// No need to stop walk here — Play() automatically switches
		}
		else
		{
			QueueFree();  // fallback
		}
	}

	private void OnAnimationFinished()
	{
		// Called when ANY animation finishes
		if (_animatedSprite.Animation == DeathAnimation)
		{
			// Death finished → remove enemy
			QueueFree();
		}
		else if (_animatedSprite.Animation == WalkAnimation)
		{
			// Walk finished → loop it again (in case loop is off in editor)
			if (!_isDying && _animatedSprite.SpriteFrames.HasAnimation(WalkAnimation))
			{
				_animatedSprite.Play(WalkAnimation);
			}
		}
	}

	private void UpdateHealthBar()
	{
		if (_healthBarInstance == null || !IsInstanceValid(_healthBarInstance)) return;

		float healthPct = (float)_currentHealth / MaxHealth;
		_healthBarInstance.Value = Mathf.Lerp(0, 100, healthPct);

		Color barColor = healthPct > 0.6f ? Colors.Green :
						 healthPct > 0.3f ? Colors.Yellow : Colors.Red;

		var currentStyle = _healthBarInstance.GetThemeStylebox("fill") as StyleBoxFlat;
		var newStyle = currentStyle != null ? (StyleBoxFlat)currentStyle.Duplicate() : new StyleBoxFlat();
		newStyle.BgColor = barColor;
		_healthBarInstance.AddThemeStyleboxOverride("fill", newStyle);
	}
}
