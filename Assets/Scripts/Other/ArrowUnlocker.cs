using UnityEngine;
using System.Collections;

public class ArrowUnlocker : MonoBehaviour {

    public StartTempleDoor target;
	
    void OnTriggerEnter(Collider col)
    {
        if (col.name.ToLower().Contains("arrow"))
        {
            target.allowOpen = true;
            StartCoroutine(target.OpenClose(true));
        }
    }
}
