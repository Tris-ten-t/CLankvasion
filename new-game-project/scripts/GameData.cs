using Godot;

public partial class GameData : Node
{
	public static GameData Instance { get; private set; }
	public int SelectedLevel { get; set; } = 1;

	public override void _Ready()
	{
		Instance = this;
	}
}
