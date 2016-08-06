using UnityEngine;
using System.Collections;

public class TriggerNotifier : MonoBehaviour {

    public GameObject parent;

    void OnTriggerEnter(Collider other)
    {
        parent.SendMessage(name + "TriggerEnter", other);
    }

    void OnTriggerExit(Collider other)
    {
        parent.SendMessage(name + "TriggerExit", other);
    }
}
