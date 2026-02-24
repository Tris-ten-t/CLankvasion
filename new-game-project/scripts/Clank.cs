using Godot;

public partial class Clank : CharacterBody2D, IDamageable
{
	[Export] public float Speed = 45f;
	[Export] public int MaxHealth = 10;
	[Export] public string WalkAnimation = "walk";
	private int _currentHealth;
	private ProgressBar _healthBarInstance;
	private AnimatedSprite2D _animatedSprite;
	private Vector2 _targetPos;
	private Node2D _tower;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_animatedSprite = GetNode<AnimatedSprite2D>("Sprite");

		// Force sprite upright on spawn
		if (_animatedSprite != null)
		{
			_animatedSprite.Rotation = 0f;
			_animatedSprite.FlipV = false;
		}

		// Animation setup
		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null &&
			_animatedSprite.SpriteFrames.HasAnimation(WalkAnimation))
		{
			_animatedSprite.Play(WalkAnimation);
			_animatedSprite.AnimationFinished += () => _animatedSprite.Play(WalkAnimation);
		}

		// Create unique health bar from template
		var template = GetTree().Root.GetNodeOrNull<ProgressBar>("Area/HealthBarTemplate");
		if (template != null)
		{
			_healthBarInstance = (ProgressBar)template.Duplicate();
			GetTree().CurrentScene.AddChild(_healthBarInstance);
			_healthBarInstance.Visible = true;
			_healthBarInstance.ZIndex = 10;

			// Apply 90-degree rotation right after creation so it starts horizontal
			_healthBarInstance.Rotation = Mathf.Pi / 2;
		}
		else
		{
			GD.Print("ERROR: HealthBarTemplate not found at 'Area/HealthBarTemplate'");
		}

		_tower = GetTree().GetFirstNodeInGroup("towers") as Node2D;
		if (_tower != null)
			_targetPos = _tower.GlobalPosition;

		UpdateHealthBar();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_tower == null) return;

		Vector2 direction = (_targetPos - GlobalPosition).Normalized();
		Velocity = direction * Speed;
		MoveAndSlide();

		// Keep sprite upright — only flip horizontally for left/right facing
		if (_animatedSprite != null && direction.LengthSquared() > 0.1f)
		{
			_animatedSprite.FlipH = direction.X < 0;   // Face left if moving left
			_animatedSprite.FlipV = false;             // Never flip vertically
			_animatedSprite.Rotation = 0f;             // Enforce no rotation
		}

		// Health bar: rotated 90° to make it horizontal, positioned above head
		if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
		{
			// Keep rotation consistent every frame (in case something resets it)
			_healthBarInstance.Rotation = Mathf.Pi / 2;

			// Offset when rotated 90° — the bar's "width" is now vertical in world space
			// You will likely need to tune these values based on your sprite size
			// Try variations: (-50, -30), (-40, -50), (-60, -20), (30, -40), etc.
			Vector2 offset = new Vector2(-50, -40);   // ← Start here and adjust
			_healthBarInstance.GlobalPosition = GlobalPosition + offset;
		}
	}

	public void TakeDamage(int damage)
	{
		_currentHealth -= damage;
		if (_currentHealth < 0) _currentHealth = 0;
		UpdateHealthBar();

		if (_currentHealth <= 0)
		{
			if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
				_healthBarInstance.QueueFree();
			QueueFree();
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
