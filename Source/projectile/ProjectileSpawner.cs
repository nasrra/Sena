using Godot;
using System;

public partial class ProjectileSpawner : Node2D{
    [Export] private PackedScene projectilePackedScene;

    public void Fire(Vector2 direction){
        Projectile projectile = (Projectile)projectilePackedScene.Instantiate();
        projectile.GlobalPosition = GlobalPosition;
        projectile.Movement.Move(direction);
        ProjectileManager.Instance.AddChild(projectile);
    }

    public void Fire(){
        Projectile projectile = (Projectile)projectilePackedScene.Instantiate();
        projectile.GlobalPosition = GlobalPosition;
        projectile.Movement.Move(-Transform.Y);
        ProjectileManager.Instance.AddChild(projectile);
    }
}
