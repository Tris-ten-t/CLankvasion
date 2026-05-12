using Godot;

public partial class ShopMenu : CanvasLayer
{
	private Label itemNameLabel;
	private Label itemDescriptionLabel;
	private Label itemCostLabel;
	private Button buyButton;
	private TextureRect itemIcon;
	private Button upgradeButton;
	private Button shieldButton;

	private string selectedItem = "";

	// Item costs
	private const int MineCost = 25;
	private const int WaterTankCost = 60;
	private const int EmpCost = 75;
	private const int TrashCompactorCost = 120;

	private MainTower _mainTower;

	public override void _Ready()
	{
		GD.Print("Shop Menu opened");

		Layer = 128;
		GetTree().Paused = true;
		ProcessMode = ProcessModeEnum.Always;

		_mainTower = GetTree().CurrentScene.GetNodeOrNull<MainTower>("MainTower");

		itemNameLabel = GetNodeOrNull<Label>("ItemDetails/ItemNameLabel");
		itemDescriptionLabel = GetNodeOrNull<Label>("ItemDetails/ItemDescriptionLabel");
		itemCostLabel = GetNodeOrNull<Label>("ItemDetails/ItemCostLabel");
		buyButton = GetNodeOrNull<Button>("ItemDetails/BuyButton");
		itemIcon = GetNodeOrNull<TextureRect>("ItemDetails/ItemIcon");

		upgradeButton = GetNodeOrNull<Button>("ItemList/UpgradeButton");
		shieldButton = GetNodeOrNull<Button>("ItemList/ShieldButton");

		if (buyButton != null)
			buyButton.Pressed += OnBuyPressed;

		if (upgradeButton != null)
			upgradeButton.Pressed += OnUpgradePressed;
		else
			GD.PrintErr("UpgradeButton not found!");

		if (shieldButton != null)
			shieldButton.Pressed += OnShieldPressed;
		else
			GD.PrintErr("ShieldButton not found!");

		var closeButton = GetNodeOrNull<Button>("CloseButton");
		if (closeButton != null)
			closeButton.Pressed += OnCloseButtonPressed;
		else
			GD.PrintErr("CloseButton not found!");

		UpdateShopButtons();
		UpdateUpgradeButton();
		UpdateShieldButton();
		ClearDetails();
	}

	private void UpdateShopButtons()
	{
		int coins = Economy.Coins;
		SetItemButtonAffordable("ItemList/MineButton", coins >= MineCost);
		SetItemButtonAffordable("ItemList/WaterTankButton", coins >= WaterTankCost);
		SetItemButtonAffordable("ItemList/EmpButton", coins >= EmpCost);
		SetItemButtonAffordable("ItemList/TrashCompactorButton", coins >= TrashCompactorCost);
	}

	private void UpdateUpgradeButton()
	{
		if (upgradeButton == null || _mainTower == null) return;

		if (!_mainTower.CanUpgrade())
		{
			upgradeButton.Text = "Tower Max Level!";
			upgradeButton.Disabled = true;
			upgradeButton.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.7f);
			return;
		}

		int cost = _mainTower.GetUpgradeCost();
		int level = _mainTower.GetTowerLevel();
		bool canAfford = Economy.Coins >= cost;

		upgradeButton.Text = $"Upgrade Tower to Lvl {level + 1} ({cost} coins)";
		upgradeButton.Disabled = !canAfford;
		upgradeButton.Modulate = canAfford ? new Color(1, 1, 1, 1) : new Color(0.5f, 0.5f, 0.5f, 0.7f);
	}

	private void UpdateShieldButton()
	{
		if (shieldButton == null || _mainTower == null) return;

		int cost = _mainTower.GetShieldCost();
		bool canAfford = Economy.Coins >= cost;

		shieldButton.Text = $"Buy Shield +25 ({cost} coins)";
		shieldButton.Disabled = !canAfford;
		shieldButton.Modulate = canAfford ? new Color(1, 1, 1, 1) : new Color(0.5f, 0.5f, 0.5f, 0.7f);
	}

	private void SetItemButtonAffordable(string buttonName, bool canAfford)
	{
		var button = GetNodeOrNull<Button>(buttonName);
		if (button != null)
		{
			button.Disabled = !canAfford;
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

	private void OnUpgradePressed()
	{
		if (_mainTower == null) return;
		_mainTower.UpgradeTower();
		UpdateUpgradeButton();
		UpdateShopButtons();
		GD.Print($"Tower upgraded to level {_mainTower.GetTowerLevel()}");
	}

	private void OnShieldPressed()
	{
		if (_mainTower == null) return;
		_mainTower.BuyShield();
		UpdateShieldButton();
		UpdateShopButtons();
		GD.Print("Shield purchased!");
	}

	private void OnBuyPressed()
	{
		if (selectedItem == "Mine" && Economy.Coins >= MineCost)
		{
			Economy.AddCoins(-MineCost);
			CloseMenu();
			if (_mainTower != null) _mainTower.StartMinePlacement();
		}
		else if (selectedItem == "WaterTank" && Economy.Coins >= WaterTankCost)
		{
			Economy.AddCoins(-WaterTankCost);
			CloseMenu();
			if (_mainTower != null) _mainTower.StartWaterTankPlacement();
		}
		else if (selectedItem == "Emp" && Economy.Coins >= EmpCost)
		{
			Economy.AddCoins(-EmpCost);
			CloseMenu();
			if (_mainTower != null) _mainTower.StartEmpPlacement();
		}
		else if (selectedItem == "TrashCompactor" && Economy.Coins >= TrashCompactorCost)
		{
			Economy.AddCoins(-TrashCompactorCost);
			CloseMenu();
			if (_mainTower != null) _mainTower.StartTrashCompactorPlacement();
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
