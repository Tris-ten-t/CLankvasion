using Godot;

public partial class Clank : BaseEnemy
{
	public override EnemyType EnemyType => EnemyType.Clank;
	public override int CoinValue => 2;
	public override Vector2 HealthBarOffset => new Vector2(-50, -40);
	public override float HealthBarRotation => Mathf.Pi / 2;

	public override void _PhysicsProcess(double delta)
	{
		if (CheckStun()) return;
		if (_isDying || _tower == null) { Velocity = Vector2.Zero; return; }

		Vector2 direction = (_tower.GlobalPosition - GlobalPosition).Normalized();
		Velocity = direction * Speed;
		MoveAndSlide();

		if (_animatedSprite != null && direction.LengthSquared() > 0.1f)
		{
			_animatedSprite.FlipH = direction.X < 0;
			_animatedSprite.FlipV = false;
			_animatedSprite.Rotation = 0f;
		}

		UpdateHealthBarPosition();
		CheckTowerDistance();
	}

	protected override void OnAnimationFinished()
	{
		if (_animatedSprite.Animation == DeathAnimation)
			CleanupAndDie();
		else if (_animatedSprite.Animation == WalkAnimation && !_isDying)
			_animatedSprite.Play(WalkAnimation);
	}
}
