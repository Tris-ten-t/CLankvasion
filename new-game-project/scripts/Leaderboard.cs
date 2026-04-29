using Godot;

public partial class Leaderboard : Control
{
	private readonly string[] _levelNames = new string[]
	{
		"The City",
		"The Island",
		"The Volcano",
		"The Pizzeria?",
	};

	private int _selectedLevel = 1;
	private Label _scoresLabel;
	private HBoxContainer _tabContainer;

	public override void _Ready()
	{
		_scoresLabel = GetNode<Label>("ScoresLabel");
		_tabContainer = GetNode<HBoxContainer>("TabContainer");

		// Create a tab button for each level
		for (int i = 0; i < _levelNames.Length; i++)
		{
			int levelNumber = i + 1;
			var button = new Button();
			button.Text = _levelNames[i];
			button.Pressed += () => OnLevelTabPressed(levelNumber);
			_tabContainer.AddChild(button);
		}

		// Show first level by default
		ShowScoresForLevel(1);
	}

	private void OnLevelTabPressed(int level)
	{
		_selectedLevel = level;
		ShowScoresForLevel(level);
	}

	private void ShowScoresForLevel(int level)
	{
		int totalWaves = GameData.Instance.GetTotalWavesForLevel(level);
		string levelName = _levelNames[level - 1];

		string display = $"{levelName}\n";
		display += "─────────────────\n";
		display += $"Total Waves Survived: {totalWaves}\n";

		_scoresLabel.Text = display;
		GD.Print($"Showing scores for {levelName}: {totalWaves} waves");
	}
}
