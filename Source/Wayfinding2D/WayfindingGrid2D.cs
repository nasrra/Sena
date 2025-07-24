using Godot;
using System;
using System.Collections.Generic;

namespace Entropek.Ai;

public partial class WayfindingGrid2D : Node2D{

    public static WayfindingGrid2D Instance {get;private set;}

    CellData[,] cells;
    PathCell[,] paths;

    [Export] private TileMapLayer tileMap;
    [Export] private Font debugFont;
    private Color debugBlockedColour        = new Color(1,0,0,0.5f);
    private Color debugTraversableColour    = new Color(0.5f,0.5f,1,0.5f);
    private Vector2 debugSquareSize         = new Vector2(4,4);
    private Vector2I gridSize = Vector2I.Zero;
    public Vector2I GridSize {get => gridSize;}
    private Vector2I cellSize = Vector2I.Zero;
    public Vector2I CellSize {get => cellSize;}

    private bool drawDebug = true;
    private bool hideTiles = true;

    // orthogonal (Manhattan)

    // private Vector2I[] directions = new Vector2I[]{
    //     new Vector2I(1, 0),     // left 
    //     new Vector2I(-1, 0),    // right
    //     new Vector2I(0, 1),     // up
    //     new Vector2I(0, -1)     // down
    // };

    // diagonal (Ocitile)

    private Vector2I[] directions = new Vector2I[]{
        new Vector2I(1, 0),     // left 
        new Vector2I(1, 1),     // top left 
        new Vector2I(1, -1),    // bot left

        new Vector2I(-1, 0),    // right
        new Vector2I(-1, 1),    // top right
        new Vector2I(-1, -1),   // bot right

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

        gridSize.X = tileMap.GetUsedRect().Size.X;
        gridSize.Y = tileMap.GetUsedRect().Size.Y;
        cellSize.X = tileMap.TileSet.TileSize.X;
        cellSize.Y = tileMap.TileSet.TileSize.Y;

        cells = new CellData[gridSize.X, gridSize.Y];
        paths = new PathCell[gridSize.X, gridSize.Y];

        // offset to the top left of the grid.
        // so that we scan from to left to right on each row for each column.
        // starting from the top left of the grid and ending at the bottom right.

        for(int x = 0; x < gridSize.X; x++){
            int globalX = x + tileMap.GetUsedRect().Position.X;
            for(int y = 0; y < gridSize.Y; y++){
                int globalY = y + tileMap.GetUsedRect().Position.Y;

                Vector2I index = new Vector2I(globalX, globalY);

                if(TileIsInUse(x,y, out TileData sharedTileData) == false){
                    continue;
                }

                NavigationType navigation = (NavigationType)(int)sharedTileData.GetCustomData("NavigationType");

                ref CellData cell = ref cells[x,y];
                
                switch(navigation){
                    case NavigationType.Block:
                        cell.SetBlocked(true);
                        cell.LockData(); // <-- lock for statics.
                        break;
                    case NavigationType.Open:
                        cell.SetBlocked(false);
                        break;
                }
            }
        }
    }

    public override void _Process(double delta){
        base._Process(delta);
        QueueRedraw();
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

    // Calculates clearance from top left to bottom right.
    // for bottom right alignment.

    // public void CalculateClearance(int cellX, int cellY){
    //     ref CellData cell = ref cells[cellX, cellY];
        
    //     if (cell.Blocked){
    //         cell.SetClearance(0);
    //         return;
    //     }

    //     byte clearance = 1;
    //     bool foundBlocked = false;

    //     while (clearance < 255){

    //         // Check bounds before checking the square
            
    //         if (cellX + clearance >= cells.GetLength(0) || cellY + clearance >= cells.GetLength(1))
    //             break;

    //         // Check new border row and column of the square
            
    //         for (int i = 0; i <= clearance; i++){

    //             ref CellData bottomRowNeighbour = ref cells[cellX + i, cellY + clearance];
    //             ref CellData rightColumnNeighbour = ref cells[cellX + clearance, cellY + i]; 

    //             if (bottomRowNeighbour.Blocked ||
    //                 rightColumnNeighbour.Blocked){
    //                 foundBlocked = true;
    //                 break;
    //             }
    //         }

    //         if (foundBlocked)
    //             break;

    //         clearance++;
    //     }

    //     cell.SetClearance(clearance);
    // }

    public void CalculateClearance(int cx, int cy) {
        ref CellData cell = ref cells[cx, cy];

        if(cell.Blocked==true){
            cell.SetClearance(0);
            return;
        }

        byte clearance = 1;
        bool foundBlocked = false;

        while(true){
            // define the square bounds to check.
            int left = cx - clearance;
            int right = cx + clearance;
            int top = cy - clearance;
            int bot = cy + clearance;

            // stop if we are out of the grid.
            if(left < 0 
            || top < 0
            || right >= gridSize.X 
            || bot >= gridSize.Y){
                break;
            }
            
            // analyse border of square at current clearance radius.
            // to find a cell that is blocked.
            for(int i = -clearance; i <= clearance; i++){
                
                // top and bottom rows.
            
                if(cells[cx + i, top].Blocked == true
                || cells[cx +i, bot].Blocked == true){
                    foundBlocked = true;
                    break;
                }

                // left and right columns 
                // <-- (< not <=) skip corners as they are already checked.

                if(cells[left, cy + i].Blocked == true
                || cells[right, cy + i].Blocked == true){
                    foundBlocked = true;
                    break;
                }

            }

            if(foundBlocked == true){
                break;
            }

            clearance++;
        }

        cell.SetClearance(clearance);
    }

    /// <summary>
    /// Gets a path along the grid towards a given end point.
    /// </summary>
    /// <param name="startGlobalPosition"></param>
    /// <param name="endGlobalPosition"></param>
    /// <param name="agentSize"></param>
    /// <param name="tolerance">tolerance is the amount of leeway given for the end point check.</param>
    /// <returns></returns>

    public Stack<Vector2> GetPath(Vector2 startGlobalPosition, Vector2 endGlobalPosition, byte agentSize, byte tolerance = 0){
        return GetPath(
            GlobalToIdPosition(startGlobalPosition), 
            GlobalToIdPosition(endGlobalPosition), 
            agentSize,
            tolerance
        );
    }  

    /// <summary>
    /// Gets a path along the grid towards a given end point.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="agentSize"></param>
    /// <param name="tolerance">the amount of leeway given for the end point check.</param>
    /// <returns></returns>

    private Stack<Vector2> GetPath(Vector2I start, Vector2I end, byte agentSize, byte tolerance = 0){

        // if either point is out of bounds.

        if(start.X >= gridSize.X || start.Y >= gridSize.Y || start.X < 0 || start.Y < 0
        || end.X >= gridSize.X || end.Y >= gridSize.Y || end.X < 0 || end.Y < 0){
            return new();
        }

        // if either point is within a blocked cell.

        ref CellData startCell = ref cells[start.X, start.Y];
        ref CellData endCell = ref cells[end.X, end.Y];
        if(startCell.Blocked == true || endCell.Blocked == true){
            return new();
        }

        List<Vector2I> openList = new List<Vector2I>();
        HashSet<Vector2I> closedSet = new HashSet<Vector2I>();
        
        ref PathCell endPathCell = ref paths[end.X, end.Y];
        ref PathCell startPathCell = ref paths[start.X, start.Y];
        startPathCell = new PathCell(
            id: start,
            cost: 0, 
            heuristic: CalculateHeuristic(start, end)
        );

        openList.Add(start);
        closedSet.Add(start);

        Vector2 toleranceLowerBound = Vector2.Zero;
        Vector2 toleranceUpperBound = Vector2.Zero;

        if(tolerance > 0){
            toleranceLowerBound.X = end.X - tolerance;
            toleranceUpperBound.X = end.X + tolerance;
            toleranceLowerBound.Y = end.Y - tolerance;
            toleranceUpperBound.Y = end.Y + tolerance;
        }


        while(openList.Count > 0){
            PathCell current = GetLowestTotalCostPathCell(openList);

            if(tolerance > 0){
                if(WithinTolerance(toleranceLowerBound, toleranceUpperBound, ref current) == true
                && CalcToleranceBetweenPathCells(current.Id, end) == true){
                    return ReconstructPath(current);
                }
            }
            else if(current.Id == end){
                return ReconstructPath(current);
            }

            openList.Remove(current.Id);
            closedSet.Add(current.Id);

            foreach(Vector2I direction in directions){
                Vector2I neighbourId = current.Id + direction;

                // check bounds.
                
                if(closedSet.Contains(neighbourId) || neighbourId.X >= gridSize.X  || neighbourId.Y >= gridSize.Y){
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

    public Vector2I GlobalToIdPosition(Vector2 globalPosition){
        return tileMap.LocalToMap(globalPosition);
    }

    public Vector2 IdToGlobalPosition(Vector2I gridIdPosition){
        return tileMap.MapToLocal(gridIdPosition);
    }

    /// <summary>
    /// Check if a path cell is within a given tolerance radius in the grid.
    /// </summary>
    /// <param name="lowerBound"></param>
    /// <param name="upperBound"></param>
    /// <param name="currentPathCell"></param>
    /// <returns></returns>

    private bool WithinTolerance(Vector2 lowerBound, Vector2 upperBound, ref PathCell currentPathCell){
        return
        currentPathCell.Id.X >= lowerBound.X && currentPathCell.Id.X <= upperBound.X
        && currentPathCell.Id.Y >= lowerBound.Y && currentPathCell.Id.Y <= upperBound.Y;
    }

    /// <summary>
    /// line-of-sight style check that determines if there's and unobstructed straight line between two cells.
    /// where every cell in that line must have at least agentSize clearance.
    /// Note: modified Bresenham's line algorithm.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>

    private bool CalcToleranceBetweenPathCells(Vector2I from, Vector2I to){
        int x0 = from.X;
        int y0 = from.Y;
        int x1 = to.X;
        int y1 = to.Y;

        // distance between cells.

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        // direction to step in.

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        // error term used by Bresenham's algorithm to determine when to step Y in addition to X.
        // (or vice versa), keeping the line straight even when slope is not 1.

        int err = dx - dy;

        while (true){
            // Bounds check
            if (x0 < 0 || x0 >= gridSize.X || y0 < 0 || y0 >= gridSize.Y){
                return false;
            }


            // Blocked check
            ref CellData cellData = ref cells[x0, y0];
            if (cellData.Blocked){
                // GD.Print($"end! {to.X} {to.Y}");
                // GD.Print($"blocked! {x0} {y0}");
                return false;
            }

            // Reached destination
            if (x0 == x1 && y0 == y1){
                break;
            }

            // step.
            int e2 = 2 * err;
            if (e2 > -dy){
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx){
                err += dx;
                y0 += sy;
            }
        }

        return true;
    }

    /// <summary>
    /// Constructs a path from a given path cell after a path has been found using GetPath().
    /// </summary>
    /// <param name="endPathCell"></param>
    /// <returns></returns>

    private Stack<Vector2> ReconstructPath(PathCell endPathCell){
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
        // Manhattan distance (orthogonal).
        // return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    
        // ocitile distance (diagonal)
        int dx = Math.Abs(a.X - b.X);
        int dy = Math.Abs(a.Y - b.Y);

        int D = 10;     // Cost for straight (orthogonal) movement
        int D2 = 14;    // Approx. sqrt(2) * 10 for diagonal movement

        return D * (dx + dy) + (D2 - 2 * D) * Math.Min(dx, dy);
    }

    public override void _Draw(){
        base._Draw();
        if(drawDebug == false){
            return;
        }
        for(int x = 0; x < tileMap.GetUsedRect().Size.X; x++){
            for(int y = 0; y < tileMap.GetUsedRect().Size.Y; y++){                    

                Vector2I index = new Vector2I(
                    x,
                    y
                );

                if(TileIsInUse(x, y, out TileData sharedTileData)==false){
                    continue;
                }

                ref CellData cell = ref cells[x,y];

                if(cell.Blocked==true){
                    Vector2 globalPosition = tileMap.MapToLocal(index);
                    DrawRect(new Rect2(globalPosition-debugSquareSize, debugSquareSize), debugBlockedColour);
                }
                else{
                    Vector2 globalPosition = tileMap.MapToLocal(index);
                    DrawRect(new Rect2(globalPosition-debugSquareSize, debugSquareSize), debugTraversableColour);
                    DrawString(debugFont, globalPosition, cell.Clearance.ToString(), HorizontalAlignment.Left, -1, 4);
                }
            }
        }            
    }

    public void Insert(Rect2 globalAABB,  NavigationType navigationType, out List<Vector2I> currentFrameIndices){
        currentFrameIndices = new List<Vector2I>();

        Vector2I minGridPosition = tileMap.LocalToMap(globalAABB.Position - (globalAABB.Size *0.5f));
        Vector2I maxGridPosition = tileMap.LocalToMap(globalAABB.Position + (globalAABB.Size *0.5f));

        for (int y = minGridPosition.Y; y <= maxGridPosition.Y; y++) {
            for (int x = minGridPosition.X; x <= maxGridPosition.X; x++) {

                Vector2I index = new Vector2I(x,y);
                
                currentFrameIndices.Add(index);

                if(TileIsInUse(index.X, index.Y, out TileData sharedTileData)==false){
                    continue;
                }
                
                ref CellData cellData = ref cells[index.X, index.Y];

                if(cellData.Locked==true){
                    continue;
                }

                switch(navigationType){
                    case NavigationType.Block:
                        cellData.SetBlocked(true);
                        break;
                    case NavigationType.Pass:
                        throw new Exception($"{NavigationType.Pass} not implmeneted!");
                    case NavigationType.Open:
                        cellData.SetBlocked(false);
                        break;
                }
            }
        }

        InitialiseGridClearance();
    }

    public void Remove(List<Vector2I> indices, NavigationType navigationType){
        for(int i = 0 ; i < indices.Count; i++){
            
            Vector2I index = indices[i];

            if(TileIsInUse(index.X, index.Y, out TileData sharedTileData)==false){
                continue;
            }
            
            ref CellData cellData = ref cells[index.X, index.Y];

            if(cellData.Locked==true){
                continue;
            }

            switch(navigationType){
                case NavigationType.Block:
                    cellData.SetBlocked(false);
                    break;
                case NavigationType.Pass:
                    throw new Exception($"{NavigationType.Pass} not implmeneted!");
                case NavigationType.Open:
                    cellData.SetBlocked(false);
                    break;
            }
        }

        InitialiseGridClearance();
    }

    private bool TileIsInUse(int x, int y, out TileData sharedTileData){
        Vector2I index = new(x,y);
        sharedTileData = tileMap.GetCellTileData(index);
        return sharedTileData!=null;
    }
}
