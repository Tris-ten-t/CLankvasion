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

		for (int i = 0; i < _levelNames.Length; i++)
		{
			int levelNumber = i + 1;
			var button = new Button();
			button.Text = _levelNames[i];
			button.Pressed += () => OnLevelTabPressed(levelNumber);
			_tabContainer.AddChild(button);
		}

		ShowScoresForLevel(1);
	}

	private void OnLevelTabPressed(int level)
	{
		_selectedLevel = level;
		ShowScoresForLevel(level);
	}

	private void ShowScoresForLevel(int level)
	{
		var scores = GameData.Instance.GetScoresForLevel(level);
		string levelName = _levelNames[level - 1];

		string display = $"{levelName} - Top 10\n";
		display += "─────────────────\n";

		if (scores.Count == 0)
		{
			display += "No scores yet!";
		}
		else
		{
			for (int i = 0; i < scores.Count; i++)
			{
				string medal = i == 0 ? "🥇" : i == 1 ? "🥈" : i == 2 ? "🥉" : $"{i + 1}.";
				display += $"{medal} {scores[i]} waves\n";
			}
		}

		_scoresLabel.Text = display;
	}
}
