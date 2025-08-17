using Godot;
using System;
using System.Collections.Generic;
using System.Data;

namespace Entropek.Ai;

public partial class WayfindingGrid3D : GridMap{

	public const byte MaxAgentSize = 8;

	public static WayfindingGrid3D Singleton {get;private set;}

	/// 
	/// links the mesh index of a grid maps mesh library to a NavigationType.
	/// 

	[Export] Godot.Collections.Array<NavigationType> meshIndexToNavigationType = new Godot.Collections.Array<NavigationType>();

	/// 
	/// clearnace layers:
	/// 	The index of the array is the clearance layer index.
	/// 	The navigation type associated defines what navigation cell types are used as 
	/// 	"clear" cells to calculate clearance.
	/// 

	[Export] Godot.Collections.Array<NavigationType> clearanceLayers = new Godot.Collections.Array<NavigationType>();

	/// 
	/// clearance:
	/// 	x - clearanceLayerId.
	/// 	y - cell index x.
	/// 	z - cell index y.
	/// 	w - cell index z.
	/// 

	private byte[,,,] clearance;

	private PathCell3D[,,] paths;
	private bool[,,] locked;
	private NavigationType[,,] navigationType;

	private static readonly Vector3I[] directions = [
		new(-1, -1, -1), // LeftDownBack
		new(-1, -1,  0), // LeftDown
		new(-1, -1,  1), // LeftDownFront

		new(-1,  0, -1), // LeftBack
		new(-1,  0,  0), // Left
		new(-1,  0,  1), // LeftFront

		new(-1,  1, -1), // LeftUpBack
		new(-1,  1,  0), // LeftUp
		new(-1,  1,  1), // LeftUpFront

		new( 0, -1, -1), // DownBack
		new( 0, -1,  1), // DownFront

		new( 0,  0, -1), // Back
		new( 0,  0,  1), // Front

		new( 0,  1, -1), // UpBack
		new( 0,  1,  1), // UpFront

		new( 1, -1, -1), // RightDownBack
		new( 1, -1,  0), // RightDown
		new( 1, -1,  1), // RightDownFront

		new( 1,  0, -1), // RightBack
		new( 1,  0,  0), // Right
		new( 1,  0,  1), // RightFront

		new( 1,  1, -1), // RightUpBack
		new( 1,  1,  0), // RightUp
		new( 1,  1,  1)  // RightUpFront
	];


	[Export] private Font debugFont;
	private Color debugBlockedColour        = new Color(1,0,0,1f);
	private Color debugPassThroughColour    = new Color(0.5f,0.5f,0,1f);
	private Color debugOpenColour           = new Color(1f,1f,1f,1f);
	private Color debugNoneColour           = new Color(0.5f,0.5f,0.5f,1f);
	public Vector3I GridSize {get => gridSize;}
	private Vector3I gridSize 				= Vector3I.Zero;	

	private bool drawDebug = false;

	// orthogonal (Manhattan)

	// private Vector2I[] directions = new Vector2I[]{
	//     new Vector2I(1, 0),     // left 
	//     new Vector2I(-1, 0),    // right
	//     new Vector2I(0, 1),     // up
	//     new Vector2I(0, -1)     // down
	// };

	// diagonal (Ocitile)

	public override void _Ready(){
		base._Ready();
		Visible=false;
		Singleton=this;
		Initialise();
	}


	/// 
	/// Initialisation.
	/// 


	private void Initialise(){ // <-- inits static environment.
		CalculateGridProperties();
		AllocateArrayData();
		SetCellNavigationFromGrid();
		InitialiseGridClearance();
		// CallDeferred("Test");
	}

	private void Test(){
		CalculateClearance(new Vector3I(2,1,11), NavigationType.Open);
	}

	private void CalculateGridProperties(){
		Godot.Collections.Array<Vector3I> usedCells = GetUsedCells();
		if(usedCells.Count == 0){
			return;
		}

		Vector3I min = usedCells[0];
		Vector3I max = usedCells[0];

		foreach(Vector3I cell in usedCells){
			min = min.Min(cell);
			max = max.Max(cell);
		}

		gridSize = max - min + Vector3I.One;
	}

	private void AllocateArrayData(){
		clearance 		= new byte[clearanceLayers.Count, gridSize.X, gridSize.Y, gridSize.Z];
		locked          = new bool[gridSize.X, gridSize.Y, gridSize.Z];
		paths 			= new PathCell3D[gridSize.X, gridSize.Y, gridSize.Z];
		navigationType  = new NavigationType[gridSize.X, gridSize.Y, gridSize.Z];
	}

	private void SetCellNavigationFromGrid(){
		for(int x = 0; x < gridSize.X; x++){
		for(int y = 0; y < gridSize.Y; y++){
		for(int z = 0; z < gridSize.Z; z++){
			if(CellIsInUse(x,y,z, out int meshIndex) == false){
				navigationType[x,y,z] = NavigationType.None;
				locked[x,y,z] = true;
				continue;
			}

			NavigationType navigation = meshIndexToNavigationType[meshIndex];
			
			switch(navigation){
				case NavigationType.Blocked:
					navigationType[x,y,z] = NavigationType.Blocked;
					locked[x,y,z] = true;
					break;
				case NavigationType.PassThrough:
					navigationType[x,y,z] = NavigationType.PassThrough;
					locked[x,y,z] = true;
					break;
				case NavigationType.Open:
					navigationType[x,y,z] = NavigationType.Open;
					locked[x,y,z] = false;
					break;
			}
		}}}
	}

	private void InitialiseGridClearance(){
		for(int i = 0; i < clearanceLayers.Count; i++){
		for(int x = 0; x < gridSize.X; x++){
		for(int y = 0; y < gridSize.Y; y++){
		for(int z = 0; z < gridSize.Z; z++){
			clearance[i,x,y,z] = CalculateClearance(x,y,z,clearanceLayers[i]);
		
		}}}}
	}


	//
	// runtime functions.
	//


	// protected void UpdateClearance(NavigationType agentType, int cx, int cy){
	// 	byte[,] clearance = agentType == NavigationType.PassThrough ? aerialClearance : groundClearance;
	// 	NavigationType blockTypes = agentType == NavigationType.PassThrough
	// 		? NavigationType.Blocked | NavigationType.PassThrough
	// 		: NavigationType.Blocked;

	// 	// Calculate the maximum possible clearance from the center point (cx, cy)
	// 	int maxClearance = Math.Min(
	// 		Math.Min(cx, gridSize.X - 1 - cx),
	// 		Math.Min(cy, gridSize.Y - 1 - cy)
	// 	);

	// 	// GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");

	// 	// Loop through potential clearances
	// 	for (int i = 0; i <= maxClearance; i++){
	// 		int left = cx - i;
	// 		int right = cx + i;
	// 		int top = cy - i;
	// 		int bottom = cy + i;
			
	// 		bool noChange = true;

	// 		// Check the four borders (left, right, top, bottom) at once
	// 		for (int j = -i; j <= i; j++)
	// 		{
	// 			Vector2I topCell = new(cx + j, top);
	// 			byte newTopClearance = CalculateClearance(topCell.X, topCell.Y, blockTypes);
	// 			if(clearance[topCell.X, topCell.Y] != newTopClearance){
	// 				clearance[topCell.X, topCell.Y] = newTopClearance;
	// 				noChange = false;
	// 				// debugDraw.Call("rect",tileMap.MapToLocal(topCell), Vector2.One*8, new Color(0,1f,0f), 8f, 6f);
	// 			}
	// 			else{
	// 				// debugDraw.Call("rect",tileMap.MapToLocal(topCell), Vector2.One*8, new Color(1,0f,0f), 8f, 6f);
	// 			}

	// 			Vector2I botCell = new(cx + j, bottom);
	// 			byte newBotClearance = CalculateClearance(botCell.X, botCell.Y, blockTypes);
	// 			if(clearance[botCell.X, botCell.Y] != newBotClearance){
	// 				clearance[botCell.X, botCell.Y] = newBotClearance;
	// 				noChange = false;
	// 				// debugDraw.Call("rect",tileMap.MapToLocal(botCell), Vector2.One*8, new Color(0,1f,0f), 8f, 6f);
	// 			}
	// 			else{
	// 				// debugDraw.Call("rect",tileMap.MapToLocal(botCell), Vector2.One*8, new Color(1,0f,0f), 8f, 6f);
	// 			}

	// 			Vector2I leftCell = new(left, cy + j);
	// 			byte newLeftClearance = CalculateClearance(leftCell.X, leftCell.Y, blockTypes);
	// 			if(clearance[leftCell.X, leftCell.Y] != newLeftClearance){
	// 				clearance[leftCell.X,leftCell.Y] = newLeftClearance;
	// 				noChange = false;
	// 				// debugDraw.Call("rect",tileMap.MapToLocal(leftCell), Vector2.One*8, new Color(0,1f,0f), 8f, 6f);
	// 			}
	// 			else{
	// 				// debugDraw.Call("rect",tileMap.MapToLocal(leftCell), Vector2.One*8, new Color(1,0f,0f), 8f, 6f);
	// 			}

	// 			Vector2I rightCell = new(right, cy + j);
	// 			byte newRightClearance = CalculateClearance(rightCell.X, rightCell.Y, blockTypes);
	// 			if(clearance[rightCell.X, rightCell.Y] != newRightClearance){
	// 				clearance[rightCell.X, rightCell.Y] = newRightClearance;
	// 				noChange = false;
	// 				// debugDraw.Call("rect",tileMap.MapToLocal(rightCell), Vector2.One*8, new Color(0,1f,0f), 8f, 6f);
	// 			}
	// 			else{
	// 				// debugDraw.Call("rect",tileMap.MapToLocal(rightCell), Vector2.One*8, new Color(1,0f,0f), 8f, 6f);
	// 			}
	// 		}

	// 		// If blocked, break and set the clearance
	// 		if (noChange==true){
	// 			break;
	// 		}
	// 	}
	// }

	public override void _Process(double delta){
		base._Process(delta);
	}

	double deltaAdd = 0;

	public override void _PhysicsProcess(double delta){
		base._PhysicsProcess(delta);
		DrawDebug();
	}

    public void DrawDebug(){
		if(drawDebug == false){
			return;
		}

		int clearanceLayer = 0;
		int textSize = 24;
		float drawDuration = 0.167f;

		for(int x = 0; x < gridSize.X; x++){
		for(int y = 0; y < gridSize.Y; y++){
		for(int z = 0; z < gridSize.Z; z++){
			
			string clearanceText = ""+clearance[clearanceLayer, x,y,z];
			Vector3 globalPosition = MapToLocal(new Vector3I(x,y,z));

			switch(navigationType[x,y,z]){
				case NavigationType.Blocked:
					// DebugDraw3D.DrawBox(globalPosition, Quaternion.Identity, Vector3.One * 0.1f, debugBlockedColour, true, drawDuration);
					DebugDraw3D.DrawText(globalPosition,clearanceText, textSize, debugBlockedColour, drawDuration);
				break;
				case NavigationType.PassThrough:
					// DebugDraw3D.DrawBox(globalPosition, Quaternion.Identity, Vector3.One * 0.1f, debugPassThroughColour, true, drawDuration);
					DebugDraw3D.DrawText(globalPosition,clearanceText, textSize, debugPassThroughColour, drawDuration);
				break;
				case NavigationType.Open:
					// DebugDraw3D.DrawBox(globalPosition, Quaternion.Identity, Vector3.One * 0.1f, debugOpenColour, true, drawDuration);
					DebugDraw3D.DrawText(globalPosition,clearanceText, textSize, debugOpenColour, drawDuration);
				break;
				case NavigationType.None:
					// DebugDraw3D.DrawBox(globalPosition, Quaternion.Identity, Vector3.One * 0.1f, debugNoneColour, true, drawDuration);
				break;
			}
		}}}                


    }


	// /// 
	// /// Clearance.
	// /// 

	public byte CalculateClearance(int cx, int cy, int cz, NavigationType capability){
		return CalculateClearance(new Vector3I(cx, cy, cz), capability);
	}

	public byte CalculateClearance(Vector3I cell, NavigationType capability){
		if(IsCellValid(cell) == false || IsCellNavigable(cell,capability)==false){
			return 0;
		}


		byte maxDistanceReached = 0;
		byte minDistanceReached = MaxAgentSize;

		HashSet<Vector3I> visited = new HashSet<Vector3I>();
		Queue<(Vector3I cell, byte distance)> fillRoots = new Queue<(Vector3I, byte)>();

		fillRoots.Enqueue((cell, 0));
		visited.Add(cell);

		while(fillRoots.Count > 0){
			(Vector3I position, byte distance) current = fillRoots.Dequeue();
			
			if(	IsCellValid(current.position) == false ||
				IsCellNavigable(current.position, capability) == false){ 
				continue;
			}

			maxDistanceReached = Math.Max(maxDistanceReached, current.distance);
			if(maxDistanceReached >= MaxAgentSize){
				break;
			}

			
			for (int z = -1; z <= 1; z++) {
				for (int x = -1; x <= 1; x++) {
					Vector3I offset = new(x, 0, z); // Z + 1 is forward
					Vector3I startCell = current.position + offset;

					if (visited.Contains(startCell))
						continue;

					bool foundNavigableCell = IsCellSliceNavigable(
						startCell,
						capability,
						out Vector3I navigableCell,
						out Vector3I aboveCell,
						out Vector3I belowCell
					);

					if (foundNavigableCell) {
						if(!visited.Contains(navigableCell)){
							fillRoots.Enqueue((navigableCell, (byte)(current.distance + 1)));
						}
					}
					else{
						minDistanceReached = Math.Min(minDistanceReached, current.distance);
					}

					visited.Add(startCell);
					visited.Add(aboveCell);
					visited.Add(belowCell);
				}
			}
		}
		return (byte)(minDistanceReached + 1);
	}

	private bool IsCellSliceNavigable(Vector3I startCell, NavigationType capability, out Vector3I navigableCell, out Vector3I aboveCell, out Vector3I belowCell){
		aboveCell 		= startCell + Vector3I.Up;
		belowCell 		= startCell + Vector3I.Down;
		navigableCell 	= Vector3I.Zero;
		
		// check if the cell is None as well for proper elevation navigation checking.

		bool belowIsNavigable 		= IsCellValid(belowCell) && IsCellNavigable(belowCell, capability | NavigationType.None);
		bool startIsNavigable 		= IsCellValid(startCell) && IsCellNavigable(startCell, capability | NavigationType.None);
		bool aboveIsNavigable 		= IsCellValid(aboveCell) && IsCellNavigable(aboveCell, capability | NavigationType.None);

		if(belowIsNavigable && startIsNavigable && IsCellNavigable(belowCell, capability)){
			navigableCell = belowCell;
			return true;
		}
		if(startIsNavigable && IsCellNavigable(startCell, capability)){
			navigableCell = startCell;
			return true;
		}
		if(aboveIsNavigable && IsCellNavigable(aboveCell, capability)){
			navigableCell = aboveCell;
			return true;
		}
		return false;
	}

	private bool IsCellNavigable(Vector3I cell, NavigationType capability){
		return IsCellNavigable(cell.X, cell.Y, cell.Z, capability);
	}

	private bool IsCellNavigable(int x, int y, int z, NavigationType capability){
		
		// if the cell contains any flag in capability.
		
		return (capability & navigationType[x,y,z]) != 0; 
	}




	// /// <summary>
	// /// Gets a path along the grid towards a given end point.
	// /// </summary>
	// /// <param name="startGlobalPosition"></param>
	// /// <param name="endGlobalPosition"></param>
	// /// <param name="agentSize"></param>
	// /// <param name="tolerance">tolerance is the amount of leeway given for the end point check.</param>
	// /// <returns></returns>

	public Stack<Vector3> GetPath(Vector3 startGlobalPosition, Vector3 endGlobalPosition, NavigationType agentType, byte agentSize, byte tolerance = 0){
		int clearanceLayer = GetClearanceLayer(agentType);
		return GetPath(LocalToMap(startGlobalPosition), LocalToMap(endGlobalPosition), clearanceLayer, agentType, agentSize, tolerance);		
	}  

	// /// <summary>
	// /// Gets a path along the grid towards a given end point.
	// /// </summary>
	// /// <param name="start"></param>
	// /// <param name="end"></param>
	// /// <param name="agentSize"></param>
	// /// <param name="tolerance">the amount of leeway given for the end point check.</param>
	// /// <returns></returns>

	private Stack<Vector3> GetPath(Vector3I start, Vector3I end, int clearanceLayer, NavigationType capability, byte agentSize, byte tolerance = 0){
		if(IsCellValid(start) == false || IsCellValid(end) == false || IsCellNavigable(start, capability) == false  || IsCellNavigable(end,capability) == false){
			return new();
		}

		List<Vector3I> openList = new List<Vector3I>();
		HashSet<Vector3I> closedSet = new HashSet<Vector3I>();
		
		ref PathCell3D endPathCell    = ref paths[end.X, end.Y, end.Z];
		ref PathCell3D startPathCell  = ref paths[start.X, start.Y, start.Z];
		startPathCell = new PathCell3D(
			cost: 0, 
			heuristic: CalculateHeuristic(start, end)
		);

		openList.Add(start);
		closedSet.Add(start);

		Vector3I toleranceLowerBound = Vector3I.Zero;
		Vector3I toleranceUpperBound = Vector3I.Zero;

		if(tolerance > 0){
			Vector3I toleranceVector = Vector3I.One * tolerance;;
			toleranceUpperBound += toleranceVector;
			toleranceLowerBound -= toleranceVector;
		}


		while(openList.Count > 0){
			Vector3I currentIndex = GetLowestTotalCostPathCell(openList);
			PathCell3D current = paths[currentIndex.X, currentIndex.Y, currentIndex.Z];

			if(tolerance > 0){
				if(currentIndex <= toleranceUpperBound && currentIndex >= toleranceLowerBound
				&& CalcToleranceBetweenPathCells(currentIndex, end, capability) == true){
					return ReconstructPath(current, currentIndex);
				}
			}
			else if(currentIndex == end){
				return ReconstructPath(current, currentIndex);
			}

			openList.Remove(currentIndex);
			closedSet.Add(currentIndex);

			foreach(Vector3I direction in directions){
				Vector3I neighbour = currentIndex + direction;

				// check bounds.
				
				if(closedSet.Contains(neighbour) || IsCellValid(neighbour)==false){
					continue;
				}

				// Check if terrain is in agent's capability
				if (IsCellNavigable(neighbour, capability)==false){
					continue;
				}

				// check clearance.

				if(clearance[clearanceLayer, neighbour.X, neighbour.Y, neighbour.Z] < agentSize){
					continue;
				}

				PathCell3D neighbourPathCell  = new PathCell3D(
					parentId: currentIndex,
					cost: current.Cost + 1,
					heuristic: CalculateHeuristic(neighbour, end)
				);

				// if already in open list with lower total cost.

				ref PathCell3D existing = ref paths[neighbour.X, neighbour.Y, neighbour.Z];
				if(openList.Contains(neighbour) && neighbourPathCell.Total >= neighbourPathCell.Total){    
					continue;
				}

				// assign the neighbour data.

				existing = neighbourPathCell;

				openList.Add(neighbour);
			}
		}
		return new();
	}

	private Vector3I GetLowestTotalCostPathCell(List<Vector3I> indexes){
		
		Vector3I index 		= indexes[0];		
		Vector3I bestIndex 	= index;
		PathCell3D bestCell = paths[index.X, index.Y, index.Z];

		for(int i = 1; i < indexes.Count; i++){
			index = indexes[i];
			PathCell3D other = paths[index.X, index.Y, index.Z];
			if(other.Total < bestCell.Total){
				bestCell = other;
				bestIndex = index;
			}
		}

		return bestIndex;
	}

	// 	return best;
	// }
	
	private int CalculateHeuristic(Vector3I a, Vector3I b) {
		int dx = Math.Abs(a.X - b.X);
		int dy = Math.Abs(a.Y - b.Y);
		int dz = Math.Abs(a.Z - b.Z);

		const int D = 10;   // Straight move
		// const int D2 = 14;  // 2D diagonal
		// const int D3 = 17;  // 3D diagonal (approx sqrt(3) * 10)
		const int D2 = 20;  // 2D diagonal
		const int D3 = 30;  // 3D diagonal (approx sqrt(3) * 10)

		int minXY = Math.Min(dx, dy);
		int maxXY = Math.Max(dx, dy);
		int straightXY = maxXY - minXY;

		int minXYZ = Math.Min(minXY, dz);
		int midXYZ = Math.Max(Math.Min(Math.Max(dx, dy), dz), minXYZ);
		int maxXYZ = Math.Max(dx, Math.Max(dy, dz));

		return D3 * minXYZ + D2 * (midXYZ - minXYZ) + D * (maxXYZ - midXYZ);
	}

	// /// <summary>
	// /// Check if a path cell is within a given tolerance radius in the grid.
	// /// </summary>
	// /// <param name="lowerBound"></param>
	// /// <param name="upperBound"></param>
	// /// <param name="currentPathCell"></param>
	// /// <returns></returns>

	// /// <summary>
	// /// line-of-sight style check that determines if there's and unobstructed straight line between two cells.
	// /// where every cell in that line must have at least agentSize clearance.
	// /// Note: modified Bresenham's line algorithm.
	// /// </summary>
	// /// <param name="from"></param>
	// /// <param name="to"></param>
	// /// <returns></returns>

	private bool CalcToleranceBetweenPathCells(Vector3I from, Vector3I to, NavigationType capability) {
		int x0 = from.X, y0 = from.Y, z0 = from.Z;
		int x1 = to.X, y1 = to.Y, z1 = to.Z;

		int dx = Mathf.Abs(x1 - x0);
		int dy = Mathf.Abs(y1 - y0);
		int dz = Mathf.Abs(z1 - z0);

		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int sz = z0 < z1 ? 1 : -1;

		int dx2 = dx * 2;
		int dy2 = dy * 2;
		int dz2 = dz * 2;

		int err1, err2;

		// Find the dominant direction (longest axis)
		if (dx >= dy && dx >= dz) {
			err1 = dy2 - dx;
			err2 = dz2 - dx;

			while (x0 != x1) {
				GD.Print("1");
				if (IsCellValid(x0, y0, z0) == false || IsCellNavigable(x0, y0, z0, capability) == false)
					return false;

				if (err1 > 0) {
					y0 += sy;
					err1 -= dx2;
				}
				if (err2 > 0) {
					z0 += sz;
					err2 -= dx2;
				}

				err1 += dy2;
				err2 += dz2;
				x0 += sx;
			}
		}
		else if (dy >= dx && dy >= dz) {
			err1 = dx2 - dy;
			err2 = dz2 - dy;

			while (y0 != y1) {
				GD.Print("2");
				if (IsCellValid(x0, y0, z0) == false || IsCellNavigable(x0, y0, z0, capability) == false)
					return false;

				if (err1 > 0) {
					x0 += sx;
					err1 -= dy2;
				}
				if (err2 > 0) {
					z0 += sz;
					err2 -= dy2;
				}

				err1 += dx2;
				err2 += dz2;
				y0 += sy;
			}
		}
		else {
			err1 = dx2 - dz;
			err2 = dy2 - dz;

			while (z0 != z1) {
				GD.Print("3");
				if (IsCellValid(x0, y0, z0) == false || IsCellNavigable(x0, y0, z0, capability) == false)
					return false;

				if (err1 > 0) {
					x0 += sx;
					err1 -= dz2;
				}
				if (err2 > 0) {
					y0 += sy;
					err2 -= dz2;
				}

				err1 += dx2;
				err2 += dy2;
				z0 += sz;
			}
		}

		GD.Print("end");

		// Final cell check (destination)
		if (IsCellValid(x1, y1, z1) == false || IsCellNavigable(x1, y1, z1, capability) == false)
			return false;

		return true;
	}


	// /// <summary>
	// /// Constructs a path from a given path cell after a path has been found using GetPath().
	// /// </summary>
	// /// <param name="endPathCell"></param>
	// /// <returns></returns>

	private Stack<Vector3> ReconstructPath(PathCell3D endPathCell, Vector3I endPathIndex){
		Stack<Vector3> path = new Stack<Vector3>();
		PathCell3D current = endPathCell;
		path.Push(MapToLocal(endPathIndex));
		while(true){
			if(current.ParentId == new Vector3I(-1,-1,-1)){
				break;
			}
			else{
				path.Push(MapToLocal(current.ParentId));
				current = paths[current.ParentId.X, current.ParentId.Y, current.ParentId.Z];
			}
		}
		return path;
	}


	// public void Insert(Rect2 globalAABB,  NavigationType agentNavigationType, out List<Vector2I> currentFrameIndices){
	// 	currentFrameIndices = new List<Vector2I>();

	// 	Vector2I minGridPosition = tileMap.LocalToMap(globalAABB.Position - (globalAABB.Size *0.5f));
	// 	Vector2I maxGridPosition = tileMap.LocalToMap(globalAABB.Position + (globalAABB.Size *0.5f));
	// 	Vector2I gridPos = tileMap.LocalToMap(globalAABB.Position);

	// 	for (int y = minGridPosition.Y; y <= maxGridPosition.Y; y++) {
	// 		for (int x = minGridPosition.X; x <= maxGridPosition.X; x++) {

	// 			Vector2I index = new Vector2I(x,y);
				
	// 			currentFrameIndices.Add(index);

	// 			if(TileIsInUse(index.X, index.Y, out TileData sharedTileData)==false){
	// 				continue;
	// 			}

	// 			if(locked[x,y]==true){
	// 				continue;
	// 			}

	// 			switch(agentNavigationType){
	// 				case NavigationType.Blocked:
	// 					navigationType[x,y] = NavigationType.Blocked;
	// 					break;
	// 				case NavigationType.PassThrough:
	// 					navigationType[x,y] = NavigationType.PassThrough;
	// 					break;
	// 				case NavigationType.Open:
	// 					navigationType[x,y] = NavigationType.Open;
	// 					break;
	// 			}
	// 		}
	// 	}

	// 	// InitialiseGridClearance(NavigationType.Open);
	// 	// InitialiseGridClearance(NavigationType.PassThrough);
	// 	UpdateClearance(NavigationType.Open, gridPos.X, gridPos.Y);
	// 	UpdateClearance(NavigationType.PassThrough, gridPos.X, gridPos.Y);
	// }

	// public void Remove(Rect2 globalAABB, List<Vector2I> indices){
	// 	Vector2I gridPos = tileMap.LocalToMap(globalAABB.Position);

	// 	for(int i = 0 ; i < indices.Count; i++){
			
	// 		Vector2I index = indices[i];

	// 		if(TileIsInUse(index.X, index.Y, out TileData sharedTileData)==false){
	// 			continue;
	// 		}
			
	// 		if(locked[index.X, index.Y] == true){
	// 			continue;
	// 		}
	// 		navigationType[index.X, index.Y] = NavigationType.Open;
	// 	}
	// 	// InitialiseGridClearance(NavigationType.Open);
	// 	// InitialiseGridClearance(NavigationType.PassThrough);
	// 	UpdateClearance(NavigationType.Open, gridPos.X, gridPos.Y);
	// 	UpdateClearance(NavigationType.PassThrough, gridPos.X, gridPos.Y);
	// }

	// public List<Vector2I> GetCellsInArea(Vector2 originPosition, Vector2I startOffset, Vector2I endOffset){
	// 	if(startOffset > endOffset){
	// 		throw new InvalidOperationException($"endOffset: {endOffset} cannot be smaller than startOffset {startOffset}");
	// 	}
	// 	Vector2I centerCell = GlobalToIdPosition(originPosition);
	// 	List<Vector2I> cellsInArea = new List<Vector2I>();
		
	// 	Vector2I start = centerCell + startOffset;
	// 	Vector2I end = centerCell + endOffset;

	// 	for(int x = start.X; x <= end.X; x++){
			
	// 		if(x<0||x>=gridSize.X){
	// 			continue;
	// 		}
			
	// 		for(int y = start.Y; y <= end.Y; y++){

	// 			if(y<0||y>=gridSize.Y){
	// 				continue;
	// 			}
			
	// 			cellsInArea.Add(new Vector2I(x,y));
	// 		}
	// 	}

	// 	return cellsInArea;
	// }

	// public List<Vector2I> GetGroundClearanceCells(List<Vector2I> cells, byte clearance){
		
	// 	List<Vector2I> filteredCells = new List<Vector2I>();
		
	// 	for(int i = 0; i < cells.Count; i++){
	// 		Vector2I cellId = cells[i];
	// 		if(cellId.X < gridSize.X && cellId.Y < gridSize.Y && groundClearance[cellId.X,cellId.Y] >= clearance){
	// 			filteredCells.Add(cellId);
	// 		}
	// 	}

	// 	return filteredCells;
	// }

	// public bool IsCellNavigationType(int cellIdX, int cellIdY, NavigationType navigationType){
	// 	return this.navigationType[cellIdX,cellIdY] == navigationType;
	// }

	// public bool IsCellNavigationType(Vector2I cellId, NavigationType navigationType){
	// 	return this.navigationType[cellId.X,cellId.Y] == navigationType;
	// }


	/// 
	/// Utility.
	/// 


	private bool CellIsInUse(int x, int y, int z, out int sharedTileData){
		return CellIsInUse(new Vector3I(x,y,z), out sharedTileData);
	}

	private bool CellIsInUse(Vector3I cell, out int meshIndex){
		meshIndex = GetCellItem(cell);
		return meshIndex != -1;
	}

	private bool IsCellValid(int x, int y, int z){
		return 
			x >= 0 
		&& 	x < gridSize.X 
		&& 	y >= 0
		&& 	y < gridSize.Y
		&& 	z >= 0
		&& 	z < gridSize.Z;
	}

	private bool IsCellValid(Vector3I cell){
		return 
			cell.X >= 0 
		&& 	cell.X < gridSize.X 
		&& 	cell.Y >= 0
		&& 	cell.Y < gridSize.Y
		&& 	cell.Z >= 0
		&& 	cell.Z < gridSize.Z;
	}	

	private int GetClearanceLayer(NavigationType navigationType){
		for(int i = 0; i < clearanceLayers.Count; i++){
			if(navigationType == clearanceLayers[i]){
				return i;
			}
		}
		throw new Exception($"{navigationType} has not been setup with a clearance layer!");
	}
}
