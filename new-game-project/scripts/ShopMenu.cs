using Godot;

public partial class ShopMenu : CanvasLayer
{
	private Label itemNameLabel;
	private Label itemDescriptionLabel;
	private Label itemCostLabel;
	private Button buyButton;
	private TextureRect itemIcon;

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
		itemIcon = GetNodeOrNull<TextureRect>("ItemDetails/ItemIcon");

		if (buyButton != null)
			buyButton.Pressed += OnBuyPressed;

		var closeButton = GetNodeOrNull<Button>("CloseButton");
		if (closeButton != null)
			closeButton.Pressed += OnCloseButtonPressed;
		else
			GD.PrintErr("CloseButton not found!");

		ClearDetails();
	}

	// Mine
	public void _on_mine_button_pressed()
	{
		selectedItem = "Mine";
		if (itemNameLabel != null) itemNameLabel.Text = "Mine Tower";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Explosive trap that deals moderate area damage when an enemy touches it.\nGreat for defending narrow paths.";
		if (itemCostLabel != null) itemCostLabel.Text = "Cost: 25 coins";

		if (itemIcon != null)
		{
			var atlas = GD.Load<AtlasTexture>("res://assets/icons/MineIcon.tres");
			if (atlas != null) { itemIcon.Texture = atlas; itemIcon.Visible = true; }
		}
		if (buyButton != null) buyButton.Disabled = false;
	}

	// Water Tank
	public void _on_water_tank_button_pressed()
	{
		selectedItem = "WaterTank";
		if (itemNameLabel != null) itemNameLabel.Text = "Water Tank";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Deals low damage over time in an area for 25 seconds.\nExcellent for sustained area control.";
		if (itemCostLabel != null) itemCostLabel.Text = "Cost: 60 coins";

		if (itemIcon != null)
		{
			var atlas = GD.Load<AtlasTexture>("res://assets/icons/WaterTankIcon.tres");
			if (atlas != null) { itemIcon.Texture = atlas; itemIcon.Visible = true; }
		}
		if (buyButton != null) buyButton.Disabled = false;
	}

	// Emp (NEW)
	public void _on_emp_button_pressed()
	{
		selectedItem = "Emp";
		if (itemNameLabel != null) itemNameLabel.Text = "EMP Tower";
		if (itemDescriptionLabel != null)
			itemDescriptionLabel.Text = "Stuns all enemies in a large area for 5 seconds.\nSingle use - breaks after activation.";
		if (itemCostLabel != null) itemCostLabel.Text = "Cost: 75 coins";

		if (itemIcon != null)
		{
			var atlas = GD.Load<AtlasTexture>("res://assets/towers/EmpIcon.tres");
			if (atlas != null) { itemIcon.Texture = atlas; itemIcon.Visible = true; }
		}
		if (buyButton != null) buyButton.Disabled = false;
	}

	private void OnBuyPressed()
	{
		var mainTower = GetTree().CurrentScene.GetNodeOrNull<MainTower>("MainTower");

		if (selectedItem == "Mine")
		{
			Economy.AddCoins(-25);
			CloseMenu();
			if (mainTower != null) mainTower.StartMinePlacement();
		}
		else if (selectedItem == "WaterTank")
		{
			Economy.AddCoins(-60);
			CloseMenu();
			if (mainTower != null) mainTower.StartWaterTankPlacement();
		}
		else if (selectedItem == "Emp")
		{
			Economy.AddCoins(-75);
			CloseMenu();
			if (mainTower != null) mainTower.StartEmpPlacement();
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
