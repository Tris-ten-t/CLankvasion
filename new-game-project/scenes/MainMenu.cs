using Godot;
using System;

public partial class MainMenu : Control
{
	// Node references
	private Button startButton;
	private Button settingsButton;
	private Button quitButton;
	private VBoxContainer mainButtons;
	private VBoxContainer settingsPanel;
	private HSlider volumeSlider;
	private OptionButton resolutionOption;
	private CheckButton fullscreenToggle;
	private Button backButton;
	private TextureRect animatedSpriteBG;

	private Vector2 screenCenter;

	public override void _Ready()
	{
		// Get nodes
		startButton = GetNode<Button>("CenterContainer/MainButtons/StartButton");
		settingsButton = GetNode<Button>("CenterContainer/MainButtons/SettingsButton");
		quitButton = GetNode<Button>("CenterContainer/MainButtons/QuitButton");
		mainButtons = GetNode<VBoxContainer>("CenterContainer/MainButtons");
		settingsPanel = GetNode<VBoxContainer>("CenterContainer/SettingsPanel");
		volumeSlider = GetNode<HSlider>("CenterContainer/SettingsPanel/VolumeSlider");
		resolutionOption = GetNode<OptionButton>("CenterContainer/SettingsPanel/ResolutionOption");
		fullscreenToggle = GetNode<CheckButton>("CenterContainer/SettingsPanel/FullscreenToggle");
		backButton = GetNode<Button>("CenterContainer/SettingsPanel/BackButton");
		animatedSpriteBG = GetNode<TextureRect>("AnimatedSpriteBG");

		// Connect signals
		startButton.Pressed += OnStartPressed;
		settingsButton.Pressed += OnSettingsPressed;
		quitButton.Pressed += OnQuitPressed;
		backButton.Pressed += OnBackPressed;
		volumeSlider.ValueChanged += OnVolumeChanged;
		resolutionOption.ItemSelected += OnResolutionSelected;
		fullscreenToggle.Toggled += OnFullscreenToggled;

		// Connect window resize signal (using GetWindow() for the current window)
		GetWindow().SizeChanged += OnWindowSizeChanged;

		// Init settings values
		int masterIndex = AudioServer.GetBusIndex("Master");
		volumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(masterIndex));

		// Correct way to check current mode
		fullscreenToggle.ButtonPressed = (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen);

		// Select current resolution in dropdown
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

		// Setup for animation
		UpdateScreenCenter();
		ResetBackgroundPosition();
	}

	public override void _Process(double delta)
	{
		if (animatedSpriteBG != null)
		{
			Vector2 mousePos = GetViewport().GetMousePosition();
			Vector2 targetOffset = (screenCenter - mousePos) * 0.025f;  // Tweak multiplier for intensity

			// Optional auto-drift (uncomment for extra animation)
			// targetOffset.X += Mathf.Sin((float)Time.GetTicksMsec() / 2000f) * 30f;
			// targetOffset.Y += Mathf.Cos((float)Time.GetTicksMsec() / 3000f) * 20f;

			Tween tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Sine);
			tween.TweenProperty(animatedSpriteBG, "position", targetOffset, 0.7f);
		}
	}

	private void OnStartPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/area.tscn");  // Update this path to your game scene
	}

	private void OnSettingsPressed()
	{
		mainButtons.Visible = false;
		settingsPanel.Visible = true;
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

			// Set the new window size (reliable in Godot 4)
			GetWindow().Size = newSize;

			// Optional: Center the window nicely on screen
			Vector2I screenSize = DisplayServer.ScreenGetSize();
			GetWindow().Position = (screenSize - newSize) / 2;

			// Update animation center
			UpdateScreenCenter();
		}
	}

	private void OnFullscreenToggled(bool toggledOn)
	{
	if (toggledOn)
	{
		// Prep for fullscreen: Set to desktop resolution
		Vector2I desktopSize = DisplayServer.ScreenGetSize();
		GetWindow().Size = desktopSize;
		GetWindow().Position = Vector2I.Zero;  // Top-left corner

		// True fullscreen (hides taskbar, no borders)
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
	}
	else
	{
		// Back to windowed: Restore your preferred size (e.g., 1920x1080)
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		GetWindow().Size = new Vector2I(1920, 1080);  // ← Change to your base res
		GetWindow().Position = (DisplayServer.ScreenGetSize() - GetWindow().Size) / 2;  // Center it
	}

	// Update background animation center
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
			animatedSpriteBG.Position = new Vector2(-400, -200);  // Match your scene's initial offset
		}
	}

	private void OnWindowSizeChanged()
	{
		UpdateScreenCenter();
		ResetBackgroundPosition();  // Optional: Reset on resize
	}
}
