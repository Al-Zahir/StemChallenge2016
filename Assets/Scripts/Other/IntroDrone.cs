using UnityEngine;
using System.Collections;

public class IntroDrone : MonoBehaviour {

    public StartTempleDoor targetDoor;

    void Explosion()
    {
        StartCoroutine(targetDoor.OpenClose(true));
    }
}
