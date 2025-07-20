using Godot;
using System;

namespace Entropek.Ai;

public partial class NavigationGrid2D : Node2D{
    [Export] TileMapLayer tileMap;
    CellData[,] cells;
    private bool drawDebug = true;
    private bool hideTiles = true;

    private Color debugBlockedColour        = new Color(1,0,0);
    private Color debugTraversableColour    = new Color(1,1,1);
    private Color debugPassableColour       = new Color(0.5f,0.5f,0);
    private Vector2 debugSquareSize         = new Vector2(4,4);

    public override void _Ready(){
        base._Ready();
        if(hideTiles == true){
            Shader shader = new Shader();
            shader.Code = "shader_type canvas_item; void fragment() { discard; }";
            ShaderMaterial material = new ShaderMaterial();
            material.Shader = shader;
            tileMap.Material = material;
        }
        InitialiseCells();
    }


    private void InitialiseCells(){ // <-- inits static environment.

        int tileMapSizeX = tileMap.GetUsedRect().Size.X;
        int tileMapSizeY = tileMap.GetUsedRect().Size.Y;

        cells = new CellData[tileMapSizeX, tileMapSizeY];

        // offset to the top left of the grid.
        // so that we scan from to left to right on each row for each column.
        // starting from the top left of the grid and ending at the bottom right.

        for(int x = 0; x < tileMapSizeX; x++){
            int globalX = x + tileMap.GetUsedRect().Position.X;
            for(int y = 0; y < tileMapSizeY; y++){
                int globalY = y + tileMap.GetUsedRect().Position.Y;

                Vector2I index = new Vector2I(globalX, globalY);

                if(TileIsInUse(x,y, out TileData sharedTileData) == false){
                    continue;
                }

                AStarTileType navigation = (AStarTileType)(int)sharedTileData.GetCustomData("NavigationType");

                ref CellData cell = ref cells[x,y];
                
                switch(navigation){
                    case AStarTileType.Block:
                        cell.SetBlocked(true);
                        cell.LockData(); // <-- lock for statics.
                        break;
                    case AStarTileType.Open:
                        cell.SetBlocked(false);
                        break;
                }
            }
        }
    }

    public override void _Draw(){
        base._Draw();
        if(drawDebug == false){
            return;
        }
        for(int x = 0; x < tileMap.GetUsedRect().Size.X; x++){
            for(int y = 0; y < tileMap.GetUsedRect().Size.Y; y++){                    

                Vector2I index = new Vector2I(
                    x + tileMap.GetUsedRect().Position.X,
                    y + tileMap.GetUsedRect().Position.Y
                );

                if(TileIsInUse(x, y, out TileData sharedTileData)==false){
                    continue;
                }

                ref CellData cell = ref cells[x,y];

                if(cell.Blocked==true){
                    DrawRect(new Rect2(tileMap.MapToLocal(index), debugSquareSize), debugBlockedColour);
                }
                else{
                    DrawRect(new Rect2(tileMap.MapToLocal(index), debugSquareSize), debugTraversableColour);
                }
            }
        }            
    }

    public Vector2I GlobalToIdPosition(Vector2 globalPosition){
        return tileMap.LocalToMap(globalPosition);
    }

    public Vector2 IdToGlobalPosition(Vector2I gridIdPosition){
        return tileMap.MapToLocal(gridIdPosition);
    }

    private bool TileIsInUse(int x, int y, out TileData sharedTileData){
        Vector2I index = new(x,y);
        sharedTileData = tileMap.GetCellTileData(index);
        return sharedTileData!=null;
    }
}
