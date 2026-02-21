using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene EnemyScene;
	[Export] public float SpawnInterval = 2.0f;
	[Export] public int MaxEnemies = 10;
	[Export] public float SpawnRadius = 600f;  // Distance from tower (adjust in inspector)

	private Timer _spawnTimer;
	private int _spawnedCount = 0;
	private Node2D _tower;  // Reference to tower for centering spawns

	public override void _Ready()
	{
		_tower = GetTree().GetFirstNodeInGroup("towers") as Node2D;

		_spawnTimer = new Timer();
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Autostart = true;
		_spawnTimer.Timeout += SpawnOneEnemy;
		AddChild(_spawnTimer);
	}

	private void SpawnOneEnemy()
	{
		if (_spawnedCount >= MaxEnemies || EnemyScene == null || _tower == null)
			return;

		var enemy = EnemyScene.Instantiate<CharacterBody2D>();
		GetTree().CurrentScene.AddChild(enemy);

		// Random angle around tower (360°)
		float randomAngle = GD.Randf() * Mathf.Pi * 2;  // 0 to 360 degrees in radians
		Vector2 spawnOffset = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * SpawnRadius;

		enemy.GlobalPosition = _tower.GlobalPosition + spawnOffset;

		_spawnedCount++;
	}

	public void ResetCounter()
	{
		_spawnedCount = 0;
	}
}
