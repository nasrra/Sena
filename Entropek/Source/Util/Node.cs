using Godot;
using System;

namespace Entropek.Util;

public static class Node{
    public static void VerifyName(Godot.Node node, string nodeName){
        if(node.Name != nodeName){
            GD.PushError($"{node.GetPath()} is not named {nodeName}.");
        }
    }
}
