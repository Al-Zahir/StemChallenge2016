using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

	private Rigidbody rigidbody;
	public float initialVelocity = 100;
	public float distance = 50;

	// Use this for initialization
	void Start () {

		rigidbody = transform.GetComponent<Rigidbody> ();
		float angle = (Mathf.Asin (distance * -Physics.gravity.y / Mathf.Pow (initialVelocity, 2)) * Mathf.Rad2Deg) / 2.0f;


		rigidbody.velocity = initialVelocity * new Vector3 (0, Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
		Debug.Log (rigidbody.velocity);
	
	}
	
	// Update is called once per frame
	void Update () {

		if (rigidbody != null && rigidbody.velocity != Vector3.zero) {
			transform.rotation = Quaternion.LookRotation (rigidbody.velocity) * Quaternion.Euler (90, 0, 0);
			//rigidbody.velocity = Vector3.zero;
			//Destroy (rigidbody);
		}
	
	}

	void OnTriggerEnter(Collider col){

		rigidbody.velocity = Vector3.zero;
		Destroy (rigidbody);
	
	}
}
