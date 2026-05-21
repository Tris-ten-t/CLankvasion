using Godot;

public partial class PauseMenu : CanvasLayer
{
	[Export] public string AreaScenePath = "res://scenes/Area.tscn";
	[Export] public string MainMenuScenePath = "res://scenes/MainMenu.tscn";

	private Panel _panel;
	private bool _isPaused = false;
	private float _hiddenX = -400f;
	private float _shownX = 0f;
	private float _slideSpeed = 10f;
	private float _targetX;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Layer = 10;

		_panel = GetNodeOrNull<Panel>("PausePanel");
		if (_panel == null)
		{
			GD.PrintErr("PausePanel not found!");
			return;
		}

		_panel.Visible = false;
		_panel.Position = new Vector2(_hiddenX, _panel.Position.Y);
		_targetX = _hiddenX;

		var resume = GetNodeOrNull<Button>("PausePanel/VBoxContainer/Resume");
		var restart = GetNodeOrNull<Button>("PausePanel/VBoxContainer/Restart");
		var quitMenu = GetNodeOrNull<Button>("PausePanel/VBoxContainer/Quit to Menu");
		var quitGame = GetNodeOrNull<Button>("PausePanel/VBoxContainer/Quit Game");

		if (resume != null) resume.Pressed += OnResumePressed;
		else GD.PrintErr("Resume button not found!");

		if (restart != null) restart.Pressed += OnRestartPressed;
		else GD.PrintErr("Restart button not found!");

		if (quitMenu != null) quitMenu.Pressed += OnQuitPressed;
		else GD.PrintErr("Quit to Menu button not found!");

		if (quitGame != null) quitGame.Pressed += OnQuitGamePressed;
		else GD.PrintErr("Quit Game button not found!");

		GD.Print("PauseMenu ready!");
	}

	public override void _Process(double delta)
	{
		if (_panel == null) return;
		float currentX = _panel.Position.X;
		float newX = Mathf.Lerp(currentX, _targetX, (float)delta * _slideSpeed);
		_panel.Position = new Vector2(newX, _panel.Position.Y);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			GD.Print("Escape pressed - isPaused: " + _isPaused);
			if (_isPaused)
				Resume();
			else
				Pause();

			GetViewport().SetInputAsHandled();
		}
	}

	private void Pause()
	{
		GD.Print("Pausing game...");
		_isPaused = true;
		_targetX = _shownX;
		_panel.Visible = true;
		GetTree().Paused = true;
	}

	private void Resume()
	{
		GD.Print("Resuming game...");
		_isPaused = false;
		_targetX = _hiddenX;
		_panel.Visible = false;
		GetTree().Paused = false;
	}

	private void OnResumePressed()
	{
		Resume();
	}

	private void OnRestartPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(AreaScenePath);
	}

	private void OnQuitPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(MainMenuScenePath);
	}

	private void OnQuitGamePressed()
	{
		GetTree().Paused = false;
		GetTree().Quit();
	}
}
