using UnityEngine;
using System.Collections;

public class FinalPuzzleArrow : MonoBehaviour {

    public FinalPuzzlePlate plate;

    void OnTriggerEnter(Collider col)
    {
        if (col.name.ToLower().Contains("arrow"))
        {
            plate.OnArrowHit();
        }
    }
}
