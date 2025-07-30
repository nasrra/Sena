using Godot;
using System;

public partial class AudioBankLoader : Node{
    [Export] string bankName;
    [Export] bool unloadBankOnExitTree = false;

    public override void _Ready(){
        base._Ready();
        if(bankName != "" && bankName != null && AudioManager.Singleton.IsBankLoaded(bankName)==false){
            AudioManager.Singleton.LoadBank(bankName);
        }
    }

    public override void _ExitTree(){
        base._ExitTree();
        if(unloadBankOnExitTree==true){
            AudioManager.Singleton.UnloadBank(bankName);
        }
    }
}
