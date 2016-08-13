using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour {

    public bool inStart = true, inEnd = false;
    public Transform startRespawn, endRespawn, gameRespawn, playerRespawn;
    public Rotator dayNight;
    private float originalRotSpeed;
    public int numCellsOnPlayer = 0;
    public Text fuelText;
    private GameObject player;

	// Use this for initialization
	void Start () {
        originalRotSpeed = dayNight.increment;
        if (inStart || inEnd)
        {
            GetComponent<WolfSpawner>().enabled = false;
            dayNight.increment = 0;
            playerRespawn.position = inStart ? startRespawn.position : endRespawn.position;
        }
        else
        {
            GetComponent<WolfSpawner>().enabled = true;
            dayNight.increment = originalRotSpeed;
            playerRespawn.position = gameRespawn.position;
        }
        player = GameObject.Find("Player");
	}

    public void FinishedStartTemple()
    {
        inStart = false;
        GetComponent<WolfSpawner>().enabled = true;
        dayNight.increment = originalRotSpeed;
        playerRespawn.position = gameRespawn.position;
        StartCoroutine(Fly());
    }

    public void StartEndTemple()
    {
        inEnd = true;
        GetComponent<WolfSpawner>().enabled = false;
        dayNight.OnSunrise();
        dayNight.increment = 0;
        dayNight.transform.rotation = Quaternion.EulerAngles(40, 0, 0);
        playerRespawn.position = endRespawn.position;
    }

    public void FinishedEndTemple()
    {
        inEnd = false;
        GetComponent<WolfSpawner>().enabled = true;
        dayNight.increment = originalRotSpeed;
        playerRespawn.position = gameRespawn.position;
        Debug.Log("Finished end");
    }

    private IEnumerator Fly()
    {
        Camera.main.GetComponent<CameraScript>().enabled = false;
        Transform first = Camera.main.GetComponent<SplineController>().SplineRoot.transform.Find("0");
        first.position = Camera.main.transform.position;
        first.rotation = Camera.main.transform.rotation;
        Camera.main.GetComponent<SplineController>().enabled = true;
        Camera.main.GetComponent<SplineInterpolator>().enabled = true;
        yield return new WaitForSeconds(52);
        Camera.main.GetComponent<CameraScript>().enabled = true;
        Camera.main.GetComponent<SplineController>().enabled = false;
        Camera.main.GetComponent<SplineInterpolator>().enabled = false;
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
