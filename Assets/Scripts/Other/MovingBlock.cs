using UnityEngine;
using System.Collections;

public class MovingBlock : MonoBehaviour {

    public Transform target;
    public float time = 5f;
    private Vector3 initialPosition;
    private Vector3 vel = Vector3.zero;
    private bool toTarget = true;

    void Start()
    {
        initialPosition = transform.position;
    }

	void FixedUpdate () {
        transform.position = Vector3.SmoothDamp(transform.position, toTarget ? target.position : initialPosition, ref vel, time);
        if (Vector3.Distance(target.position, transform.position) < 0.1f)
            toTarget = false;
        else if (Vector3.Distance(initialPosition, transform.position) < 0.01f)
            toTarget = true;
	}
}
