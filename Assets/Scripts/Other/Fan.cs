using UnityEngine;
using System.Collections;

public class Fan : MonoBehaviour {

    public float angle = 20;

	void FixedUpdate () {
        transform.Rotate(Vector3.up * angle * Time.deltaTime, Space.Self);
	}
}
