using UnityEngine;
using System.Collections;

public class TempleDoor : MonoBehaviour {

    public Transform doorTarget;
    private Vector3 doorStart;
    public float doorTime;
    public bool open = false;

	// Use this for initialization
	void Start () {
        doorStart = transform.position;
	}

    public void RequestOpen()
    {
        if(!open)
        {
            open = true;
            StartCoroutine(OpenDoor());
        }
    }

    private IEnumerator OpenDoor()
    {
        float timeStart = Time.time;
        while (Time.time - timeStart <= doorTime)
        {
            transform.position = Vector3.Lerp(doorStart, doorTarget.position, Mathf.SmoothStep(0, 1, (Time.time - timeStart) / doorTime));
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator CloseDoor()
    {
        float timeStart = Time.time;
        while (Time.time - timeStart <= doorTime)
        {
            transform.position = Vector3.Lerp(doorTarget.position, doorStart, Mathf.SmoothStep(0, 1, (Time.time - timeStart) / doorTime));
            yield return new WaitForFixedUpdate();
        }
    }
}
