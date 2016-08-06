using UnityEngine;
using System.Collections;

public class TempleEntrance : MonoBehaviour {

    public GameObject templeDoor;

	void OnTriggerEnter(Collider col)
    {
        templeDoor.GetComponent<TempleDoor>().RequestOpen();
    }
}
