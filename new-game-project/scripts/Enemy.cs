using Godot;

public partial class Enemy : CharacterBody2D, IDamageable  // ← Added IDamageable
{
	[Export] public float Speed = 60f;
	[Export] public int MaxHealth = 3;
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
		if (direction.LengthSquared() > 0.1f)
		{
			LookAt(GlobalPosition + direction);
			Rotation += Mathf.Pi / 2; // your working animation offset
		}
		// Health bar: always above head + forced horizontal
		if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
		{
			_healthBarInstance.Rotation = Mathf.Pi / 2; // +90° = horizontal
			// If bar is upside-down after this, change to: -Mathf.Pi / 2
			Vector2 worldOffset = new Vector2(0, -50); // tune Y for height
			_healthBarInstance.GlobalPosition = GlobalPosition + worldOffset;
		}
	}

	// ← Made PUBLIC for interface
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
		Color barColor = healthPct > 0.6f ? Colors.Green : healthPct > 0.3f ? Colors.Yellow : Colors.Red;
		StyleBoxFlat currentStyle = _healthBarInstance.GetThemeStylebox("fill") as StyleBoxFlat;
		StyleBoxFlat newStyle = currentStyle != null ? (StyleBoxFlat)currentStyle.Duplicate() : new StyleBoxFlat();
		newStyle.BgColor = barColor;
		_healthBarInstance.AddThemeStyleboxOverride("fill", newStyle);
	}
}
