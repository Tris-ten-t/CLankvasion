using Godot;

public partial class PauseMenu : CanvasLayer
{
	[Export] public string AreaScenePath = "res://scenes/Area.tscn";
	[Export] public string MainMenuScenePath = "res://scenes/MainMenu.tscn";

	private Panel _panel;
	private bool _isPaused = false;

	// Slide animation settings
	private float _hiddenX = -400f;  // Off screen to the left
	private float _shownX = 0f;      // On screen
	private float _slideSpeed = 10f;
	private float _targetX;

	public override void _Ready()
	{
		_panel = GetNode<Panel>("PausePanel");

		// Start hidden off screen
		_panel.Position = new Vector2(_hiddenX, _panel.Position.Y);
		_targetX = _hiddenX;

		GetNode<Button>("PausePanel/VBoxContainer/Resume").Pressed += OnResumePressed;
		GetNode<Button>("PausePanel/VBoxContainer/Restart").Pressed += OnRestartPressed;
		GetNode<Button>("PausePanel/VBoxContainer/Quit to Menu").Pressed += OnQuitPressed;
	}

	public override void _Process(double delta)
	{
		// Slide the panel toward target position
		float currentX = _panel.Position.X;
		float newX = Mathf.Lerp(currentX, _targetX, (float)delta * _slideSpeed);
		_panel.Position = new Vector2(newX, _panel.Position.Y);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel")) // ui_cancel is the Escape key by default
		{
			if (_isPaused)
				Resume();
			else
				Pause();
		}
	}

	private void Pause()
	{
		_isPaused = true;
		_targetX = _shownX;
		GetTree().Paused = true;
		Show();
	}

	private void Resume()
	{
		_isPaused = false;
		_targetX = _hiddenX;
		GetTree().Paused = false;
	}

	private void OnResumePressed()
	{
		Resume();
	}

	private void OnRestartPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://scenes/area.tscn");
	}

	private void OnQuitPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}
}
