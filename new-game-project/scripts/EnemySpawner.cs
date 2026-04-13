using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene RollerScene;
	[Export] public PackedScene ClankScene;
	[Export] public PackedScene BullScene;
	[Export] public PackedScene ClinkScene;
	[Export] public PackedScene ClunkScene;

	[Export] public WaveData[] Waves;

	private Timer _spawnTimer;
	private int _currentWave = 0;
	private int _enemiesSpawnedThisWave = 0;
	private int _totalEnemiesThisWave = 0;
	private Node2D _tower;
	private Node2D _spawnPoints;

	public override void _Ready()
	{
		_tower = GetTree().GetFirstNodeInGroup("towers") as Node2D;
		_spawnPoints = GetTree().CurrentScene.GetNodeOrNull<Node2D>("SpawnPoints");

		if (Waves == null || Waves.Length == 0)
		{
			GD.Print("WARNING: No waves defined in EnemySpawner!");
			return;
		}

		StartWave(0);
	}

	private void StartWave(int waveIndex)
	{
		if (waveIndex >= Waves.Length)
		{
			GD.Print("All waves completed!");
			return;
		}

		_currentWave = waveIndex;
		var wave = Waves[waveIndex];

		_totalEnemiesThisWave = wave.TotalEnemies;
		_enemiesSpawnedThisWave = 0;

		GD.Print($"Starting Wave {wave.WaveNumber} - Total Enemies: {wave.TotalEnemies}");

		_spawnTimer = new Timer();
		_spawnTimer.WaitTime = wave.SpawnInterval;
		_spawnTimer.Autostart = true;
		_spawnTimer.Timeout += SpawnEnemyFromWave;
		AddChild(_spawnTimer);
	}

	private void SpawnEnemyFromWave()
	{
		if (_enemiesSpawnedThisWave >= _totalEnemiesThisWave)
		{
			_spawnTimer.QueueFree();
			CallDeferred("CheckWaveComplete");
			return;
		}

		var wave = Waves[_currentWave];
		PackedScene selectedScene = GetEnemyFromWave(wave);

		if (selectedScene == null) return;

		var enemy = selectedScene.Instantiate<CharacterBody2D>();
		GetTree().CurrentScene.AddChild(enemy);

		if (_spawnPoints != null && _spawnPoints.GetChildCount() > 0)
		{
			int idx = GD.RandRange(0, _spawnPoints.GetChildCount() - 1);
			var spawnMarker = _spawnPoints.GetChild<Node2D>(idx);
			enemy.GlobalPosition = spawnMarker.GlobalPosition;
		}

		_enemiesSpawnedThisWave++;
	}

	private PackedScene GetEnemyFromWave(WaveData wave)
	{
		int total = wave.RollerCount + wave.ClankCount + wave.BullCount + wave.ClinkCount + wave.ClunkCount;
		if (total == 0) return RollerScene;

		int roll = GD.RandRange(1, total);

		if (roll <= wave.RollerCount) return RollerScene;
		roll -= wave.RollerCount;
		if (roll <= wave.ClankCount) return ClankScene;
		roll -= wave.ClankCount;
		if (roll <= wave.BullCount) return BullScene;
		roll -= wave.BullCount;
		if (roll <= wave.ClinkCount) return ClinkScene;
		return ClunkScene ?? RollerScene;
	}

	private void CheckWaveComplete()
	{
		var timer = new Timer();
		timer.WaitTime = 8.0f; // Break between waves
		timer.OneShot = true;
		timer.Timeout += () => StartWave(_currentWave + 1);
		AddChild(timer);
		timer.Start();
	}

	public void ResetCounter()
	{
		_enemiesSpawnedThisWave = 0;
	}
}
