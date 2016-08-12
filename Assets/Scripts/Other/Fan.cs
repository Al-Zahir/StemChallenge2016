using UnityEngine;
using System.Collections;

public class Fan : MonoBehaviour {

    public float angle = 20;

	void FixedUpdate () {
        transform.Rotate(transform.up * angle * Time.deltaTime);
	}
}
