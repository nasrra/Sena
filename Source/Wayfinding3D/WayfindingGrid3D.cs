// using Godot;
// using System;
// using System.Collections.Generic;
// using System.Data;

// namespace Entropek.Ai;

// public partial class WayfindingGrid3D : Node3D{

// 	public const byte MaxAgentSize = 16;

// 	public static WayfindingGrid3D Singleton {get;private set;}

// 	PathCell[,] paths;

// 	private bool[,] locked;
// 	private byte[,] groundClearance;
// 	private byte[,] aerialClearance;
// 	private NavigationType[,] navigationType;
	

// 	[Export] private GridMap gridMap;
// 	[Export] private Font debugFont;
// 	private Color debugBlockedColour        = new Color(1,0,0,1f);
// 	private Color debugPassThroughColour    = new Color(0.5f,0.5f,0,1f);
// 	private Color debugOpenColour           = new Color(1f,1f,1f,1f);
// 	private Vector2 debugSquareSize         = new Vector2(4,4);
// 	private Vector2I gridSize = Vector2I.Zero;
// 	public Vector2I GridSize {get => gridSize;}
// 	private Vector2I cellSize = Vector2I.Zero;
// 	public Vector2I CellSize {get => cellSize;}

// 	private bool drawDebug = true;
// 	private bool hideTiles = true;

// 	// orthogonal (Manhattan)

// 	// private Vector2I[] directions = new Vector2I[]{
// 	//     new Vector2I(1, 0),     // left 
// 	//     new Vector2I(-1, 0),    // right
// 	//     new Vector2I(0, 1),     // up
// 	//     new Vector2I(0, -1)     // down
// 	// };

// 	// diagonal (Ocitile)

// 	private Vector2I[] directions = new Vector2I[]{
// 		new Vector2I(1, 0),     // left 
// 		new Vector2I(1, 1),     // top left 
// 		new Vector2I(1, -1),    // bot left

// 		new Vector2I(-1, 0),    // right
// 		new Vector2I(-1, 1),    // top right
// 		new Vector2I(-1, -1),   // bot right

// 		new Vector2I(0, 1),     // up
		
// 		new Vector2I(0, -1)     // down
// 	};


// 	public override void _Ready(){
// 		base._Ready();
// 		gridMap.Visible=true;
// 		if(hideTiles == true){
// 			Shader shader = new Shader();
// 			shader.Code = "shader_type canvas_item; void fragment() { discard; }";
// 			ShaderMaterial material = new ShaderMaterial();
// 			material.Shader = shader;
// 			// gridMap.Material = material;
// 		}
// 		Singleton=this;
// 		Initialise();
// 	}

// 	private void Initialise(){ // <-- inits static environment.
// 		Vector3 gridSize = gridMap.GetUsedCellsAabb().Size;
// 		gridSize.X = gridMap.GetUsedRect().Size.X;
// 		gridSize.Y = gridMap.GetUsedRect().Size.Y;
// 		cellSize.X = gridMap.TileSet.TileSize.X;
// 		cellSize.Y = gridMap.TileSet.TileSize.Y;
	
// 		aerialClearance = new byte[gridSize.X, gridSize.Y];
// 		groundClearance = new byte[gridSize.X, gridSize.Y];
// 		locked          = new bool[gridSize.X, gridSize.Y];
// 		navigationType  = new NavigationType[gridSize.X, gridSize.Y];
		
// 		paths = new PathCell[gridSize.X, gridSize.Y];
	
	
// 		SetStaticNavigationsFromTilemap();
	
// 		InitialiseGridClearance(NavigationType.Open);
// 		InitialiseGridClearance(NavigationType.PassThrough);
// 	}

// 	private void SetStaticNavigationsFromTilemap(){
// 		for(int x = 0; x < gridSize.X; x++){
// 			for(int y = 0; y < gridSize.Y; y++){
				
// 				if(TileIsInUse(x,y, out TileData sharedTileData) == false){
// 					continue;
				
// 				}

// 				NavigationType navigation = (NavigationType)(int)sharedTileData.GetCustomData("NavigationType");
				
// 				switch(navigation){
// 					case NavigationType.Blocked:
// 						navigationType[x,y] = NavigationType.Blocked;
// 						locked[x,y] = true;
// 						break;
// 					case NavigationType.PassThrough:
// 						navigationType[x,y] = NavigationType.PassThrough;
// 						locked[x,y] = true;
// 						break;
// 					case NavigationType.Open:
// 						navigationType[x,y] = NavigationType.Open;
// 						locked[x,y] = false;
// 						break;
// 				}
// 			}
// 		}
// 	}

// 	public void InitialiseGridClearance(NavigationType navigationType){
// 		byte[,] clearance = navigationType == NavigationType.PassThrough? aerialClearance : groundClearance;
// 		NavigationType blocked = navigationType == NavigationType.PassThrough? NavigationType.Blocked | NavigationType.PassThrough : NavigationType.Blocked;
		
// 		for(int x = 0; x < gridSize.X-1; x++){
// 			for(int y = 0; y < gridSize.Y-1; y++){
// 				clearance[x,y] = CalculateClearance(x,y, blocked);
// 			}
// 		}
// 	}

// 	protected void UpdateClearance(NavigationType agentType, int cx, int cy){
// 		byte[,] clearance = agentType == NavigationType.PassThrough ? aerialClearance : groundClearance;
// 		NavigationType blockTypes = agentType == NavigationType.PassThrough
// 			? NavigationType.Blocked | NavigationType.PassThrough
// 			: NavigationType.Blocked;

// 		// Calculate the maximum possible clearance from the center point (cx, cy)
// 		int maxClearance = Math.Min(
// 			Math.Min(cx, gridSize.X - 1 - cx),
// 			Math.Min(cy, gridSize.Y - 1 - cy)
// 		);

// 		// GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");

// 		// Loop through potential clearances
// 		for (int i = 0; i <= maxClearance; i++){
// 			int left = cx - i;
// 			int right = cx + i;
// 			int top = cy - i;
// 			int bottom = cy + i;
			
// 			bool noChange = true;

// 			// Check the four borders (left, right, top, bottom) at once
// 			for (int j = -i; j <= i; j++)
// 			{
// 				Vector2I topCell = new(cx + j, top);
// 				byte newTopClearance = CalculateClearance(topCell.X, topCell.Y, blockTypes);
// 				if(clearance[topCell.X, topCell.Y] != newTopClearance){
// 					clearance[topCell.X, topCell.Y] = newTopClearance;
// 					noChange = false;
// 					// debugDraw.Call("rect",tileMap.MapToLocal(topCell), Vector2.One*8, new Color(0,1f,0f), 8f, 6f);
// 				}
// 				else{
// 					// debugDraw.Call("rect",tileMap.MapToLocal(topCell), Vector2.One*8, new Color(1,0f,0f), 8f, 6f);
// 				}

// 				Vector2I botCell = new(cx + j, bottom);
// 				byte newBotClearance = CalculateClearance(botCell.X, botCell.Y, blockTypes);
// 				if(clearance[botCell.X, botCell.Y] != newBotClearance){
// 					clearance[botCell.X, botCell.Y] = newBotClearance;
// 					noChange = false;
// 					// debugDraw.Call("rect",tileMap.MapToLocal(botCell), Vector2.One*8, new Color(0,1f,0f), 8f, 6f);
// 				}
// 				else{
// 					// debugDraw.Call("rect",tileMap.MapToLocal(botCell), Vector2.One*8, new Color(1,0f,0f), 8f, 6f);
// 				}

// 				Vector2I leftCell = new(left, cy + j);
// 				byte newLeftClearance = CalculateClearance(leftCell.X, leftCell.Y, blockTypes);
// 				if(clearance[leftCell.X, leftCell.Y] != newLeftClearance){
// 					clearance[leftCell.X,leftCell.Y] = newLeftClearance;
// 					noChange = false;
// 					// debugDraw.Call("rect",tileMap.MapToLocal(leftCell), Vector2.One*8, new Color(0,1f,0f), 8f, 6f);
// 				}
// 				else{
// 					// debugDraw.Call("rect",tileMap.MapToLocal(leftCell), Vector2.One*8, new Color(1,0f,0f), 8f, 6f);
// 				}

// 				Vector2I rightCell = new(right, cy + j);
// 				byte newRightClearance = CalculateClearance(rightCell.X, rightCell.Y, blockTypes);
// 				if(clearance[rightCell.X, rightCell.Y] != newRightClearance){
// 					clearance[rightCell.X, rightCell.Y] = newRightClearance;
// 					noChange = false;
// 					// debugDraw.Call("rect",tileMap.MapToLocal(rightCell), Vector2.One*8, new Color(0,1f,0f), 8f, 6f);
// 				}
// 				else{
// 					// debugDraw.Call("rect",tileMap.MapToLocal(rightCell), Vector2.One*8, new Color(1,0f,0f), 8f, 6f);
// 				}
// 			}

// 			// If blocked, break and set the clearance
// 			if (noChange==true){
// 				break;
// 			}
// 		}
// 	}

// 	public override void _Process(double delta){
// 		base._Process(delta);
// 	}

// 	double deltaAdd = 0;

// 	public override void _PhysicsProcess(double delta){
// 		base._PhysicsProcess(delta);
// 		QueueRedraw();
// 	}

//     public override void _Draw(){
//         base._Draw();
// 		if(drawDebug == false){
// 			return;
// 		}
// 		for(int x = 0; x < tileMap.GetUsedRect().Size.X; x++){
// 			for(int y = 0; y < tileMap.GetUsedRect().Size.Y; y++){                    

// 				Vector2I index = new Vector2I(
// 					x,
// 					y
// 				);

// 				if(TileIsInUse(x, y, out TileData sharedTileData)==false){
// 					continue;
// 				}

// 				Vector2 globalPosition = tileMap.MapToLocal(index);
// 				switch(navigationType[x,y]){
// 					case NavigationType.Blocked:
// 						DrawRect(new Rect2(globalPosition-debugSquareSize, debugSquareSize), debugBlockedColour);
// 					break;
// 					case NavigationType.PassThrough:
// 						// DrawRect(new Rect2(globalPosition-debugSquareSize, debugSquareSize), debugPassThroughColour);
// 						// DrawString(debugFont, globalPosition, aerialClearance[x,y].ToString(), HorizontalAlignment.Left, -1, 8);
// 					break;
// 					case NavigationType.Open:
// 						// DrawRect(new Rect2(globalPosition-debugSquareSize, debugSquareSize), debugOpenColour);
// 						// DrawString(debugFont, globalPosition, groundClearance[x,y].ToString(), HorizontalAlignment.Left, -1, 8);
// 					break;
// 				}
// 			}
// 		}            

//     }


// 	public byte CalculateClearance(int cx, int cy, NavigationType blockTypes){
// 		if (navigationType[cx, cy] == NavigationType.Blocked){
// 			return 0;
// 		}

// 		// Calculate the maximum possible clearance from the center point (cx, cy)
// 		int maxClearance = Math.Min(
// 			Math.Min(cx, gridSize.X - 1 - cx),
// 			Math.Min(cy, gridSize.Y - 1 - cy)
// 		);

// 		byte clearance = 1;

// 		// Loop through potential clearances
// 		for (; clearance <= maxClearance && clearance < MaxAgentSize; clearance++)
// 		{
// 			int left = cx - clearance;
// 			int right = cx + clearance;
// 			int top = cy - clearance;
// 			int bottom = cy + clearance;

// 			bool blocked = false;

// 			// Check the four borders (left, right, top, bottom) at once
// 			for (int i = -clearance; i <= clearance; i++)
// 			{
// 				if (
// 					(navigationType[cx + i, top] & blockTypes) != 0 ||
// 					(navigationType[cx + i, bottom] & blockTypes) != 0 ||
// 					(navigationType[left, cy + i] & blockTypes) != 0 ||
// 					(navigationType[right, cy + i] & blockTypes) != 0
// 				)
// 				{
// 					blocked = true;
// 					break;
// 				}
// 			}

// 			// If blocked, break and set the clearance
// 			if (blocked){
// 				break;
// 			}
// 		}

// 		// If we found no blockage, the full clearance is possible
// 		return clearance;
// 	}




// 	/// <summary>
// 	/// Gets a path along the grid towards a given end point.
// 	/// </summary>
// 	/// <param name="startGlobalPosition"></param>
// 	/// <param name="endGlobalPosition"></param>
// 	/// <param name="agentSize"></param>
// 	/// <param name="tolerance">tolerance is the amount of leeway given for the end point check.</param>
// 	/// <returns></returns>

// 	public Stack<Vector2> GetPath(Vector2 startGlobalPosition, Vector2 endGlobalPosition, NavigationType agentType, byte agentSize, byte tolerance = 0){
// 		switch(agentType){
// 			case NavigationType.Open:
// 				return GetPath(
// 					ref groundClearance,
// 					GlobalToIdPosition(startGlobalPosition), 
// 					GlobalToIdPosition(endGlobalPosition), 
// 					NavigationType.Open,
// 					agentSize,
// 					tolerance
// 				);
// 			case NavigationType.PassThrough:
// 				return GetPath(
// 					ref aerialClearance,
// 					GlobalToIdPosition(startGlobalPosition), 
// 					GlobalToIdPosition(endGlobalPosition), 
// 					NavigationType.Open | NavigationType.PassThrough,
// 					agentSize,
// 					tolerance
// 				);
// 			default:
// 				throw new Exception($"{agentType} not implemented!");
// 		}
		
// 	}  

// 	/// <summary>
// 	/// Gets a path along the grid towards a given end point.
// 	/// </summary>
// 	/// <param name="start"></param>
// 	/// <param name="end"></param>
// 	/// <param name="agentSize"></param>
// 	/// <param name="tolerance">the amount of leeway given for the end point check.</param>
// 	/// <returns></returns>

// 	private Stack<Vector2> GetPath(ref byte[,] clearanceData, Vector2I start, Vector2I end, NavigationType capability, byte agentSize, byte tolerance = 0){

// 		// if either point is out of bounds.

// 		if(start.X >= gridSize.X || start.Y >= gridSize.Y || start.X < 0 || start.Y < 0
// 		|| end.X >= gridSize.X || end.Y >= gridSize.Y || end.X < 0 || end.Y < 0){
// 			return new();
// 		}

// 		// if either point is within a blocked cell.

// 		if(navigationType[start.X, start.Y] == NavigationType.Blocked 
// 		|| navigationType[end.X, end.Y]     == NavigationType.Blocked){
// 			return new();
// 		}

// 		List<Vector2I> openList = new List<Vector2I>();
// 		HashSet<Vector2I> closedSet = new HashSet<Vector2I>();
		
// 		ref PathCell endPathCell    = ref paths[end.X, end.Y];
// 		ref PathCell startPathCell  = ref paths[start.X, start.Y];
// 		startPathCell = new PathCell(
// 			id: start,
// 			cost: 0, 
// 			heuristic: CalculateHeuristic(start, end)
// 		);

// 		openList.Add(start);
// 		closedSet.Add(start);

// 		Vector2 toleranceLowerBound = Vector2.Zero;
// 		Vector2 toleranceUpperBound = Vector2.Zero;

// 		if(tolerance > 0){
// 			toleranceLowerBound.X = end.X - tolerance;
// 			toleranceUpperBound.X = end.X + tolerance;
// 			toleranceLowerBound.Y = end.Y - tolerance;
// 			toleranceUpperBound.Y = end.Y + tolerance;
// 		}


// 		while(openList.Count > 0){
// 			PathCell current = GetLowestTotalCostPathCell(openList);

// 			if(tolerance > 0){
// 				if(WithinTolerance(toleranceLowerBound, toleranceUpperBound, ref current) == true
// 				&& CalcToleranceBetweenPathCells(current.Id, end) == true){
// 					return ReconstructPath(current);
// 				}
// 			}
// 			else if(current.Id == end){
// 				return ReconstructPath(current);
// 			}

// 			openList.Remove(current.Id);
// 			closedSet.Add(current.Id);

// 			foreach(Vector2I direction in directions){
// 				Vector2I neighbour = current.Id + direction;

// 				// check bounds.
				
// 				if(closedSet.Contains(neighbour) 
// 				|| neighbour.X >= gridSize.X  
// 				|| neighbour.Y >= gridSize.Y){
// 					continue;
// 				}

// 				// Check if terrain is in agent's capability
// 				if ((navigationType[neighbour.X, neighbour.Y] & capability) == 0)
// 					continue;

// 				// check clearance.

// 				if(clearanceData[neighbour.X, neighbour.Y] < agentSize){
// 					continue;
// 				}

// 				// ref PathCell neighbourPathCell = ref paths[neighbourId.X, neighbourId.Y]; 

// 				PathCell neighbourPathCell  = new PathCell(
// 					id: neighbour,
// 					parentId: current.Id,
// 					cost: current.Cost + 1,
// 					heuristic: CalculateHeuristic(neighbour, end)
// 				);

// 				// if already in open list with lower total cost.

// 				ref PathCell existing = ref paths[neighbour.X, neighbour.Y];
// 				if(openList.Contains(neighbour) && neighbourPathCell.Total >= neighbourPathCell.Total){    
// 					continue;
// 				}

// 				// assign the neighbour data.

// 				existing = neighbourPathCell;

// 				openList.Add(neighbour);
// 			}
// 		}
// 		return new();
// 	}

// 	public Vector2I GlobalToIdPosition(Vector2 globalPosition){
// 		return tileMap.LocalToMap(globalPosition);
// 	}

// 	public Vector2 IdToGlobalPosition(Vector2I gridIdPosition){
// 		return tileMap.MapToLocal(gridIdPosition);
// 	}

// 	/// <summary>
// 	/// Check if a path cell is within a given tolerance radius in the grid.
// 	/// </summary>
// 	/// <param name="lowerBound"></param>
// 	/// <param name="upperBound"></param>
// 	/// <param name="currentPathCell"></param>
// 	/// <returns></returns>

// 	private bool WithinTolerance(Vector2 lowerBound, Vector2 upperBound, ref PathCell currentPathCell){
// 		return
// 		currentPathCell.Id.X >= lowerBound.X && currentPathCell.Id.X <= upperBound.X
// 		&& currentPathCell.Id.Y >= lowerBound.Y && currentPathCell.Id.Y <= upperBound.Y;
// 	}

// 	/// <summary>
// 	/// line-of-sight style check that determines if there's and unobstructed straight line between two cells.
// 	/// where every cell in that line must have at least agentSize clearance.
// 	/// Note: modified Bresenham's line algorithm.
// 	/// </summary>
// 	/// <param name="from"></param>
// 	/// <param name="to"></param>
// 	/// <returns></returns>

// 	private bool CalcToleranceBetweenPathCells(Vector2I from, Vector2I to){
// 		int x0 = from.X;
// 		int y0 = from.Y;
// 		int x1 = to.X;
// 		int y1 = to.Y;

// 		// distance between cells.

// 		int dx = Mathf.Abs(x1 - x0);
// 		int dy = Mathf.Abs(y1 - y0);

// 		// direction to step in.

// 		int sx = x0 < x1 ? 1 : -1;
// 		int sy = y0 < y1 ? 1 : -1;

// 		// error term used by Bresenham's algorithm to determine when to step Y in addition to X.
// 		// (or vice versa), keeping the line straight even when slope is not 1.

// 		int err = dx - dy;

// 		while (true){
// 			// Bounds check
// 			if (x0 < 0 || x0 >= gridSize.X || y0 < 0 || y0 >= gridSize.Y){
// 				return false;
// 			}


// 			// Blocked check
// 			if (navigationType[x0,y0] == NavigationType.Blocked){
// 				return false;
// 			}

// 			// Reached destination
// 			if (x0 == x1 && y0 == y1){
// 				break;
// 			}

// 			// step.
// 			int e2 = 2 * err;
// 			if (e2 > -dy){
// 				err -= dy;
// 				x0 += sx;
// 			}
// 			if (e2 < dx){
// 				err += dx;
// 				y0 += sy;
// 			}
// 		}

// 		return true;
// 	}

// 	/// <summary>
// 	/// Constructs a path from a given path cell after a path has been found using GetPath().
// 	/// </summary>
// 	/// <param name="endPathCell"></param>
// 	/// <returns></returns>

// 	private Stack<Vector2> ReconstructPath(PathCell endPathCell){
// 		Stack<Vector2> path = new Stack<Vector2>();
// 		PathCell current = endPathCell;
// 		while(true){
// 			// current.Id + (Vector2I.One * (agentSize-1));
// 			path.Push(IdToGlobalPosition(current.Id));    
// 			if(current.ParentId == new Vector2I(-1,-1)){
// 				break;
// 			}
// 			else{
// 				current = paths[current.ParentId.X, current.ParentId.Y];
// 			}
// 		}
// 		return path;
// 	}

// 	private PathCell GetLowestTotalCostPathCell(List<Vector2I> indexes){
// 		Vector2I index = indexes[0];
// 		PathCell best = paths[index.X, index.Y];
	
// 		for(int i = 1; i < indexes.Count; i++){
// 			index = indexes[i];
// 			PathCell other = paths[index.X, index.Y];
// 			if(other.Total < best.Total){
// 				best = other;
// 			}
// 		}

// 		return best;
// 	}

// 	private int CalculateHeuristic(Vector2I a, Vector2I b){
// 		// Manhattan distance (orthogonal).
// 		// return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
	
// 		// ocitile distance (diagonal)
// 		int dx = Math.Abs(a.X - b.X);
// 		int dy = Math.Abs(a.Y - b.Y);

// 		int D = 10;     // Cost for straight (orthogonal) movement
// 		int D2 = 14;    // Approx. sqrt(2) * 10 for diagonal movement

// 		return D * (dx + dy) + (D2 - 2 * D) * Math.Min(dx, dy);
// 	}
	
// 	public void Insert(Rect2 globalAABB,  NavigationType agentNavigationType, out List<Vector2I> currentFrameIndices){
// 		currentFrameIndices = new List<Vector2I>();

// 		Vector2I minGridPosition = tileMap.LocalToMap(globalAABB.Position - (globalAABB.Size *0.5f));
// 		Vector2I maxGridPosition = tileMap.LocalToMap(globalAABB.Position + (globalAABB.Size *0.5f));
// 		Vector2I gridPos = tileMap.LocalToMap(globalAABB.Position);

// 		for (int y = minGridPosition.Y; y <= maxGridPosition.Y; y++) {
// 			for (int x = minGridPosition.X; x <= maxGridPosition.X; x++) {

// 				Vector2I index = new Vector2I(x,y);
				
// 				currentFrameIndices.Add(index);

// 				if(TileIsInUse(index.X, index.Y, out TileData sharedTileData)==false){
// 					continue;
// 				}

// 				if(locked[x,y]==true){
// 					continue;
// 				}

// 				switch(agentNavigationType){
// 					case NavigationType.Blocked:
// 						navigationType[x,y] = NavigationType.Blocked;
// 						break;
// 					case NavigationType.PassThrough:
// 						navigationType[x,y] = NavigationType.PassThrough;
// 						break;
// 					case NavigationType.Open:
// 						navigationType[x,y] = NavigationType.Open;
// 						break;
// 				}
// 			}
// 		}

// 		// InitialiseGridClearance(NavigationType.Open);
// 		// InitialiseGridClearance(NavigationType.PassThrough);
// 		UpdateClearance(NavigationType.Open, gridPos.X, gridPos.Y);
// 		UpdateClearance(NavigationType.PassThrough, gridPos.X, gridPos.Y);
// 	}

// 	public void Remove(Rect2 globalAABB, List<Vector2I> indices){
// 		Vector2I gridPos = tileMap.LocalToMap(globalAABB.Position);

// 		for(int i = 0 ; i < indices.Count; i++){
			
// 			Vector2I index = indices[i];

// 			if(TileIsInUse(index.X, index.Y, out TileData sharedTileData)==false){
// 				continue;
// 			}
			
// 			if(locked[index.X, index.Y] == true){
// 				continue;
// 			}
// 			navigationType[index.X, index.Y] = NavigationType.Open;
// 		}
// 		// InitialiseGridClearance(NavigationType.Open);
// 		// InitialiseGridClearance(NavigationType.PassThrough);
// 		UpdateClearance(NavigationType.Open, gridPos.X, gridPos.Y);
// 		UpdateClearance(NavigationType.PassThrough, gridPos.X, gridPos.Y);
// 	}

// 	public List<Vector2I> GetCellsInArea(Vector2 originPosition, Vector2I startOffset, Vector2I endOffset){
// 		if(startOffset > endOffset){
// 			throw new InvalidOperationException($"endOffset: {endOffset} cannot be smaller than startOffset {startOffset}");
// 		}
// 		Vector2I centerCell = GlobalToIdPosition(originPosition);
// 		List<Vector2I> cellsInArea = new List<Vector2I>();
		
// 		Vector2I start = centerCell + startOffset;
// 		Vector2I end = centerCell + endOffset;

// 		for(int x = start.X; x <= end.X; x++){
			
// 			if(x<0||x>=gridSize.X){
// 				continue;
// 			}
			
// 			for(int y = start.Y; y <= end.Y; y++){

// 				if(y<0||y>=gridSize.Y){
// 					continue;
// 				}
			
// 				cellsInArea.Add(new Vector2I(x,y));
// 			}
// 		}

// 		return cellsInArea;
// 	}

// 	public List<Vector2I> GetGroundClearanceCells(List<Vector2I> cells, byte clearance){
		
// 		List<Vector2I> filteredCells = new List<Vector2I>();
		
// 		for(int i = 0; i < cells.Count; i++){
// 			Vector2I cellId = cells[i];
// 			if(cellId.X < gridSize.X && cellId.Y < gridSize.Y && groundClearance[cellId.X,cellId.Y] >= clearance){
// 				filteredCells.Add(cellId);
// 			}
// 		}

// 		return filteredCells;
// 	}

// 	public bool IsCellNavigationType(int cellIdX, int cellIdY, NavigationType navigationType){
// 		return this.navigationType[cellIdX,cellIdY] == navigationType;
// 	}

// 	public bool IsCellNavigationType(Vector2I cellId, NavigationType navigationType){
// 		return this.navigationType[cellId.X,cellId.Y] == navigationType;
// 	}

// 	private bool TileIsInUse(int x, int y, out TileData sharedTileData){
// 		Vector2I index = new(x,y);
// 		sharedTileData = tileMap.GetCellTileData(index);
// 		return sharedTileData!=null;
// 	}
// }
