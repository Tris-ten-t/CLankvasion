using Godot;

[GlobalClass]                    // ← This line is very important
public partial class WaveData : Resource
{
	[Export] public int WaveNumber = 1;
	[Export] public int TotalEnemies = 10;
	[Export] public int RollerCount = 4;
	[Export] public int ClankCount = 3;
	[Export] public int BullCount = 2;
	[Export] public int ClinkCount = 1;
	[Export] public int ClunkCount = 0;

	[Export] public float SpawnInterval = 1.2f;
}
