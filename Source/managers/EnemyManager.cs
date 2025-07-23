using Godot;
using System;

public partial class EnemyManager : Node{
    
    public static EnemyManager Instance {get;private set;}

    private Entropek.Collections.SwapbackList<Enemy> enemies = new Entropek.Collections.SwapbackList<Enemy>();

    public override void _EnterTree(){
        base._EnterTree();
        Instance = this;
    }

    public override void _ExitTree(){
        base._ExitTree();
    }

    public void AddEnemy(Enemy enemy){
        enemies.Add(enemy);
    }

    public void RemoveEnemy(Enemy enemy){
        enemies.Remove(enemy);
    }
}
