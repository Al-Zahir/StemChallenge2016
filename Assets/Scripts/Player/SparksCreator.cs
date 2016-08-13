using UnityEngine;
using System.Collections;

public class SparksCreator : MonoBehaviour {

    public Transform sparkLocation;
    public GameObject spark;

    void HitEnemySword()
    {
        Instantiate(spark, sparkLocation.position, sparkLocation.rotation);
    }
}
