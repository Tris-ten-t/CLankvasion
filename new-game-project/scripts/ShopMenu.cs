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

		GD.Print("itemNameLabel: ", itemNameLabel);
		GD.Print("itemDescriptionLabel: ", itemDescriptionLabel);
		GD.Print("itemCostLabel: ", itemCostLabel);
		GD.Print("buyButton: ", buyButton);

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
			buyButton.Disabled = Economy.Coins < 25;
		GD.Print("Mine button clicked - details updated");
	}
	private void OnBuyPressed()
	{
		if (selectedItem == "Mine" && Economy.Coins >= 25)
		{
			Economy.AddCoins(-25);
			GD.Print("Mine Tower purchased!");
			CloseMenu();
		}
		else
		{
			GD.Print("Not enough coins!");
		}
	}
	private void ClearDetails()
	{
		if (itemNameLabel != null) itemNameLabel.Text = "";
		if (itemDescriptionLabel != null) 
			itemDescriptionLabel.Text = "Select an item from the left to see details.";
		if (itemCostLabel != null) itemCostLabel.Text = "";
		if (buyButton != null) buyButton.Disabled = true;
	}
	private void OnCloseButtonPressed()
	{
		GetTree().Paused = false;
		QueueFree();
		GD.Print("Shop menu closed");
	}
	private void CloseMenu()
	{
		GetTree().Paused = false;
		QueueFree();
		GD.Print("Shop menu closed");
	}
}
