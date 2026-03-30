using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene RollerScene;     // Fast, low HP
	[Export] public PackedScene ClankScene;      // Slow, medium HP
	[Export] public PackedScene BullScene;       // Medium speed, high HP
	[Export] public PackedScene ClinkScene;      // Fast, high HP
	[Export] public PackedScene ClunkScene;      // ← Your new one
	[Export] public float SpawnInterval = 2.0f;  // Seconds between spawns
	[Export] public int MaxEnemies = 20;         // Total enemies before stopping
	[Export] public float SpawnRadius = 600f;    // Distance from tower

	// Spawn chances (adjust these in the inspector as needed)
	[Export] public float ClankSpawnChance = 0.25f;
	[Export] public float BullSpawnChance = 0.25f;
	[Export] public float ClinkSpawnChance = 0.2f;
	[Export] public float ClunkSpawnChance = 0.2f;
	// Roller gets the remaining percentage

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

		PackedScene selectedScene;
		float rand = GD.Randf();

		if (rand < ClankSpawnChance)
		{
			selectedScene = ClankScene ?? RollerScene;
		}
		else if (rand < ClankSpawnChance + BullSpawnChance)
		{
			selectedScene = BullScene ?? RollerScene;
		}
		else if (rand < ClankSpawnChance + BullSpawnChance + ClinkSpawnChance)
		{
			selectedScene = ClinkScene ?? RollerScene;
		}
		else if (rand < ClankSpawnChance + BullSpawnChance + ClinkSpawnChance + ClunkSpawnChance)
		{
			selectedScene = ClunkScene ?? RollerScene;
		}
		else
		{
			selectedScene = RollerScene;
		}

		if (selectedScene == null) return;

		var enemy = selectedScene.Instantiate<CharacterBody2D>();
		GetTree().CurrentScene.AddChild(enemy);

		// Spawn in random direction around tower (full 360°)
		float randomAngle = GD.Randf() * Mathf.Tau;
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
