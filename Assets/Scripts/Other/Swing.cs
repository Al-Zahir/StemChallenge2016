using UnityEngine;
using System.Collections;

public class Swing : MonoBehaviour {

    public float degrees = 10;
    public float period = 1;
    private Quaternion q1, q2;

	// Use this for initialization
    void Start()
    {
        q1 = Quaternion.AngleAxis(degrees, transform.right) * transform.rotation;
        q2 = Quaternion.AngleAxis(-degrees, transform.right) * transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.Slerp(q1, q2, 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI / period * Time.time));
	}
}
