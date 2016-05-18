using UnityEngine;
using System.Collections;

public class PlayerLastSeen : MonoBehaviour {

	public Transform playerHologram;

	private Transform hologram;

	void Awake(){

		hologram = null;

	}

	public void Appeared(){

		if (hologram != null) 
			Destroy(hologram.gameObject);

	}

	public void Disappeared(){

		if (hologram != null)
			Destroy (hologram.gameObject);

		hologram = Instantiate (playerHologram, transform.position, transform.rotation) as Transform;

		Transform[] hologramJoints = hologram.GetComponentsInChildren <Transform> ();
		Transform[] transformJoints = transform.GetComponentsInChildren<Transform> ();
		
		for (int i = 0; i < hologramJoints.Length; i++) {
			
			for (int j = 0; j < transformJoints.Length; j++){
				
				if (hologramJoints[i].name.CompareTo(transformJoints[j].name) == 0){
					
					hologramJoints[i].position = transformJoints[j].position;
					hologramJoints[i].rotation = transformJoints[j].rotation;
					
				}
				
			}
			
		}
	}

}
