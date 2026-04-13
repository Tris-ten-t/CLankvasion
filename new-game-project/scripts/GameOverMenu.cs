using Godot;

public partial class GameOverMenu : CanvasLayer
{
	public override void _Ready()
	{
		GD.Print("Game Over Menu _Ready() - Setting up buttons...");

		Layer = 128;                    // High layer
		GetTree().Paused = true;

		// Find buttons (with multiple possible paths)
		Button restartBtn = GetNodeOrNull<Button>("%RestartButton") 
						 ?? GetNodeOrNull<Button>("Content/RestartButton")
						 ?? GetNodeOrNull<Button>("RestartButton");

		Button quitBtn = GetNodeOrNull<Button>("%QuitButton") 
					  ?? GetNodeOrNull<Button>("Content/QuitButton")
					  ?? GetNodeOrNull<Button>("QuitButton");

		if (restartBtn != null)
		{
			restartBtn.Pressed += OnRestartPressed;
			GD.Print("RestartButton connected");
		}
		else
			GD.PrintErr("ERROR: RestartButton not found!");

		if (quitBtn != null)
		{
			quitBtn.Pressed += OnQuitPressed;
			GD.Print("QuitButton connected");
		}
		else
			GD.PrintErr("ERROR: QuitButton not found!");
	}

	private void OnRestartPressed()
	{
		GD.Print("Restart button clicked! Reloading scene...");
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnQuitPressed()
	{
		GD.Print("Quit button clicked! Exiting game...");
		GetTree().Paused = false;
		GetTree().Quit();
	}

	// Make sure input is processed even when paused
	public override void _Input(InputEvent @event)
	{
		// This helps ensure clicks are registered
	}
}
