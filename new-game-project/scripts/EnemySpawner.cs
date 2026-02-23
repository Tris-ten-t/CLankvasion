using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene RollerScene;     // Drag your original Enemy.tscn here
	[Export] public PackedScene ClankScene;      // Drag your Clank.tscn here
	[Export] public float SpawnInterval = 2.0f;  // Seconds between spawns
	[Export] public int MaxEnemies = 10;         // Total enemies before stopping
	[Export] public float SpawnRadius = 600f;    // Distance from tower (adjust for map size)
	[Export] public float ClankSpawnChance = 0.4f;  // 40% chance to spawn Clank (60% Roller)

	private Timer _spawnTimer;
	private int _spawnedCount = 0;
	private Node2D _tower;

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
		if (_spawnedCount >= MaxEnemies) return;
		if (_tower == null) return;

		// Randomly choose which enemy to spawn
		PackedScene selectedScene;
		if (GD.Randf() < ClankSpawnChance)
		{
			selectedScene = ClankScene ?? RollerScene;  // Fallback to Roller if Clank not set
		}
		else
		{
			selectedScene = RollerScene;
		}

		if (selectedScene == null) return;

		var enemy = selectedScene.Instantiate<CharacterBody2D>();
		GetTree().CurrentScene.AddChild(enemy);

		// Spawn in random direction around tower (full 360°)
		float randomAngle = GD.Randf() * Mathf.Tau;  // Tau = 2*Pi = full circle
		Vector2 spawnOffset = new Vector2(
			Mathf.Cos(randomAngle),
			Mathf.Sin(randomAngle)
		) * SpawnRadius;

		enemy.GlobalPosition = _tower.GlobalPosition + spawnOffset;

		_spawnedCount++;
	}

	// Call this to reset for new waves or restarts
	public void ResetCounter()
	{
		_spawnedCount = 0;
	}
}
