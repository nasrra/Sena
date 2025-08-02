using Godot;
using System;

public partial class EnemySpawner : Node2D{
    [ExportGroup(nameof(EnemySpawner))]
    [Export] PackedScene packedScene;

    public void SpawnEnemy(){
        Enemy enemy = (Enemy)packedScene.Instantiate();
        enemy.GlobalPosition = GlobalPosition;
        enemy.Target = Player.Instance;
        // SceneManager.Instance.Current2DScene.AddChild(enemy);
        EntityManager.Singleton.AddChild(enemy);
    }
}
