using Godot;
using System.Collections.Generic;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene RollerScene;
	[Export] public PackedScene ClankScene;
	[Export] public PackedScene BullScene;
	[Export] public PackedScene ClinkScene;
	[Export] public PackedScene ClunkScene;
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
		GameData.Instance.CurrentWave = _currentWaveIndex; // Track current wave

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
	}
}
