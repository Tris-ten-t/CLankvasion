using Godot;

public partial class ShopMenu : CanvasLayer
{
	private Label itemNameLabel;
	private Label itemDescriptionLabel;
	private Label itemCostLabel;
	private Button buyButton;
	private TextureRect itemIcon;

	private string selectedItem = "";

	// Item costs
	private const int MineCost = 25;
	private const int WaterTankCost = 60;
	private const int EmpCost = 75;
	private const int TrashCompactorCost = 120;

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
		itemIcon = GetNodeOrNull<TextureRect>("ItemDetails/ItemIcon");

		if (buyButton != null)
			buyButton.Pressed += OnBuyPressed;

		var closeButton = GetNodeOrNull<Button>("CloseButton");
		if (closeButton != null)
			closeButton.Pressed += OnCloseButtonPressed;
		else
			GD.PrintErr("CloseButton not found!");

		// Update all item buttons based on current coins
		UpdateShopButtons();
		ClearDetails();
	}

	private void UpdateShopButtons()
	{
		int coins = Economy.Coins;

		// Grey out item buttons if player can't afford them
		SetItemButtonAffordable("MineButton", coins >= MineCost);
		SetItemButtonAffordable("WaterTankButton", coins >= WaterTankCost);
		SetItemButtonAffordable("EmpButton", coins >= EmpCost);
		SetItemButtonAffordable("TrashCompactorButton", coins >= TrashCompactorCost);
	}

	private void SetItemButtonAffordable(string buttonName, bool canAfford)
	{
		var button = GetNodeOrNull<Button>(buttonName);
		if (button != null)
		{
			button.Disabled = !canAfford;
			// Modulate the button to visually show it's unaffordable
			button.Modulate = canAfford ? new Color(1, 1, 1, 1) : new Color(0.5f, 0.5f, 0.5f, 0.7f);
		}
	}

	public void _on_mine_button_pressed()
	{
		selectedItem = "Mine";
		if (itemNameLabel != null) itemNameLabel.Text = "Mine Tower";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Explosive trap that deals moderate area damage when an enemy touches it.\nGreat for defending narrow paths.";
		if (itemCostLabel != null) itemCostLabel.Text = $"Cost: {MineCost} coins";

		if (itemIcon != null)
		{
			var atlas = GD.Load<AtlasTexture>("res://assets/towers/MineIcon.tres");
			if (atlas != null) { itemIcon.Texture = atlas; itemIcon.Visible = true; }
		}

		// Only enable buy button if player can afford it
		if (buyButton != null)
			buyButton.Disabled = Economy.Coins < MineCost;
	}

	public void _on_water_tank_button_pressed()
	{
		selectedItem = "WaterTank";
		if (itemNameLabel != null) itemNameLabel.Text = "Water Tank";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Deals low damage over time in an area for 25 seconds.\nExcellent for sustained area control.";
		if (itemCostLabel != null) itemCostLabel.Text = $"Cost: {WaterTankCost} coins";

		if (itemIcon != null)
		{
			var atlas = GD.Load<AtlasTexture>("res://assets/towers/WaterTankIcon.tres");
			if (atlas != null) { itemIcon.Texture = atlas; itemIcon.Visible = true; }
		}

		if (buyButton != null)
			buyButton.Disabled = Economy.Coins < WaterTankCost;
	}

	public void _on_emp_button_pressed()
	{
		selectedItem = "Emp";
		if (itemNameLabel != null) itemNameLabel.Text = "EMP Tower";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Stuns all enemies in a large area for 5 seconds.\nSingle use - breaks after activation.";
		if (itemCostLabel != null) itemCostLabel.Text = $"Cost: {EmpCost} coins";

		if (itemIcon != null)
		{
			var atlas = GD.Load<AtlasTexture>("res://assets/towers/EmpIcon.tres");
			if (atlas != null) { itemIcon.Texture = atlas; itemIcon.Visible = true; }
		}

		if (buyButton != null)
			buyButton.Disabled = Economy.Coins < EmpCost;
	}

	public void _on_trash_compactor_button_pressed()
	{
		selectedItem = "TrashCompactor";
		if (itemNameLabel != null) itemNameLabel.Text = "Trash Compactor";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Draws in the first enemy that touches it and eliminates it instantly.\nSingle use - high cost.";
		if (itemCostLabel != null) itemCostLabel.Text = $"Cost: {TrashCompactorCost} coins";

		if (itemIcon != null)
		{
			var atlas = GD.Load<AtlasTexture>("res://assets/towers/TrashCompactorIcon.tres");
			if (atlas != null) { itemIcon.Texture = atlas; itemIcon.Visible = true; }
		}

		if (buyButton != null)
			buyButton.Disabled = Economy.Coins < TrashCompactorCost;
	}

	private void OnBuyPressed()
	{
		var mainTower = GetTree().CurrentScene.GetNodeOrNull<MainTower>("MainTower");

		if (selectedItem == "Mine" && Economy.Coins >= MineCost)
		{
			Economy.AddCoins(-MineCost);
			CloseMenu();
			if (mainTower != null) mainTower.StartMinePlacement();
		}
		else if (selectedItem == "WaterTank" && Economy.Coins >= WaterTankCost)
		{
			Economy.AddCoins(-WaterTankCost);
			CloseMenu();
			if (mainTower != null) mainTower.StartWaterTankPlacement();
		}
		else if (selectedItem == "Emp" && Economy.Coins >= EmpCost)
		{
			Economy.AddCoins(-EmpCost);
			CloseMenu();
			if (mainTower != null) mainTower.StartEmpPlacement();
		}
		else if (selectedItem == "TrashCompactor" && Economy.Coins >= TrashCompactorCost)
		{
			Economy.AddCoins(-TrashCompactorCost);
			GD.Print("Trash Compactor purchased! Entering placement mode...");
			CloseMenu();
			if (mainTower != null) mainTower.StartTrashCompactorPlacement();
		}
	}

	private void ClearDetails()
	{
		if (itemNameLabel != null) itemNameLabel.Text = "";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Select an item from the left to see details.";
		if (itemCostLabel != null) itemCostLabel.Text = "";
		if (buyButton != null) buyButton.Disabled = true;
		if (itemIcon != null) itemIcon.Visible = false;
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
