using Godot;

public partial class ShopMenu : CanvasLayer
{
	// References (set in editor)
	private Label itemNameLabel;
	private Label itemDescriptionLabel;
	private Label itemCostLabel;
	private Button buyButton;

	private string selectedItem = "";

	public override void _Ready()
	{
		itemNameLabel = GetNodeOrNull<Label>("ItemDetails/ItemNameLabel");
		itemDescriptionLabel = GetNodeOrNull<Label>("ItemDetails/ItemDescriptionLabel");
		itemCostLabel = GetNodeOrNull<Label>("ItemDetails/ItemCostLabel");
		buyButton = GetNodeOrNull<Button>("ItemDetails/BuyButton");

		if (buyButton != null)
			buyButton.Pressed += OnBuyPressed;

		// Start with nothing selected
		ClearDetails();
	}

	// Called when player clicks "Mine Tower" button
	public void OnMineButtonPressed()
	{
		selectedItem = "Mine";
		itemNameLabel.Text = "Mine Tower";
		itemDescriptionLabel.Text = "Explosive trap. Deals moderate area damage when an enemy touches it.";
		itemCostLabel.Text = "Cost: 25 coins";
		buyButton.Disabled = Economy.Coins < 25;
	}

	private void OnBuyPressed()
	{
		if (selectedItem == "Mine" && Economy.Coins >= 25)
		{
			Economy.AddCoins(-25);
			GD.Print("Mine purchased! (Placement logic will be added next)");
			// TODO: Switch to placement mode
			QueueFree(); // Close menu after purchase for now
		}
	}

	private void ClearDetails()
	{
		if (itemNameLabel != null) itemNameLabel.Text = "";
		if (itemDescriptionLabel != null) itemDescriptionLabel.Text = "Select an item to see details.";
		if (itemCostLabel != null) itemCostLabel.Text = "";
		if (buyButton != null) buyButton.Disabled = true;
	}

	// Close button
	public void OnClosePressed()
	{
		QueueFree();
	}
}
