using Godot;
using System;
using System.Collections.Generic;

public partial class HealthHud : HBoxContainer{
    
    public const string NodeName = nameof(HealthHud);

    TextureRect[] heartSprites;
    private Health health;
    [Export] PackedScene heartSprite;

    public override void _Ready(){
        base._Ready();
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
    }

    private void UpdateSprites(){
        for(int i = 0; i < heartSprites.Length; i++){
            heartSprites[i].Visible = false;
        }
        for(int i = 0; i < health.Value; i++){
            heartSprites[i].Visible = true;
        }   
    }

    public void LinkEvents(Health health){
        
        if(health != null){
            UnlinkEvents();
        }
        
        heartSprites   = new TextureRect[health.Max];
        
        for(int i = 0; i < health.Max; i++){
            TextureRect sprite = (TextureRect)heartSprite.Instantiate();
            AddChild(sprite);
            sprite.Visible = false;
            heartSprites[i] = sprite;
        }
        
        health.OnDamage += UpdateSprites;
        health.OnHeal   += UpdateSprites;
        this.health = health;

        UpdateSprites();
    }

    public void UnlinkEvents(){
        
        if(heartSprites != null){
            for(int i = 0; i < heartSprites.Length; i++){
                heartSprites[i].QueueFree();
            }
        }
        
        heartSprites = null;
        if(health != null){
            health.OnDamage -= UpdateSprites;
            health.OnHeal   -= UpdateSprites;
        }
    }
}
