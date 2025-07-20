using Godot;
using System;

namespace Entropek.Ai;

public partial class NavigationGrid2D : Node2D{

    CellData[,] cells;

    [Export] private TileMapLayer tileMap;
    [Export] private Font debugFont;
    private Color debugBlockedColour        = new Color(1,0,0,0.5f);
    private Color debugTraversableColour    = new Color(0.5f,0.5f,1,0.5f);
    private Vector2 debugSquareSize         = new Vector2(4,4);

    private bool drawDebug = true;
    private bool hideTiles = true;

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
        InitialiseGridClearance();
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

    public override void _Process(double delta){
        base._Process(delta);
    }

    public void InitialiseGridClearance(){
        int rows = cells.GetLength(0);
        int cols = cells.GetLength(1);

        for(int x = 0; x < rows; x++){
            for(int y = 0; y < cols; y++){
                CalculateClearance(x,y);
            }
        }
    }

    public void CalculateClearance(int cellX, int cellY){
        ref CellData cell = ref cells[cellX, cellY];
        
        if (cell.Blocked){
            cell.SetClearance(0);
            return;
        }

        byte clearance = 1;
        bool foundBlocked = false;

        while (clearance < 255){

            // Check bounds before checking the square
            
            if (cellX + clearance >= cells.GetLength(0) || cellY + clearance >= cells.GetLength(1))
                break;

            // Check new border row and column of the square
            
            for (int i = 0; i <= clearance; i++){

                ref CellData bottomRowNeighbour = ref cells[cellX + i, cellY + clearance];
                ref CellData rightColumnNeighbour = ref cells[cellX + clearance, cellY + i]; 

                if (bottomRowNeighbour.Blocked ||
                    rightColumnNeighbour.Blocked){
                    foundBlocked = true;
                    break;
                }
            }

            if (foundBlocked)
                break;

            clearance++;
        }

        cell.SetClearance(clearance);
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
                    Vector2 globalPosition = tileMap.MapToLocal(index);
                    DrawRect(new Rect2(globalPosition-debugSquareSize*0.5f, debugSquareSize), debugBlockedColour);
                }
                else{
                    Vector2 globalPosition = tileMap.MapToLocal(index);
                    DrawRect(new Rect2(globalPosition, debugSquareSize*0.5f), debugTraversableColour);
                    DrawString(debugFont, globalPosition, cell.Clearance.ToString(), HorizontalAlignment.Left, -1, 4);
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
