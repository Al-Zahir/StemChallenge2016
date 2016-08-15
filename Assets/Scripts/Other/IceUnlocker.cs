using UnityEngine;
using System.Collections;

public class IceUnlocker : MonoBehaviour {

    public StartTempleDoor targetDoor;
    private bool opened = false;

    // Use this for initialization
    void OnTriggerEnter(Collider col)
    {
        if ((col.tag == "IceBlock" || col.tag == "PlayerBody") && !opened)
        {
            StartCoroutine(targetDoor.OpenClose(true));
            targetDoor.allowOpen = true;
            opened = true;
        }
    }

    // Use this for initialization
    void OnTriggerExit(Collider col)
    {
        if ((col.tag == "IceBlock" || col.tag == "PlayerBody") && opened)
        {
            StartCoroutine(targetDoor.OpenClose(false));
            targetDoor.allowOpen = false;
            opened = false;
        }
    }
}
