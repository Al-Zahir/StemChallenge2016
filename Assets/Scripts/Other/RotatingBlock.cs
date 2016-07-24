using UnityEngine;
using System.Collections;

public class RotatingBlock : MonoBehaviour {

    public Transform target;
    public float time = 5f;
    public float wait = 2f;
    private Quaternion initialRotation;
    private bool toTarget = true;
    private float timeCtr;

    void Start()
    {
        initialRotation = transform.rotation;
    }

	void FixedUpdate () {
        transform.rotation = Quaternion.Slerp(toTarget ? initialRotation : target.rotation, toTarget ? target.rotation : initialRotation, timeCtr < time ? timeCtr / time : 1f);
        if (timeCtr > time + wait)
        {
            toTarget = !toTarget;
            timeCtr = 0;
        }

        timeCtr += Time.deltaTime;
	}
}
