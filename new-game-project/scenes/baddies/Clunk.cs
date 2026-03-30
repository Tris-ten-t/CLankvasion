using Godot;

public partial class Clunk : CharacterBody2D, IDamageable
{
	[Export] public float Speed = 58f;           // Fast
	[Export] public int MaxHealth = 20;          // High HP
	[Export] public string WalkAnimation = "walk";   // Make sure this matches SpriteFrames exactly
	[Export] public string DeathAnimation = "death";

	private int _currentHealth;
	private ProgressBar _healthBarInstance;
	private AnimatedSprite2D _animatedSprite;
	private Vector2 _targetPos;
	private Node2D _tower;
	private bool _isDead = false;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");

		if (_animatedSprite == null)
		{
			GD.Print("[Clunk] ERROR: No AnimatedSprite2D named 'Sprite' found!");
			return;
		}

		GD.Print("[Clunk] Sprite found. Available animations: ", string.Join(", ", _animatedSprite.SpriteFrames?.GetAnimationNames() ?? new string[0]));

		// Force sprite upright
		_animatedSprite.Rotation = 0f;
		_animatedSprite.FlipV = false;

		// Try to play walk animation
		if (_animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation(WalkAnimation))
		{
			_animatedSprite.Play(WalkAnimation);
			GD.Print("[Clunk] Successfully started playing walk animation: " + WalkAnimation);
		}
		else
		{
			GD.Print("[Clunk] ERROR: Animation '" + WalkAnimation + "' not found in SpriteFrames!");
		}

		_animatedSprite.AnimationFinished += OnAnimationFinished;

		// Health bar setup
		var template = GetTree().Root.GetNodeOrNull<ProgressBar>("Area/HealthBarTemplate");
		if (template != null)
		{
			_healthBarInstance = (ProgressBar)template.Duplicate();
			GetTree().CurrentScene.AddChild(_healthBarInstance);
			_healthBarInstance.Visible = true;
			_healthBarInstance.ZIndex = 10;
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
		if (_tower == null || _isDead) return;

		Vector2 direction = (_targetPos - GlobalPosition).Normalized();
		Velocity = direction * Speed;
		MoveAndSlide();

		if (_animatedSprite != null && direction.LengthSquared() > 0.1f)
		{
			_animatedSprite.FlipH = direction.X < 0;
			_animatedSprite.FlipV = false;
			_animatedSprite.Rotation = 0f;
		}

		if (_healthBarInstance != null && IsInstanceValid(_healthBarInstance))
		{
			_healthBarInstance.Rotation = Mathf.Pi / 2;
			Vector2 offset = new Vector2(-50, -40);
			_healthBarInstance.GlobalPosition = GlobalPosition + offset;
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
		Velocity = Vector2.Zero;

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
		else if (_animatedSprite.Animation == WalkAnimation)
		{
			_animatedSprite.Play(WalkAnimation); // Force loop
		}
	}

	private void CleanupAndDie()
	{
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
