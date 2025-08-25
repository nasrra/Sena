using Godot;
using System;

public partial class EnemyManager : Node{
	public static EnemyManager Singleton {get;private set;}
	[Export] private GridMap enemyGroups;
	private Entropek.Collections.SwapbackList<Enemy>[] enemies = new Entropek.Collections.SwapbackList<Enemy>[9];
	public event Action<int> OnEnemyGroupKilled;

	public override void _EnterTree(){
		base._EnterTree();
		Singleton = this;
		for (int i = 0; i < enemies.Length; i++){
			enemies[i] = new Entropek.Collections.SwapbackList<Enemy>();
		}

	}

	public override void _ExitTree(){
		base._ExitTree();
	}

	public void AddEnemy(Enemy enemy){
		int enemyGroup = GetEnemyGroup(enemy.GlobalPosition);
		enemies[enemyGroup].Add(enemy);
		// GD.Print($"enemy added to group [{enemyGroup}]");
	}

	public void RemoveEnemy(Enemy enemy){
		int enemyGroup = GetEnemyGroup(enemy.GlobalPosition); 
		enemies[enemyGroup].Remove(enemy);
		if(enemies[enemyGroup].Count == 0){
			OnEnemyGroupKilled?.Invoke(enemyGroup);
			// GD.Print($"enemy group [{enemyGroup}] killed");
		}
	}

	private int GetEnemyGroup(Vector3 globalPosition){
		Vector3I mapPosition = enemyGroups.LocalToMap(globalPosition);
		int group = enemyGroups.GetCellItem(mapPosition);
		if(group==-1){
			throw new Exception("spawned enemy is not within a enemy group area!");
		}
		return group;
	}
}
