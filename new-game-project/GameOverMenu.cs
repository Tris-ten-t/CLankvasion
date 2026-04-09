using Godot;

public partial class GameOverMenu : CanvasLayer
{
	private Label finalWaveLabel;

	public override void _Ready()
	{
		finalWaveLabel = GetNodeOrNull<Label>("Content/FinalWaveLabel");

		// Connect buttons
		var restartBtn = GetNode<Button>("Content/RestartButton");
		var quitBtn = GetNode<Button>("Content/QuitButton");

		restartBtn.Pressed += OnRestartPressed;
		quitBtn.Pressed += OnQuitPressed;

		// Optional: Pause the game while menu is shown
		GetTree().Paused = true;
	}

	public void SetFinalWave(int waveNumber)
	{
		if (finalWaveLabel != null)
			finalWaveLabel.Text = $"You reached Wave {waveNumber}";
	}

	private void OnRestartPressed()
	{
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
