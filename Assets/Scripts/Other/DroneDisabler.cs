using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DroneDisabler : MonoBehaviour {

    public GameObject[] drones;
    public bool disableEntirely = false;

	// Use this for initialization
    void Start()
    {
        ShockDrone[] d1 = FindObjectsOfType<ShockDrone>();
        MineDrone[] d2 = FindObjectsOfType<MineDrone>();
        ReconDrone[] d3 = FindObjectsOfType<ReconDrone>();

        List<GameObject> droneList = new List<GameObject>();

        foreach (ShockDrone d in d1)
            if (GetComponent<BoxCollider>().bounds.Contains(d.transform.position))
                droneList.Add(d.gameObject);

        foreach (MineDrone d in d2)
            if (GetComponent<BoxCollider>().bounds.Contains(d.transform.position))
                droneList.Add(d.gameObject);

        foreach (ReconDrone d in d3)
            if (GetComponent<BoxCollider>().bounds.Contains(d.transform.position))
                droneList.Add(d.gameObject);

        drones = droneList.ToArray();

        SetDrones(GetComponent<BoxCollider>().bounds.Contains(GameObject.Find("Player").transform.position));
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "PlayerBody")
        {
            SetDrones(true);
        }
    }


    void OnTriggerExit(Collider col)
    {
        if (col.tag == "PlayerBody")
        {
            SetDrones(false);
        }
    }

    public void SetDrones(bool active)
    {
        foreach (GameObject g in drones)
            if (g != null && g.GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
            {
                if (disableEntirely)
                    g.SetActive(active);
                else
                    g.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = active;
            }
    }
}
