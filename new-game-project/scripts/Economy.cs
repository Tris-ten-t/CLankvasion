using Godot;

public static class Economy
{
	public static int Coins { get; private set; } = 0;

	public static void AddCoins(int amount)
	{
		Coins += amount;
		GD.Print($"[Economy] +{amount} coins → Total: {Coins}");
	}

	public static void Reset()
	{
		Coins = 0;
	}
}
