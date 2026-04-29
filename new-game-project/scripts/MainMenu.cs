using Godot;
using System;

public partial class MainMenu : Control
{
	// Node references
	private Button startButton;
	private Button settingsButton;
	private Button quitButton;
	private Button leaderboardButton;
	private VBoxContainer mainButtons;
	private VBoxContainer settingsPanel;
	private Control leaderboardPanel;
	private HSlider volumeSlider;
	private OptionButton resolutionOption;
	private CheckButton fullscreenToggle;
	private Button backButton;
	private Button leaderboardBackButton;
	private TextureRect animatedSpriteBG;

	private Vector2 screenCenter;

	public override void _Ready()
	{
		// Get nodes
		startButton = GetNode<Button>("CenterContainer/MainButtons/StartButton");
		settingsButton = GetNode<Button>("CenterContainer/MainButtons/SettingsButton");
		quitButton = GetNode<Button>("CenterContainer/MainButtons/QuitButton");
		leaderboardButton = GetNode<Button>("CenterContainer/MainButtons/LeaderboardButton");
		mainButtons = GetNode<VBoxContainer>("CenterContainer/MainButtons");
		settingsPanel = GetNode<VBoxContainer>("CenterContainer/SettingsPanel");
		leaderboardPanel = GetNode<Control>("CenterContainer/LeaderboardPanel");
		volumeSlider = GetNode<HSlider>("CenterContainer/SettingsPanel/VolumeSlider");
		resolutionOption = GetNode<OptionButton>("CenterContainer/SettingsPanel/ResolutionOption");
		fullscreenToggle = GetNode<CheckButton>("CenterContainer/SettingsPanel/FullscreenToggle");
		backButton = GetNode<Button>("CenterContainer/SettingsPanel/BackButton");
		leaderboardBackButton = GetNode<Button>("CenterContainer/LeaderboardPanel/BackButton");
		animatedSpriteBG = GetNode<TextureRect>("AnimatedSpriteBG");

		// Connect signals
		startButton.Pressed += OnStartPressed;
		settingsButton.Pressed += OnSettingsPressed;
		quitButton.Pressed += OnQuitPressed;
		leaderboardButton.Pressed += OnLeaderboardPressed;
		backButton.Pressed += OnBackPressed;
		leaderboardBackButton.Pressed += OnLeaderboardBackPressed;
		volumeSlider.ValueChanged += OnVolumeChanged;
		resolutionOption.ItemSelected += OnResolutionSelected;
		fullscreenToggle.Toggled += OnFullscreenToggled;

		GetWindow().SizeChanged += OnWindowSizeChanged;

		// Init settings values
		int masterIndex = AudioServer.GetBusIndex("Master");
		volumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(masterIndex));
		fullscreenToggle.ButtonPressed = (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen);

		Vector2I curSize = GetWindow().Size;
		for (int i = 0; i < resolutionOption.ItemCount; i++)
		{
			string txt = resolutionOption.GetItemText(i);
			string[] dims = txt.Split('x');
			if (dims.Length == 2 &&
				int.TryParse(dims[0].Trim(), out int w) &&
				int.TryParse(dims[1].Trim(), out int h) &&
				w == curSize.X && h == curSize.Y)
			{
				resolutionOption.Select(i);
				break;
			}
		}

		UpdateScreenCenter();
		ResetBackgroundPosition();

		// Hide panels on start
		settingsPanel.Visible = false;
		leaderboardPanel.Visible = false;
	}

	public override void _Process(double delta)
	{
		if (animatedSpriteBG != null)
		{
			Vector2 mousePos = GetViewport().GetMousePosition();
			Vector2 targetOffset = (screenCenter - mousePos) * 0.025f;

			Tween tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Sine);
			tween.TweenProperty(animatedSpriteBG, "position", targetOffset, 0.7f);
		}
	}

	private void OnStartPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/LevelSelect.tscn");
	}

	private void OnSettingsPressed()
	{
		mainButtons.Visible = false;
		settingsPanel.Visible = true;
	}

	private void OnLeaderboardPressed()
	{
		mainButtons.Visible = false;
		leaderboardPanel.Visible = true;
		UpdateLeaderboard();
	}

	private void UpdateLeaderboard()
	{
		string[] levelNames = new string[]
		{
			"The City",
			"The Island",
			"The Volcano",
			"The Pizzeria?",
		};

		for (int i = 0; i < levelNames.Length; i++)
		{
			int level = i + 1;
			int totalWaves = GameData.Instance.GetTotalWavesForLevel(level);
			var label = GetNodeOrNull<Label>($"CenterContainer/LeaderboardPanel/Level{level}Score");
			if (label != null)
				label.Text = $"{levelNames[i]}: {totalWaves} waves survived";
		}
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}

	private void OnBackPressed()
	{
		settingsPanel.Visible = false;
		mainButtons.Visible = true;
	}

	private void OnLeaderboardBackPressed()
	{
		leaderboardPanel.Visible = false;
		mainButtons.Visible = true;
	}

	private void OnVolumeChanged(double value)
	{
		int masterIndex = AudioServer.GetBusIndex("Master");
		AudioServer.SetBusVolumeDb(masterIndex, Mathf.LinearToDb((float)value));
	}

	private void OnResolutionSelected(long index)
	{
		string text = resolutionOption.GetItemText((int)index);
		string[] parts = text.Split('x');
		if (parts.Length == 2 &&
			int.TryParse(parts[0].Trim(), out int w) &&
			int.TryParse(parts[1].Trim(), out int h))
		{
			Vector2I newSize = new Vector2I(w, h);
			GetWindow().Size = newSize;
			Vector2I screenSize = DisplayServer.ScreenGetSize();
			GetWindow().Position = (screenSize - newSize) / 2;
			UpdateScreenCenter();
		}
	}

	private void OnFullscreenToggled(bool toggledOn)
	{
		if (toggledOn)
		{
			Vector2I desktopSize = DisplayServer.ScreenGetSize();
			GetWindow().Size = desktopSize;
			GetWindow().Position = Vector2I.Zero;
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			GetWindow().Size = new Vector2I(1920, 1080);
			GetWindow().Position = (DisplayServer.ScreenGetSize() - GetWindow().Size) / 2;
		}

		UpdateScreenCenter();
	}

	private void UpdateScreenCenter()
	{
		screenCenter = GetViewportRect().Size / 2f;
	}

	private void ResetBackgroundPosition()
	{
		if (animatedSpriteBG != null)
		{
			animatedSpriteBG.Position = new Vector2(-400, -200);
		}
	}

	private void OnWindowSizeChanged()
	{
		UpdateScreenCenter();
		ResetBackgroundPosition();
	}
}
