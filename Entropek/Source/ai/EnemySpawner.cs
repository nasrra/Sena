// using Entropek.Util;
// using Godot;
// using Godot.Collections;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Net;

// public partial class EnemySpawner : Node3D{
//     [Export] PackedScene groundEnemy;
    
//     [Export] private Timer cooldown;
    
//     [Export] private float radius;
//     [Export] private float groundEnemyNormalUpperBound = 0.45f;
    
//     [Export(PropertyHint.Layers3DPhysics)]
//     public uint EnvironmentLayer; // <-- where to spawn enemies in relation to.
//     [Export(PropertyHint.Layers3DPhysics)]
//     public uint SpawnObstructionLayer; 

//     RandomNumberGenerator random = new RandomNumberGenerator();

//     // a list of cast rays to avoid using the same or a similar direction for spawning enemies.
//     List<Vector3> castRays = new List<Vector3>();

//     public event Action<EnemySpawner> OnAvailable;

//     private const float debugDrawTime = 1f;

//     public override void _Ready(){
//         base._Ready();
//         LinkEvents();
//         random.Randomize();
//         EnemyManager.Instance.AddSpawner(this);
//         cooldown.Start();
//     }

//     public override void _ExitTree(){
//         base._ExitTree();
//         UnlinkEvents();
//     }


//     public override void _PhysicsProcess(double delta){
//         base._PhysicsProcess(delta);
//         // DebugDraw.Sphere(GlobalPosition, radius, Color.Color8(255,255, 255, 255), debugDrawTime);
//     }

//     public void SpawnEnemy(EnemySpawnData spawnData, int amount){

//         for(int i =0; i < amount; i++){
//             Vector3 direction = new Vector3(random.RandfRange(-1,1), random.RandfRange(0,1), random.RandfRange(-1,1));
            
//             castRays.Add(direction);
            
//             Vector3 destination = GlobalPosition + (direction * radius);
//             if(GetSpawnPoint(destination, spawnData.SpawnRadius, spawnData.SpawnType == EnemySpawnType.Ground, out Vector3 spawnPoint)==true){                
//                 Node3D enemy =(Node3D)spawnData.PackedScene.Instantiate();
//                 GetTree().Root.AddChild(enemy);
//                 enemy.GlobalPosition = spawnPoint;
//             }
//         }

//         castRays.Clear();
//         cooldown.Start();
//     }

//     public bool GetSpawnPoint(Vector3 position, float spawnAreaRadius, bool floor, out Vector3 spawnPoint){        
        
//         // get collision points from the GlobalPosition to the end point.
        
//         List<CollisionResult> results = Physics.IntersectRay(
//             GetWorld3D(), 
//             new PhysicsRayQueryParameters3D{
//                 From = GlobalPosition,
//                 To = position,
//                 CollisionMask = EnvironmentLayer,
//                 HitFromInside = false,
//             },
//             3
//         );


//         if(results.Count >0){

//             // get collision points from the collision point to the GlobalPosition.

//             for(int i = 0; i < results.Count; i++){
//                 if(HandleCollisionSpawnVector(results[i].Position, GlobalPosition, spawnAreaRadius, floor, out spawnPoint)==true){
//                     return true;
//                 }
//                 else{
//                     continue;
//                 }
//             }
//         }

//         return HandleSpawnVector(GlobalPosition, position, spawnAreaRadius, floor, out spawnPoint);
//     }

//     /// <summary>
//     /// Handles spawn vectors that have collided with an obstruction.
//     /// Shooting a ray back to this spawner to check if there are any obstructions behind.
//     /// Determining if the area is sufficient to spawn an enemy.
//     /// </summary>
//     /// <param name="start">The point where the collision happened.</param>
//     /// <param name="end">The position to trace back to.</param>
//     /// <param name="spawnAreaRadius">The radius used for a sphere intersect check, determining if there is enough space to spawn.</param>
//     /// <param name="floor">Whether or not the spawn point should stick to the ground.</param>
//     /// <param name="spawnPoint">The point to spawn the enemy.</param>
//     /// <returns>true if the spawn point is viable; otherwise false.</returns>

//     private bool HandleCollisionSpawnVector(Vector3 start, Vector3 end, float spawnAreaRadius, bool floor, out Vector3 spawnPoint){
//         Dictionary rayResult = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D{
//             From = start, // results[i].Position
//             To = end, // GlobalPosition
//             CollisionMask = EnvironmentLayer,
//             HitFromInside = false,
//         });

//         // get the position of the collision going back to us.

//         Vector3 position = rayResult.Count > 0 ? (Vector3)rayResult["position"] : GlobalPosition;

//         // direction from the collision point to the 

//         Vector3 vectorDirection = start - position;
        
//         // set the spawn point to half way across the space.
        
//         spawnPoint = start - (vectorDirection * 0.5f);

//         // ground enemies: check the ground and spawn it there.

//         if(floor == true){
//             if(FloorSpawnPoint(spawnPoint, out Vector3 flooredSpawnPoint)==false){
//                 return false;
//             }

//             // move the spawn point up from the floor so the spawn area sphere doesnt go into the floor. 

//             spawnPoint = flooredSpawnPoint + (Vector3.Up * (spawnAreaRadius+0.01f)); // <-- padding.

//             if(IsSpawnAreaObstructed(spawnPoint, spawnAreaRadius)==false){
//                 return true;
//             }
//         }

//         // aerial enemies: just see if the location has sufficient space.

//         else if(IsSpawnAreaObstructed(spawnPoint, spawnAreaRadius)==false){
//             return true;
//         }

//         DebugDraw.Line(start, position, new Color(255,0,0, 0.5f), debugDrawTime);

//         return false;
//     }

//     /// <summary>
//     /// Handles spawn vectors that have not collided with any obstruction.
//     /// </summary>
//     /// <param name="start">The start point of the spawn vector.</param>
//     /// <param name="end">The end point of the spawn vector.</param>
//     /// <param name="spawnAreaRadius">The radius used for a sphere intersect check, determining if there is enough space to spawn.</param>
//     /// <param name="floor">Whether or not the spawn point should stick to the ground.</param>
//     /// <param name="spawnPoint">The point to spawn the enemy.</param>
//     /// <returns>true if the spawn point is viable; otherwise false.</returns>

//     private bool HandleSpawnVector(Vector3 start, Vector3 end, float spawnAreaRadius, bool floor, out Vector3 spawnPoint){
//         // set the spawn point to half way across the space.

//         spawnPoint = start + ((end - start) * 0.5f);
//         DebugDraw.Sphere(spawnPoint, 0.5f, new Color(255,255,255, 155), debugDrawTime);

//         if(floor == true){
//             if(FloorSpawnPoint(spawnPoint, out Vector3 flooredSpawnPoint)==false){
//                 return false;
//             }

//             // move the spawn point up from the floor so the spawn area sphere doesnt go into the floor. 

//             spawnPoint = flooredSpawnPoint + (Vector3.Up * (spawnAreaRadius+0.01f)); // <-- padding.

//             if(IsSpawnAreaObstructed(spawnPoint, spawnAreaRadius)==false){
//                 return true;
//             }
//         }
//         else if(IsSpawnAreaObstructed(spawnPoint, spawnAreaRadius)==false){
//             return true;
//         }
            
//         DebugDraw.Line(GlobalPosition, end, new Color(255,0,0, 155), debugDrawTime);
    
//         return false;
//     }

//     /// <summary>
//     /// Sticks a spawn point to the first colliding point below it, determined by the EnvironmentalLayer. 
//     /// </summary>
//     /// <param name="unflooredPosition">The position to floor.</param>
//     /// <param name="spawnPoint">The floored position.</param>
//     /// <returns>true if the position was successfully floored; otherwise false.</returns>

//     private bool FloorSpawnPoint(Vector3 unflooredPosition, out Vector3 spawnPoint){
//         spawnPoint = Vector3.Zero;

//         // Vector3 spawnPoint = results[i].Position - (vectorDirection * 0.5f);

//         Dictionary floorResult = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D{
//             From = unflooredPosition,
//             To = unflooredPosition + Vector3.Down * radius, // cast a ray to the floor of the radius of this enemy spawner.
//             CollisionMask = EnvironmentLayer,
//             HitFromInside = true,
//         });

//         // if we hit the floor and its within the angled threshold.

//         if(floorResult.Count > 0 && ((Vector3)floorResult["normal"]).Y >= groundEnemyNormalUpperBound){
//             spawnPoint = (Vector3)floorResult["position"]; // <-- padding.
//             return true;
//         }

//         return false;
//     }

//     /// <summary>
//     /// Performs a sphere intersect shape check to determine if a given position is obstructed; determined by the SpawnObstructionLayer.
//     /// </summary>
//     /// <param name="spawnPoint">The position to spawn the enemy.</param>
//     /// <param name="spawnAreaRadius">The radius of the sphere used to intersect check for obstructions.</param>
//     /// <returns></returns>

//     private bool IsSpawnAreaObstructed(Vector3 spawnPoint, float spawnAreaRadius){

//         // determine if there is anything obstructing the spawn point.

//         Array<Dictionary> shapeResult = GetWorld3D().DirectSpaceState.IntersectShape(new PhysicsShapeQueryParameters3D{
//             Shape = new SphereShape3D{Radius = spawnAreaRadius},
//             Transform = new Transform3D(Basis.Identity, spawnPoint),
//             CollisionMask = SpawnObstructionLayer,
//         });

//         DebugDraw.Sphere(spawnPoint, 0.1f, new Color(255,255,255, 155), debugDrawTime);

//         // if there is enough leeway to spawn the enemy.

//         return shapeResult.Count > 0;
//     }

//     private void TriggerAvailableSignal(){
//         OnAvailable?.Invoke(this);
//     }

//     private void LinkEvents(){
//         cooldown.Timeout += TriggerAvailableSignal;
//     }

//     private void UnlinkEvents(){
//         cooldown.Timeout -= TriggerAvailableSignal;
//     }
// }
