using Godot;
using System;

public partial class EmberNotchChainHud : HBoxContainer{
    
    public const string NodeName = nameof(EmberNotchChainHud);

    EmberNotchGui[] notches;
    [Export] private PackedScene notchPackedScene;

    public override void _Ready(){
        base._Ready();
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
    }

    private void UpdateNotches(int filledNotches, int remainderAmount){
        int i = 0;

        // filled notches.

        for(i = 0; i < filledNotches; i++){
            notches[i].UpdateFill(EmberStorage.NotchMaxEmberValue);
        }

        if(i>=notches.Length){
            return;
        }
        // the remainder notch / next to be filled.

        notches[i++].UpdateFill(remainderAmount);
    
        // the rest that are empty.

        for(int j = i; j < notches.Length; j++){
            notches[j].UpdateFill(0);
        }
    }

    public void LinkToEmberStorage(EmberStorage emberStorage){

        notches = new EmberNotchGui[emberStorage.MaxNotchAmount];
        for(int i = 0; i < notches.Length; i++){
            EmberNotchGui notch = (EmberNotchGui)notchPackedScene.Instantiate();
            AddChild(notch);
            notches[i] = notch;
        }
        
        emberStorage.OnNotchesUpdated += UpdateNotches;
        UpdateNotches(emberStorage.NotchAmount, emberStorage.RemainderNotchEmberValue);
    }

    public void UnlinkFromEmberStorage(EmberStorage emberStorage){

        for(int i = 0; i < notches.Length; i++){
            notches[i].QueueFree();
        }
        notches = null;

        emberStorage.OnNotchesUpdated -= UpdateNotches;
    }
}
