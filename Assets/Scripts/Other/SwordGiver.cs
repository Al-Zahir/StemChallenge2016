using UnityEngine;
using System.Collections;

public class SwordGiver : MonoBehaviour {

    public PlayerWeaponSelector playerWeaponSelector;
    public GameObject displaySword, displayShield;

	void OnTriggerEnter(Collider col)
    {
        if(col.tag == "PlayerBody")
        {
            playerWeaponSelector.swordAvail = true;
            Destroy(displaySword);
            Destroy(displayShield);
        }
    }

}
