using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Entropek.Util;

public static class Physics{

    // performs a ray cast with multiple hits.

    public static List<CollisionResult> IntersectRay(World3D world, PhysicsRayQueryParameters3D query, int maxHits){
        List<CollisionResult> results = new List<CollisionResult>();
        Array<Rid> exlcudes = new Array<Rid>();

        Vector3 from    = query.From;
        Vector3 to      = query.To;
        
        for(int i = 0; i < maxHits; i++){
            Dictionary result = world.DirectSpaceState.IntersectRay(
                new PhysicsRayQueryParameters3D{
                    From                = from,
                    To                  = to,
                    CollisionMask       = query.CollisionMask,
                    CollideWithAreas    = query.CollideWithAreas,
                    CollideWithBodies   = query.CollideWithBodies,
                    HitFromInside       = query.HitFromInside,
                    HitBackFaces        = query.HitBackFaces,
                    Exclude             = exlcudes
                }
            );            
            if(result.Count <= 0){
                break;
            }

            Vector3 position = (Vector3)result["position"];
            Vector3 normal = (Vector3)result["normal"]; 

            // move slightly past the hit point to avoid hitting the same spot again.


            results.Add(new CollisionResult(
                result["collider"],
                position,
                normal
            ));

            // exculde the collider from the next raycast.

            exlcudes.Add((Rid)result["rid"]);
        }
        return results;
    }
}

public struct CollisionResult{
    public readonly object Collider;
    public readonly Vector3 Position;
    public readonly Vector3 Normal;

    public CollisionResult(object collider, Vector3 position, Vector3 normal){
        Collider = collider;
        Position = position;
        Normal = normal;
    }
}