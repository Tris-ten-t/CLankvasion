using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class GameData : Node
{
	public static GameData Instance { get; private set; }
	public int SelectedLevel { get; set; } = 1;
	public int CurrentWave { get; set; } = 0;

	private const string SavePath = "user://leaderboard.json";

	public override void _Ready()
	{
		Instance = this;
	}

	public void SaveScore(int level, int wavesCompleted)
	{
		var scores = LoadAllScores();
		string key = $"level_{level}";

		// Only save if it's higher than the current best
		if (!scores.ContainsKey(key) || wavesCompleted > scores[key])
			scores[key] = wavesCompleted;

		string json = JsonSerializer.Serialize(scores);
		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		file.StoreString(json);
	}

	public Dictionary<string, int> LoadAllScores()
	{
		if (!FileAccess.FileExists(SavePath))
			return new Dictionary<string, int>();

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		string json = file.GetAsText();

		try
		{
			return JsonSerializer.Deserialize<Dictionary<string, int>>(json)
				   ?? new Dictionary<string, int>();
		}
		catch
		{
			return new Dictionary<string, int>();
		}
	}

	public int GetTotalWavesForLevel(int level)
	{
		var scores = LoadAllScores();
		string key = $"level_{level}";
		return scores.ContainsKey(key) ? scores[key] : 0;
	}
}
