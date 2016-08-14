using UnityEngine;
using System.Collections;

public class ArrowUnlocker : MonoBehaviour {

    public StartTempleDoor target;
    private bool unlocked = false;

    void OnTriggerEnter(Collider col)
    {
        if (col.name.ToLower().Contains("arrow") && !unlocked)
        {
            target.allowOpen = true;
            StartCoroutine(target.OpenClose(true));
            unlocked = true;
        }
    }
}
