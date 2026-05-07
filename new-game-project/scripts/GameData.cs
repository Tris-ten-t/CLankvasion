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

		if (!scores.ContainsKey(key))
			scores[key] = new List<int>();

		scores[key].Add(wavesCompleted);
		scores[key].Sort((a, b) => b.CompareTo(a)); // Sort highest first

		// Keep only top 10
		if (scores[key].Count > 10)
			scores[key] = scores[key].GetRange(0, 10);

		string json = JsonSerializer.Serialize(scores);
		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		file.StoreString(json);
	}

	public Dictionary<string, List<int>> LoadAllScores()
	{
		if (!FileAccess.FileExists(SavePath))
			return new Dictionary<string, List<int>>();

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		string json = file.GetAsText();

		try
		{
			return JsonSerializer.Deserialize<Dictionary<string, List<int>>>(json)
				   ?? new Dictionary<string, List<int>>();
		}
		catch
		{
			return new Dictionary<string, List<int>>();
		}
	}

	public List<int> GetScoresForLevel(int level)
	{
		var scores = LoadAllScores();
		string key = $"level_{level}";
		return scores.ContainsKey(key) ? scores[key] : new List<int>();
	}
}
