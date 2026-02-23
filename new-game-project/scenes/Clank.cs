using Godot;

public partial class Clank : CharacterBody2D
{
	[Export] public float Speed = 45f;          // Slower than roller
	[Export] public int MaxHealth = 10;         // Tankier
	[Export] public string WalkAnimation = "walk";

	private int _currentHealth;
	private ProgressBar _healthBar;
	private AnimatedSprite2D _animatedSprite;
	private Vector2 _targetPos;
	private Node2D _tower;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_healthBar = GetNode<ProgressBar>("HealthBar");
		_animatedSprite = GetNode<AnimatedSprite2D>("Sprite");

		// Animation setup (same as roller)
		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null &&
			_animatedSprite.SpriteFrames.HasAnimation(WalkAnimation))
		{
			_animatedSprite.Play(WalkAnimation);
			_animatedSprite.AnimationFinished += () => _animatedSprite.Play(WalkAnimation); // Loop
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

		// Rotate enemy + animation toward player/tower (your working version)
		if (direction.LengthSquared() > 0.1f)
		{
			LookAt(GlobalPosition + direction);
			Rotation += Mathf.Pi / 2; // +90 degrees (this fixed facing)
		}

		// Health bar stays horizontal
		if (_healthBar != null)
		{
			_healthBar.Rotation = -Rotation;
			_healthBar.RotationDegrees = 0; // extra lock
		}
	}

	public void TakeDamage(int damage)
	{
		_currentHealth -= damage;
		if (_currentHealth < 0) _currentHealth = 0;
		UpdateHealthBar();
		if (_currentHealth <= 0) QueueFree();
	}

	private void UpdateHealthBar()
	{
		if (_healthBar == null) return;

		float healthPct = (float)_currentHealth / MaxHealth;
		_healthBar.Value = Mathf.Lerp(0, 100, healthPct);

		Color barColor = healthPct > 0.6f ? Colors.Green : healthPct > 0.3f ? Colors.Yellow : Colors.Red;

		StyleBoxFlat currentStyle = _healthBar.GetThemeStylebox("fill") as StyleBoxFlat;
		StyleBoxFlat newStyle = currentStyle != null ? (StyleBoxFlat)currentStyle.Duplicate() : new StyleBoxFlat();
		newStyle.BgColor = barColor;
		_healthBar.AddThemeStyleboxOverride("fill", newStyle);
	}
}
