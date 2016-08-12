using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour {

    public bool inStart = true;
    public Transform startRespawn, gameRespawn, playerRespawn;
    public Rotator dayNight;
    private float originalRotSpeed;
    public int numCellsOnPlayer = 0;
    public Text fuelText;

	// Use this for initialization
	void Start () {
        originalRotSpeed = dayNight.increment;
	}
	
    void FixedUpdate()
    {
        playerRespawn.position = inStart ? startRespawn.position : gameRespawn.position;
        dayNight.increment = inStart ? 0 : originalRotSpeed;
    }

	// Update is called once per frame
	void Update () {
	
	}

    public void FinishedStartTemple()
    {
        inStart = false;
    }

    public void AddFuel()
    {
        numCellsOnPlayer++;
        UpdateFuelText();
    }

    public void EmptyFuel()
    {
        numCellsOnPlayer = 0;
        UpdateFuelText();
    }

    public void UpdateFuelText()
    {
        fuelText.text = "x" + numCellsOnPlayer;
    }
}
