using Godot;

public partial class Enemy : BaseEnemy
{
	public override EnemyType EnemyType => EnemyType.Roller;
	public override int CoinValue => 1;
	public override Vector2 HealthBarOffset => new Vector2(30, -40);
	public override float HealthBarRotation => -Mathf.Pi / 2;

	public override void _PhysicsProcess(double delta)
	{
		if (CheckStun()) return;
		if (_isDying || _tower == null) { Velocity = Vector2.Zero; return; }

		Vector2 direction = (_tower.GlobalPosition - GlobalPosition).Normalized();
		Velocity = direction * Speed;
		MoveAndSlide();

		if (_animatedSprite != null && direction.LengthSquared() > 0.1f)
		{
			_animatedSprite.LookAt(GlobalPosition + direction);
			_animatedSprite.Rotation += Mathf.Pi / 2;
		}

		UpdateHealthBarPosition();
		CheckTowerDistance();
	}
}
