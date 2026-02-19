using Godot;

public partial class MainTower : AnimatedSprite2D
{
	[Export] public float RotationSpeed = 8.0f;  // adjust 4–12 range, higher = snappier

	public override void _Process(double delta)
	{
		Vector2 mousePos = GetGlobalMousePosition();
		
		// Direction vector from tower → mouse
		Vector2 direction = mousePos - GlobalPosition;
		
		// Get the target angle (in radians)
		float targetAngle = direction.Angle();
		
		// Apply the 90° offset here (most towers face UP by default)
		targetAngle -= Mathf.Pi / 2;           // -90° → now "up" is correct
		
		// Smoothly interpolate current rotation toward target
		Rotation = Mathf.LerpAngle(Rotation, targetAngle, (float)delta * RotationSpeed);
	}
}
