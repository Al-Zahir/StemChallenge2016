using UnityEngine;
using System.Collections;

public class Scroll : MonoBehaviour {

    public float speed = 1;
	
	// Update is called once per frame
	void Update () {
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
	}
}
