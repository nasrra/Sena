using Entropek.Ai;
using Godot;

public partial class EnvironmentDoor : Door{
    [Export]
    private WayfindingStaticObstacle2D wayfindingObstacle;
    public override void Open(){
        base.Open();
        wayfindingObstacle.Disable();
    }

    public override void Close(){
        base.Close();
        wayfindingObstacle.Enable();
    }
}
