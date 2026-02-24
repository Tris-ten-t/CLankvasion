using Godot;

public partial class Enemy : CharacterBody2D, IDamageable
{
	[Export] public float Speed = 60f;
	[Export] public int MaxHealth = 3;
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

		// No forced rotation/flip here — we'll handle it dynamically

		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null)
		{
			if (_animatedSprite.SpriteFrames.HasAnimation(WalkAnimation))
			{
				_animatedSprite.Play(WalkAnimation);
			}

			_animatedSprite.AnimationFinished += OnAnimationFinished;
		}

		var template = GetTree().Root.GetNodeOrNull<ProgressBar>("Area/HealthBarTemplate");
		if (template != null)
		{
			_healthBarInstance = (ProgressBar)template.Duplicate();
			GetTree().CurrentScene.AddChild(_healthBarInstance);
			_healthBarInstance.Visible = true;
			_healthBarInstance.ZIndex = 10;
			_healthBarInstance.Rotation = -Mathf.Pi / 2;  // your preferred horizontal
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

		// Make the sprite face the movement direction
		if (_animatedSprite != null && direction.LengthSquared() > 0.1f)
		{
			// Point the sprite toward where it's moving
			_animatedSprite.LookAt(GlobalPosition + direction);

			// Because your animation rolls "up", add offset so "up" becomes "forward"
			// Test both +90° and -90° — one will make it roll toward the target
			_animatedSprite.Rotation += Mathf.Pi / 2;   // +90° — try this first
			// If rolling the wrong way, change to: _animatedSprite.Rotation += -Mathf.Pi / 2;
		}

		// Health bar
		if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
		{
			_healthBarInstance.Rotation = -Mathf.Pi / 2;

			// Adjust offset after -90° rotation
			Vector2 offset = new Vector2(30, -40);   // tune as needed
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

		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null &&
			_animatedSprite.SpriteFrames.HasAnimation(DeathAnimation))
		{
			_animatedSprite.Play(DeathAnimation);
			// Optional: reset rotation for death anim if it looks weird
			_animatedSprite.Rotation = 0f;
		}
		else
		{
			QueueFree();
		}
	}

	private void OnAnimationFinished()
	{
		if (_animatedSprite.Animation == DeathAnimation)
		{
			QueueFree();
		}
		else if (_animatedSprite.Animation == WalkAnimation && !_isDying)
		{
			if (_animatedSprite.SpriteFrames.HasAnimation(WalkAnimation))
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

		StyleBoxFlat currentStyle = _healthBarInstance.GetThemeStylebox("fill") as StyleBoxFlat;
		StyleBoxFlat newStyle = currentStyle != null ? (StyleBoxFlat)currentStyle.Duplicate() : new StyleBoxFlat();
		newStyle.BgColor = barColor;
		_healthBarInstance.AddThemeStyleboxOverride("fill", newStyle);
	}
}
