using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene RollerScene;
	[Export] public PackedScene ClankScene;
	[Export] public PackedScene BullScene;
	[Export] public PackedScene ClinkScene;
	[Export] public PackedScene ClunkScene;
<<<<<<< Updated upstream

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
=======
	[Export] public NodePath[] SpawnPoints;
	[Export] public WaveData[] Waves = new WaveData[0];
	[Export] public float SpawnInterval = 1.2f;
	[Export] public float TimeBetweenWaves = 10.0f;

	private List<Marker2D> _spawnMarkers = new List<Marker2D>();
	private List<PackedScene> _spawnQueue = new List<PackedScene>();
	private Timer _spawnTimer;
	private Timer _waveDelayTimer;
	private int _currentWaveIndex = 0;
	private int _livingEnemies = 0;

	public override void _Ready()
	{
		foreach (var path in SpawnPoints)
		{
			var marker = GetNodeOrNull<Marker2D>(path);
			if (marker != null)
				_spawnMarkers.Add(marker);
		}

		if (_spawnMarkers.Count == 0)
			GD.PrintErr("No spawn points assigned!");

		_spawnTimer = new Timer();
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Timeout += SpawnNextEnemy;
		AddChild(_spawnTimer);

		_waveDelayTimer = new Timer();
		_waveDelayTimer.OneShot = true;
		_waveDelayTimer.Timeout += StartNextWave;
		AddChild(_waveDelayTimer);

		if (Waves.Length > 0)
			StartWave(Waves[0]);
		else
			GD.Print("No waves defined.");
	}

	public void StartWave(WaveData wave)
	{
		_spawnQueue.Clear();
		_livingEnemies = 0;

		for (int i = 0; i < wave.RollerCount; i++) _spawnQueue.Add(RollerScene);
		for (int i = 0; i < wave.ClankCount; i++) _spawnQueue.Add(ClankScene);
		for (int i = 0; i < wave.BullCount; i++) _spawnQueue.Add(BullScene);
		for (int i = 0; i < wave.ClinkCount; i++) _spawnQueue.Add(ClinkScene);
		for (int i = 0; i < wave.ClunkCount; i++) _spawnQueue.Add(ClunkScene);

		ShuffleList(_spawnQueue);

		GD.Print($"Starting Wave {wave.WaveNumber} - Total Enemies: {_spawnQueue.Count}");
		_spawnTimer.Start();
	}

	private void StartNextWave()
	{
		_currentWaveIndex++;

		if (_currentWaveIndex < Waves.Length)
		{
			GD.Print($"Starting next wave ({_currentWaveIndex + 1}/{Waves.Length})");
			StartWave(Waves[_currentWaveIndex]);
		}
		else
		{
			GD.Print("All waves complete!");
		}
	}

	private void SpawnNextEnemy()
	{
		if (_spawnQueue.Count == 0)
		{
			_spawnTimer.Stop();
			GD.Print("All enemies spawned - waiting for them to die...");
			return;
		}

		if (_spawnMarkers.Count == 0) return;

		PackedScene selectedScene = _spawnQueue[0];
		_spawnQueue.RemoveAt(0);

		var enemy = selectedScene.Instantiate<CharacterBody2D>();
		enemy.TreeExited += OnEnemyDied;
		_livingEnemies++;

		GetTree().CurrentScene.AddChild(enemy);

		int spawnIndex = (int)(GD.Randi() % (uint)_spawnMarkers.Count);
		enemy.GlobalPosition = _spawnMarkers[spawnIndex].GlobalPosition;

		GD.Print($"Spawned {enemy.Name} at spawn point {spawnIndex} - Remaining: {_spawnQueue.Count}");
	}

	private void OnEnemyDied()
	{
		_livingEnemies--;
		GD.Print($"Enemy died - Remaining alive: {_livingEnemies}");

		if (_livingEnemies <= 0 && _spawnQueue.Count == 0)
		{
			GD.Print("All enemies dead - starting next wave timer...");
			_waveDelayTimer.WaitTime = TimeBetweenWaves;
			_waveDelayTimer.Start();
		}
	}

	private void ShuffleList<T>(List<T> list)
	{
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = (int)(GD.Randi() % (uint)(i + 1));
			T temp = list[i];
			list[i] = list[j];
			list[j] = temp;
		}
>>>>>>> Stashed changes
	}
}
