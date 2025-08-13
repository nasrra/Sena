// using Entropek.Collections;
// using Godot;

// public partial class ProjectileManager : Node{
//     public static ProjectileManager Instance {get;private set;}

//     private SwapbackList<Projectile> projectiles = new SwapbackList<Projectile>();


//     /// 
//     /// Base.
//     /// 


//     public override void _EnterTree(){
//         base._EnterTree();
//         Instance = this;
//     }

//     public override void _ExitTree(){
//         base._ExitTree();
//         Instance = null;
//     }


//     /// 
//     /// Functions.
//     /// 


//     public void AddProjectile(Projectile projectile){
//         projectiles.Add(projectile);
//     }

//     public bool RemoveProjectile(Projectile projectile){
//         return projectiles.Remove(projectile);
//     }

//     public void RemoveAllProjectiles(){
//         for(int i = 0; i < projectiles.Count; i++){
//             projectiles[i].QueueFree();
//         }
//         projectiles.Clear();
//     }
// }
