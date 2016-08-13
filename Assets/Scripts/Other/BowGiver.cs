using UnityEngine;
using System.Collections;

public class BowGiver : MonoBehaviour {

    public PlayerWeaponSelector playerWeaponSelector;
    public GameObject displayBow;

	void OnTriggerEnter(Collider col)
    {
        if(col.tag == "PlayerBody")
        {
            playerWeaponSelector.bowAvail = true;
            Destroy(displayBow);
        }
    }

}
