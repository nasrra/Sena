using Godot;
using System;
using System.Collections.Generic;

public partial class PathfindingGrid : Node2D{
    
    public static PathfindingGrid Instance {get;private set;}

    [Export] TileMapLayer tileMap;
    private Dictionary<Vector2I, AStarTileData> aStarTileData = new Dictionary<Vector2I, AStarTileData>();

    private AStarGrid2D grid;

    private Color debugBlockedColour        = new Color(1,0,0);
    private Color debugTraversableColour    = new Color(1,1,1);
    private Color debugPassableColour       = new Color(0.5f,0.5f,0);
    private Vector2 debugSquareSize         = new Vector2(4,4);
    private bool drawDebug = true;
    private bool hideTiles = true;

    HashSet<Vector2I> tilesToUpdate = new HashSet<Vector2I>();

    public override void _Ready(){
        Instance = this;
        if(hideTiles == true){
            Shader shader = new Shader();
            shader.Code = "shader_type canvas_item; void fragment() { discard; }";
            ShaderMaterial material = new ShaderMaterial();
            material.Shader = shader;
            tileMap.Material = material;
        }
        InitialiseGrid();
        InitialiseTileDataFromTileMap();
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        UpdateModifiedTiles();
    }

    public override void _Process(double delta){
        base._Process(delta);
        if(Input.IsActionJustPressed("DebugPathfinding")){
            tileMap.Visible = !tileMap.Visible;
        }
    }

    public Vector2I GlobalToIdPosition(Vector2 globalPosition){
        return tileMap.LocalToMap(globalPosition);
    }

    public Vector2 IdToGlobalPosition(Vector2I gridIdPosition){
        return tileMap.MapToLocal(gridIdPosition);
    }

    public Queue<Vector2> GetPathToPoint(Vector2 startGlobalPosition, Vector2 endGlobalPosition){
        // GD.Print($"start: {GlobalToIdPosition(startGlobalPosition)}");
        // GD.Print($"end: {GlobalToIdPosition(endGlobalPosition)}");

        Godot.Collections.Array<Vector2I> idPath = grid.GetIdPath(
            GlobalToIdPosition(startGlobalPosition),
            GlobalToIdPosition(endGlobalPosition)
        ).Slice(1); // <-- remove the start position.

        Queue<Vector2> globalPath = new Queue<Vector2>();
        for(int i = 0; i < idPath.Count; i++){
            globalPath.Enqueue(IdToGlobalPosition(idPath[i]));  
        }

        return globalPath;
    }

    private void InitialiseGrid(){
        grid = new AStarGrid2D();
        grid.Region = tileMap.GetUsedRect();
        grid.CellSize = tileMap.TileSet.TileSize;
        grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles;
        grid.Update();

    }

    private void InitialiseTileDataFromTileMap(){ // <-- inits static environment.

        int tileMapSizeX = tileMap.GetUsedRect().Size.X;
        int tileMapSizeY = tileMap.GetUsedRect().Size.Y;

        // offset to the top left of the grid.
        // so that we scan from to left to right on each row for each column.
        // starting from the top left of the grid and ending at the bottom right.

        for(int x = 0; x < tileMapSizeX; x++){
            int globalX = x + tileMap.GetUsedRect().Position.X;
            for(int y = 0; y < tileMapSizeY; y++){
                int globalY = y + tileMap.GetUsedRect().Position.Y;

                Vector2I index = new Vector2I(globalX, globalY);

                if(TileIsInUse(index, out TileData sharedTileData) == false){
                    continue;
                }

                AStarTileType navigation = (AStarTileType)(int)sharedTileData.GetCustomData("NavigationType");

                aStarTileData.Add(index, new AStarTileData());

                switch(navigation){
                    case AStarTileType.Block:
                        grid.SetPointSolid(index, true);
                        aStarTileData[index].LockData();
                        break;
                    case AStarTileType.Open:
                        grid.SetPointSolid(index, false);
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

                if(TileIsInUse(index, out TileData sharedTileData)==false){
                    continue;
                }

                AStarTileType navigation = (AStarTileType)(int)sharedTileData.GetCustomData("NavigationType");


                switch(navigation){
                    case AStarTileType.Block:
                        DrawRect(new Rect2(tileMap.MapToLocal(index), debugSquareSize), debugBlockedColour);
                        break;
                    case AStarTileType.Open:
                        DrawRect(new Rect2(tileMap.MapToLocal(index), debugSquareSize), debugTraversableColour);
                        break;
                    case AStarTileType.Pass:
                        DrawRect(new Rect2(tileMap.MapToLocal(index), debugSquareSize), debugPassableColour);
                    break;
                }
            }
        }            
    }

    public void UpdateModifiedTiles(){
        if(tilesToUpdate == null){
            return;
        }
        foreach(Vector2I index in tilesToUpdate){

            if(TileIsInUse(index, out TileData sharedTileData)==false){
                continue;
            }
            
            AStarTileData tileData = aStarTileData[index];
            AStarTileType tileType = tileData.EvaluateType();
            switch(tileType){
                case AStarTileType.Block:
                    tileMap.SetCell(index, 0,new Vector2I(1,0),0);
                    grid.SetPointSolid(index, true);
                    break;
                case AStarTileType.Open:
                    tileMap.SetCell(index, 0,new Vector2I(0,0),0);
                    grid.SetPointSolid(index, false);
                    break;
                case AStarTileType.Pass:
                    throw new NotImplementedException();
            }
        }
        grid.Update();
        tilesToUpdate.Clear();
        QueueRedraw();
    }

    public void Insert(Rect2 globalAABB,  AStarTileType agentType, out List<Vector2I> currentFrameIndices){
        currentFrameIndices = new List<Vector2I>();

        Vector2I minGridPosition = tileMap.LocalToMap(globalAABB.Position - (globalAABB.Size *0.5f));
        Vector2I maxGridPosition = tileMap.LocalToMap(globalAABB.Position + (globalAABB.Size *0.5f));

        for (int y = minGridPosition.Y; y <= maxGridPosition.Y; y++) {
            for (int x = minGridPosition.X; x <= maxGridPosition.X; x++) {

                Vector2I index = new Vector2I(x,y);
                
                currentFrameIndices.Add(index);

                if(TileIsInUse(index, out TileData sharedTileData)==false){
                    continue;
                }
                
                AStarTileData tileData = aStarTileData[index];

                if(tileData.Locked==true){
                    continue;
                }

                switch(agentType){
                    case AStarTileType.Block:
                        tileData.AddBlockInhabitant();
                        tilesToUpdate.Add(index);
                        break;
                    case AStarTileType.Pass:
                        tileData.AddPassInhabitant();
                        tilesToUpdate.Add(index);
                        break;
                }
            }
        }
    }

    public void Remove(List<Vector2I> indices, AStarTileType agentType){
        for(int i = 0 ; i < indices.Count; i++){
            
            Vector2I index = indices[i];

            if(TileIsInUse(index, out TileData sharedTileData)==false){
                continue;
            }
            
            AStarTileData tileData = aStarTileData[index];

            if(tileData.Locked==true){
                continue;
            }

            switch(agentType){
                case AStarTileType.Block:
                    tileData.RemoveBlockInhabitant();
                    tilesToUpdate.Add(index);
                    break;
                case AStarTileType.Pass:
                    tileData.RemoveBlockInhabitant();
                    tilesToUpdate.Add(index);
                    break;
            }
        }
    }

    private bool TileIsInUse(Vector2I index, out TileData sharedTileData){
        sharedTileData = tileMap.GetCellTileData(index);
        return sharedTileData!=null;
    }
}