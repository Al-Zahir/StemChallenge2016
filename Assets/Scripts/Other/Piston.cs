using UnityEngine;
using System.Collections;

public class Piston : MonoBehaviour {

    public float distance = 4;
    public float period = 2;

    private Vector3 start, target;

	// Use this for initialization
	void Start () {
        start = transform.position;
        target = transform.position - transform.up * distance;
	}
	
	void Update () {
        transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0, 1, Mathf.PingPong(Time.time, period) / period));
	}
}
