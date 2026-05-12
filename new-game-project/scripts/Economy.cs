using Godot;

public static class Economy
{
	public static int Coins { get; private set; } = 9999999;   // Infinite money for testing

	public static void AddCoins(int amount)
	{
		if (amount < 0)
		{
			GD.Print("Infinite money mode - purchase allowed");
			return; // Don't subtract money
		}

		Coins += amount;
		GD.Print($"Coins updated: {Coins}");
	}
}
