using UnityEngine;
using System.Collections;

public class FuelCellTaker : MonoBehaviour {

    public Transform[] displayStarts;
    public GameObject fuelCell;
    public float offsetX, offsetY;
    public int rows = 2, cols = 2;
    public int numCells = 0;
    public int numHolders = 2;
    private GameController gameController;
    public FinalTempleDoor door;

	// Use this for initialization
	void Start () {
        UpdateView();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}

    public void UpdateView()
    {
        foreach (Transform t in transform)
            Destroy(t.gameObject);

        for(int i = 0; i < numCells; i++)
            Instantiate(fuelCell, displayStarts[i / (rows * cols)].TransformPoint(new Vector3(offsetX * (i % (rows * cols) % cols), offsetY * (i % (rows * cols) / cols), 0)), displayStarts[i / (rows * cols)].rotation);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "PlayerBody")
        {
            numCells += gameController.numCellsOnPlayer;
            if (numCells > 8)
                numCells = 8;

            if (numCells == 8)
                door.allowOpen = true;

            gameController.EmptyFuel();
            UpdateView();
        }
    }
}
