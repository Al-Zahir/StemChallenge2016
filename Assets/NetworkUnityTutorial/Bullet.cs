using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	void OnCollisionEnter(Collision col){

		Destroy (gameObject);

		var hit = col.gameObject;
		var health = hit.GetComponent<Health> ();

		if (health != null)
			health.TakeDamage (10);

	}

}
