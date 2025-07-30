using Godot;

public partial class FireFeather : Projectile{
	
	protected override void OnCollision(Node2D node){
		string layer = PhysicsManager.Singleton.GetPhysics2DLayerName((node as CollisionObject2D).CollisionLayer);
		switch (layer){
			case "Enemy":
				HandleOnHitEnemy(node as Enemy);
			break;
			case "Interactable":
				node.GetParent().GetNode<EmberHolder>(EmberHolder.NodeName).LitState();
			break;
		}

		QueueFree();
	}
}
