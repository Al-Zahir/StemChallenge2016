using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

	private Rigidbody rigidbody;
	public float initialVelocity = 100;
	public float distance = 50;
	private Quaternion[] lastRot = new Quaternion[2];
	private int updateCtr = -1;

	void Awake(){

		rigidbody = transform.GetComponent<Rigidbody> ();

	}
	//im tlalkoling lok reastart the call here
	void Fire () {

		float angle = (Mathf.Asin (distance * -Physics.gravity.y / Mathf.Pow (initialVelocity, 2)) * Mathf.Rad2Deg) / 2.0f;


		rigidbody.velocity = initialVelocity * new Vector3 (0, Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
		Debug.Log (rigidbody.velocity);
	
	}

	void FixedUpdate()
	{
		if (updateCtr == -1) {
			for (int i = 0; i < lastRot.Length; i++)
				lastRot [i] = transform.rotation;
			updateCtr = 0;
		}
		else
			lastRot[updateCtr] = transform.rotation;
		
		updateCtr++;
		if (updateCtr >= lastRot.Length)
			updateCtr = 0;
	}

	// Update is called once per frame
	void Update () {

		if (rigidbody != null && rigidbody.velocity != Vector3.zero) {
			transform.rotation = Quaternion.LookRotation (rigidbody.velocity) * Quaternion.Euler (90, 0, 0);
			//rigidbody.velocity = Vector3.zero;
			//Destroy (rigidbody);
		}
	
	}

	void OnCollisionEnter(Collision col){

		if (rigidbody) {
			rigidbody.velocity = Vector3.zero;
			Destroy (GetComponent<DontGoThroughThings>());
			Destroy (rigidbody);
			transform.parent = col.transform;
			transform.rotation = lastRot[(updateCtr - 1 + lastRot.Length) % lastRot.Length];
			//transform.rotation = Quaternion.FromToRotation (Vector3.up, col.contacts [0].normal);
		}

	}
}
