using UnityEngine;
using System.Collections;

public class Respawner : MonoBehaviour {

    public Transform respawnLocation;
    public Transform player;

    void Start()
    {
        if (player == null) 
            player = GameObject.Find("Player").transform;
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "PlayerBody")
        {
            player.position = respawnLocation.position;
        }
    }
}
