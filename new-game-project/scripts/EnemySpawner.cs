using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene RollerScene;
	[Export] public PackedScene ClankScene;
	[Export] public PackedScene BullScene;
	[Export] public PackedScene ClinkScene;
	[Export] public PackedScene ClunkScene;

	[Export] public float SpawnInterval = 2.0f;
	[Export] public int MaxEnemies = 20;

	private Timer _spawnTimer;
	private int _spawnedCount = 0;
	private Node2D _spawnPoints;   // Container with all spawn markers

	public override void _Ready()
	{
		_spawnPoints = GetTree().CurrentScene.GetNodeOrNull<Node2D>("SpawnPoints");

		if (_spawnPoints == null || _spawnPoints.GetChildCount() == 0)
			GD.Print("WARNING: No SpawnPoints node found!");

		_spawnTimer = new Timer();
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Autostart = true;
		_spawnTimer.Timeout += SpawnOneEnemy;
		AddChild(_spawnTimer);
	}

	private void SpawnOneEnemy()
	{
		if (_spawnedCount >= MaxEnemies) return;
		if (_spawnPoints == null || _spawnPoints.GetChildCount() == 0) return;

		PackedScene selectedScene = GetRandomEnemyScene();
		if (selectedScene == null) return;

		var enemy = selectedScene.Instantiate<CharacterBody2D>();
		GetTree().CurrentScene.AddChild(enemy);

		// Pick a random spawn point from the list
		int randomIndex = GD.RandRange(0, _spawnPoints.GetChildCount() - 1);
		var spawnMarker = _spawnPoints.GetChild<Node2D>(randomIndex);
		enemy.GlobalPosition = spawnMarker.GlobalPosition;

		_spawnedCount++;
	}

	private PackedScene GetRandomEnemyScene()
	{
		float rand = GD.Randf();
		if (rand < 0.25f) return RollerScene;
		if (rand < 0.5f) return ClankScene;
		if (rand < 0.75f) return BullScene;
		if (rand < 0.9f) return ClinkScene;
		return ClunkScene ?? RollerScene;
	}

	public void ResetCounter()
	{
		_spawnedCount = 0;
	}
}
