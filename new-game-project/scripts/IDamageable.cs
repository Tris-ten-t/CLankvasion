// NEW: Create this new C# script file (e.g., IDamageable.cs) anywhere in your project
// Right-click in FileSystem > New Script > C# > Paste this
using Godot;

public interface IDamageable
{
	void TakeDamage(int damage);
}
