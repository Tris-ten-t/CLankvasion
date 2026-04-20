using Godot;
using System.Collections.Generic;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene RollerScene;
	[Export] public PackedScene ClankScene;
	[Export] public PackedScene BullScene;
	[Export] public PackedScene ClinkScene;
	[Export] public PackedScene ClunkScene;

	[Export] public NodePath[] SpawnPoints;   // Drag your Marker2D spawn points here

	[Export] public WaveData[] Waves = new WaveData[0];

	[Export] public float SpawnInterval = 1.2f;   // Time between each enemy spawn

	private List<Marker2D> _spawnMarkers = new List<Marker2D>();
	private List<PackedScene> _spawnQueue = new List<PackedScene>();
	private Timer _spawnTimer;

	public override void _Ready()
	{
		// Load spawn points
		foreach (var path in SpawnPoints)
		{
			var marker = GetNodeOrNull<Marker2D>(path);
			if (marker != null)
				_spawnMarkers.Add(marker);
		}

		if (_spawnMarkers.Count == 0)
			GD.PrintErr("No spawn points assigned! Add Marker2Ds to SpawnPoints array.");

		// Setup spawn timer
		_spawnTimer = new Timer();
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Timeout += SpawnNextEnemy;
		AddChild(_spawnTimer);

		// Start first wave automatically
		if (Waves.Length > 0)
			StartWave(Waves[0]);
		else
			GD.Print("No waves defined in EnemySpawner.");
	}

	public void StartWave(WaveData wave)
	{
		_spawnQueue.Clear();

		// Add exact number of each enemy
		for (int i = 0; i < wave.RollerCount; i++) _spawnQueue.Add(RollerScene);
		for (int i = 0; i < wave.ClankCount; i++) _spawnQueue.Add(ClankScene);
		for (int i = 0; i < wave.BullCount; i++) _spawnQueue.Add(BullScene);
		for (int i = 0; i < wave.ClinkCount; i++) _spawnQueue.Add(ClinkScene);
		for (int i = 0; i < wave.ClunkCount; i++) _spawnQueue.Add(ClunkScene);

		// Shuffle so order is random
		ShuffleList(_spawnQueue);

		GD.Print($"Starting Wave {wave.WaveNumber} - Total Enemies: {_spawnQueue.Count}");
		_spawnTimer.Start();   // Start spawning
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

	private void SpawnNextEnemy()
	{
		if (_spawnQueue.Count == 0)
		{
			_spawnTimer.Stop();
			GD.Print("Wave finished - no more enemies to spawn");
			return;
		}

		if (_spawnMarkers.Count == 0) return;

		PackedScene selectedScene = _spawnQueue[0];
		_spawnQueue.RemoveAt(0);

		var enemy = selectedScene.Instantiate<CharacterBody2D>();
		GetTree().CurrentScene.AddChild(enemy);

		// Spawn at a random spawn point
		int spawnIndex = (int)(GD.Randi() % (uint)_spawnMarkers.Count);
		enemy.GlobalPosition = _spawnMarkers[spawnIndex].GlobalPosition;

		GD.Print($"Spawned {enemy.Name} at spawn point {spawnIndex} - Remaining: {_spawnQueue.Count}");
	}
}
