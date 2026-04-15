using Godot;

public partial class ShopMenu : CanvasLayer
{
	private Label itemNameLabel;
	private Label itemDescriptionLabel;
	private Label itemCostLabel;
	private Button buyButton;

	private string selectedItem = "";

	public override void _Ready()
	{
		GD.Print("Shop Menu opened");

		Layer = 128;
		GetTree().Paused = true;
		ProcessMode = ProcessModeEnum.Always;

		itemNameLabel = GetNodeOrNull<Label>("ItemDetails/ItemNameLabel");
		itemDescriptionLabel = GetNodeOrNull<Label>("ItemDetails/ItemDescriptionLabel");
		itemCostLabel = GetNodeOrNull<Label>("ItemDetails/ItemCostLabel");
		buyButton = GetNodeOrNull<Button>("ItemDetails/BuyButton");

		if (buyButton != null)
			buyButton.Pressed += OnBuyPressed;

		var closeButton = GetNodeOrNull<Button>("CloseButton");
		if (closeButton != null)
			closeButton.Pressed += OnCloseButtonPressed;
		else
			GD.PrintErr("CloseButton not found!");

		ClearDetails();
	}

	public void _on_mine_button_pressed()
	{
		selectedItem = "Mine";

		if (itemNameLabel != null) itemNameLabel.Text = "Mine Tower";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Explosive trap that deals moderate area damage when an enemy touches it.\nGreat for defending narrow paths.";
		if (itemCostLabel != null) itemCostLabel.Text = "Cost: 25 coins";

		if (buyButton != null)
			buyButton.Disabled = false;   // Always enabled with infinite money

		GD.Print("Mine button clicked - details updated");
	}

	private void OnBuyPressed()
	{
		if (selectedItem == "Mine")
		{
			Economy.AddCoins(-25);  // This does nothing because of infinite mode
			GD.Print("Mine purchased! Entering placement mode...");

			CloseMenu();

			var mainTower = GetTree().CurrentScene.GetNodeOrNull<MainTower>("MainTower");
			if (mainTower != null)
			{
				mainTower.StartMinePlacement();
			}
			else
			{
				GD.PrintErr("MainTower not found!");
			}
		}
	}

	private void ClearDetails()
	{
		if (itemNameLabel != null) itemNameLabel.Text = "";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Select an item from the left to see details.";
		if (itemCostLabel != null) itemCostLabel.Text = "";
		if (buyButton != null) buyButton.Disabled = false;
	}

	private void OnCloseButtonPressed()
	{
		CloseMenu();
	}

	private void CloseMenu()
	{
		GetTree().Paused = false;
		QueueFree();
		GD.Print("Shop menu closed");
	}
}
