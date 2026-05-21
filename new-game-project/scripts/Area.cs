using Godot;

public partial class Area : Node2D
{
	[Export] public Texture2D[] LevelBackgrounds = new Texture2D[0];
	[Export] public NodePath BackgroundPath;
	[Export] public NodePath StreetlightsPath;

	public override void _Ready()
	{
		int level = GameData.Instance.SelectedLevel;
		GD.Print($"Loading level {level}");

		// Set the background based on selected level
		if (LevelBackgrounds.Length >= level)
		{
			var background = GetNode<Sprite2D>(BackgroundPath);
			background.Texture = LevelBackgrounds[level - 1];
		}
		else
		{
			GD.PrintErr("No background assigned for level " + level);
		}

		// Hide streetlights on levels 2, 3 and 4
		var streetlights = GetNode<Sprite2D>(StreetlightsPath);
		streetlights.Visible = level == 1 || level == 5;
	}
}
