using UnityEngine;
using System.Collections;

public class FuelCellAbsorb : MonoBehaviour {

    private GameController gameController;
    private bool consumed = false;

	// Use this for initialization
	void Start () {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "PlayerBody" && !consumed)
        {
            gameController.AddFuel();
            consumed = true;
            Destroy(gameObject);
        }
    }
}
