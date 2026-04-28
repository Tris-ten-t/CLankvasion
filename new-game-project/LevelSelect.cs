using Godot;

public partial class LevelSelect : Control
{
	// Set these in the Inspector to point to your area scene
	[Export] public string AreaScenePath = "res://scenes/Area.tscn";

	private readonly string[] _levelNames = new string[]
	{
		"Level 1 - The City",
		"Level 2 - The Island",
		"Level 3 - The Volcano",
		"Level 4 - The Pizzeria?",
	};

	public override void _Ready()
	{
		var vbox = GetNode<VBoxContainer>("VBoxContainer");

		for (int i = 0; i < _levelNames.Length; i++)
		{
			int levelNumber = i + 1; // Capture for lambda
			var button = new Button();
			button.Text = _levelNames[i];
			button.Pressed += () => OnLevelSelected(levelNumber);
			vbox.AddChild(button);
		}

		// Back button to return to main menu
		var backButton = new Button();
		backButton.Text = "Back";
		backButton.Pressed += OnBackPressed;
		vbox.AddChild(backButton);
	}

	private void OnLevelSelected(int level)
	{
		GameData.Instance.SelectedLevel = level;
		GD.Print($"Selected Level {level} - Loading area scene...");
		GetTree().ChangeSceneToFile(AreaScenePath);
	}

	private void OnBackPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn"); // Adjust path to yours
	}
}
