using UnityEngine;
using System.Collections;

public class OnHitObject : MonoBehaviour {

    public GameObject parent;

    void OnTriggerEnter(Collider other)
    {
        object[] info = new object[2];
        info[0] = name;
        info[1] = other;
        parent.SendMessage("OnHit", info, SendMessageOptions.DontRequireReceiver);
    }

    void OnCollisionEnter(Collision other)
    {
        object[] info = new object[2];
        info[0] = name;
        info[1] = other;
        parent.SendMessage("OnHit2", info, SendMessageOptions.DontRequireReceiver);
    }
}
