using Godot;
using System;

public partial class TimedEnemySpawner : EnemySpawner{
    [ExportGroup(nameof(TimedEnemySpawner))]
    [Export] private Timer timer;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    private void LinkEvents(){
        timer.Timeout += SpawnEnemy;
    }

    private void UnlinkEvents(){
        timer.Timeout -= SpawnEnemy;
    }
}
