using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour {

	public float health = 100f;
	public Transform enemyRagdoll;

	public void TakeDamage(float amount){
		
		health -= amount;

		if (health <= 0f)
			Death ();

	}

	void Death(){

		Transform ragdoll = Instantiate (enemyRagdoll, transform.position, transform.rotation) as Transform;
		
		Transform[] ragdollJoints = ragdoll.GetComponentsInChildren <Transform> ();
		Transform[] transformJoints = transform.GetComponentsInChildren<Transform> ();
		
		for (int i = 0; i < ragdollJoints.Length; i++) {
			
			for (int j = 0; j < transformJoints.Length; j++){
				
				if (ragdollJoints[i].name.CompareTo(transformJoints[j].name) == 0){
					
					ragdollJoints[i].position = transformJoints[j].position;
					ragdollJoints[i].rotation = transformJoints[j].rotation;
					
				}
				
			}
			
		}

	}

}
