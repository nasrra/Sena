using Godot;
using System;
using System.Collections.Generic;

namespace Entropek.Ai;

public partial class NavigationGrid2D : Node2D{

    public static NavigationGrid2D Instance {get;private set;}

    CellData[,] cells;
    PathCell[,] paths;

    [Export] private TileMapLayer tileMap;
    [Export] private Font debugFont;
    private Color debugBlockedColour        = new Color(1,0,0,0.5f);
    private Color debugTraversableColour    = new Color(0.5f,0.5f,1,0.5f);
    private Vector2 debugSquareSize         = new Vector2(4,4);
    
    public int SizeX {get;private set;}
    public int SizeY {get;private set;}

    private bool drawDebug = true;
    private bool hideTiles = true;

    private Vector2I[] directions = new Vector2I[]{
        new Vector2I(1, 0),     // left 
        new Vector2I(-1, 0),    // right
        new Vector2I(0, 1),     // up
        new Vector2I(0, -1)     // down
    };

    public override void _Ready(){
        base._Ready();
        if(hideTiles == true){
            Shader shader = new Shader();
            shader.Code = "shader_type canvas_item; void fragment() { discard; }";
            ShaderMaterial material = new ShaderMaterial();
            material.Shader = shader;
            tileMap.Material = material;
        }
        Initialise();
        InitialiseGridClearance();
        Instance=this;
    }

    private void Initialise(){ // <-- inits static environment.

        SizeX = tileMap.GetUsedRect().Size.X;
        SizeY = tileMap.GetUsedRect().Size.Y;

        cells = new CellData[SizeX, SizeY];
        paths = new PathCell[SizeX, SizeY];

        // offset to the top left of the grid.
        // so that we scan from to left to right on each row for each column.
        // starting from the top left of the grid and ending at the bottom right.

        for(int x = 0; x < SizeX; x++){
            int globalX = x + tileMap.GetUsedRect().Position.X;
            for(int y = 0; y < SizeY; y++){
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

    public Stack<Vector2> GetPath(Vector2 startGlobalPosition, Vector2 endGlobalPosition, byte agentSize){
        return GetPath(
            GlobalToIdPosition(startGlobalPosition), 
            GlobalToIdPosition(endGlobalPosition), 
            agentSize
        );
    }  

    private Stack<Vector2> GetPath(Vector2I start, Vector2I end, byte agentSize){
        List<Vector2I> openList = new List<Vector2I>();
        HashSet<Vector2I> closedSet = new HashSet<Vector2I>();
        
        ref PathCell startPathCell = ref paths[start.X, start.Y];
        startPathCell = new PathCell(
            id: start,
            cost: 0, 
            heuristic: CalculateHeuristic(start, end)
        );

        openList.Add(start);
        closedSet.Add(start);

        while(openList.Count > 0){
            PathCell current = GetLowestTotalCostPathCell(openList);

            if(current.Id == end){
                return ReconstructPath(current, agentSize);
            }

            openList.Remove(current.Id);
            closedSet.Add(current.Id);

            foreach(Vector2I direction in directions){
                Vector2I neighbourId = current.Id + direction;

                // check bounds.
                
                if(closedSet.Contains(neighbourId) || neighbourId.X >= SizeX  || neighbourId.Y >= SizeY){
                    continue;
                }

                ref CellData neighbourCellData = ref cells[neighbourId.X,neighbourId.Y];

                // // Check if terrain is in agent's capability
                // if (!capability.Contains(neighborCell.TerrainType))
                //     continue;

                // check clearance.

                if(neighbourCellData.Clearance < agentSize){
                    continue;
                }

                // ref PathCell neighbourPathCell = ref paths[neighbourId.X, neighbourId.Y]; 

                PathCell neighbourPathCell  = new PathCell(
                    id: neighbourId,
                    parentId: current.Id,
                    cost: current.Cost + 1,
                    heuristic: CalculateHeuristic(neighbourId, end)
                );

                // if already in open list with lower total cost.

                ref PathCell existing = ref paths[neighbourId.X, neighbourId.Y];
                
                if(openList.Contains(neighbourId) && neighbourPathCell.Total >= neighbourPathCell.Total){    
                    continue;
                }

                // assign the neighbour data.

                existing = neighbourPathCell;

                openList.Add(neighbourId);
            }
        }
        return new();
    }

    private Stack<Vector2> ReconstructPath(PathCell endPathCell, byte agentSize){
        Stack<Vector2> path = new Stack<Vector2>();
        PathCell current = endPathCell;
        while(true){
            // current.Id + (Vector2I.One * (agentSize-1));
            path.Push(IdToGlobalPosition(current.Id));    
            if(current.ParentId == new Vector2I(-1,-1)){
                break;
            }
            else{
                current = paths[current.ParentId.X, current.ParentId.Y];
            }
        }
        return path;
    }

    private PathCell GetLowestTotalCostPathCell(List<Vector2I> indexes){
        Vector2I index = indexes[0];
        PathCell best = paths[index.X, index.Y];
    
        for(int i = 1; i < indexes.Count; i++){
            index = indexes[i];
            PathCell other = paths[index.X, index.Y];
            if(other.Total < best.Total){
                best = other;
            }
        }

        return best;
    }

    private int CalculateHeuristic(Vector2I a, Vector2I b){
        // Manhattan distance.
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
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
