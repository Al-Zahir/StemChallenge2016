using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameController : MonoBehaviour {

    public bool inStart = true, inEnd = false, playCrashScene = true;
    public Transform startRespawn, endRespawn, gameRespawn, playerRespawn;
    public Rotator dayNight;
    private float originalRotSpeed;
    public int numCellsOnPlayer = 0;
    public Text fuelText;
    private GameObject player;
    public AudioClip[] doorSounds;
    public AudioSource music;
    public AudioClip cameraPanMusic;
    public float cameraPanMusicStartTime = 46;
    public FuelCellAbsorb[] fuelCells;
    private int currentCellPtr;
    public FuelCellTaker finalTempleTaker;
    public Image fuelCellArrow;
    public GameObject finalCameraPan, finalWasp;
    public Transform vortexPos;
    public Text overlayText, credits;
    public float creditsWaitTime = 8;
    public AudioClip scream;

    public GameObject playerRagdoll, endSceneWolf;

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

        fuelCells = GameObject.FindObjectsOfType<FuelCellAbsorb>().OrderBy(x => int.Parse(x.name)).ToArray();

        if (inStart && playCrashScene)
            StartCoroutine(CrashScene());
	}

    public void Update()
    {
        bool found = true;

        if(fuelCells[currentCellPtr] == null)
        {
            found = false;

            for(int i=0; i<fuelCells.Length; i++)
            {
                if(fuelCells[i] != null)
                {
                    found = true;
                    currentCellPtr = i;
                    break;
                }
            }
        }

        Vector3 pointLoc;

        if (!found || finalTempleTaker.numCells + numCellsOnPlayer >= 8)
        {
            pointLoc = finalTempleTaker.transform.position;
        }
        else
            pointLoc = fuelCells[currentCellPtr].transform.position;

        Vector3 camDir = Camera.main.transform.forward;
        Vector3 fuelCellDir = pointLoc - player.transform.position;
        fuelCellDir.y = 0;
        camDir.y = 0;

        float angle = Vector3.Angle(camDir, fuelCellDir);
        Vector3 cross = Vector3.Cross(camDir, fuelCellDir);
        if (cross.y < 0) angle = -angle;

        fuelCellArrow.transform.rotation = Quaternion.Euler(0, 0, -90 - angle);
    }

    public void FinishedStartTemple()
    {
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
        StartCoroutine(Fly2());
        //Debug.Log("Finished end");
    }

    private IEnumerator CrashScene()
    {
        float volume = music.volume;
        music.volume = 0;
        player.GetComponent<PlayerMovement>().isDisabledByCutscene = true;
        Camera.main.GetComponent<CameraScript>().enabled = false;
        yield return new WaitForSeconds(1f);
        AudioSource.PlayClipAtPoint(scream, Camera.main.transform.position, 0.02f);
        yield return new WaitForSeconds(1);
        playerRagdoll.SetActive(true);
        playerRagdoll.transform.Find("mixamorig:Hips").GetComponent<Rigidbody>().velocity = playerRagdoll.transform.up * 100;
        yield return new WaitForSeconds(2);
        music.volume = volume;
        Camera.main.GetComponent<CameraScript>().enabled = true;
        player.GetComponent<PlayerMovement>().isDisabledByCutscene = false;
        Destroy(playerRagdoll);
    }

    private IEnumerator Fly()
    {
        float volume = music.volume;
        music.volume = 0.5f;
        music.clip = cameraPanMusic;
        music.Play();
        music.time = cameraPanMusicStartTime;
        player.GetComponent<PlayerMovement>().isDisabledByCutscene = true;
        Camera.main.GetComponent<CameraScript>().enabled = false;
        Transform first = Camera.main.GetComponent<SplineController>().SplineRoot.transform.Find("0");
        first.position = Camera.main.transform.position;
        first.rotation = Camera.main.transform.rotation;
        Camera.main.GetComponent<SplineController>().enabled = true;
        Camera.main.GetComponent<SplineInterpolator>().enabled = true;
        Camera.main.GetComponent<SplineController>().Start();
        Camera.main.GetComponent<SplineController>().FollowSpline();
        yield return new WaitForSeconds(52);
        Camera.main.GetComponent<CameraScript>().enabled = true;
        Camera.main.GetComponent<SplineController>().enabled = false;
        Camera.main.GetComponent<SplineInterpolator>().enabled = false;
        player.GetComponent<PlayerMovement>().isDisabledByCutscene = false;
        inStart = false;
        music.volume = volume;
        music.Stop();
    }

    private IEnumerator Fly2()
    {
        float volume = music.volume;
        music.volume = 0.5f;
        music.clip = cameraPanMusic;
        music.Play();
        music.time = 4 * 60 + 11;
        player.GetComponent<PlayerMovement>().isDisabledByCutscene = true;
        Camera.main.GetComponent<CameraScript>().enabled = false;
        Transform first = Camera.main.GetComponent<SplineController>().SplineRoot.transform.Find("0");
        first.position = Camera.main.transform.position;
        first.rotation = Camera.main.transform.rotation;
        first.position = Camera.main.transform.position;
        player.GetComponent<PlayerHealth>().sceneColorManager.SetColor(Color.black);
        yield return new WaitForSeconds(3);
        Camera.main.transform.position = vortexPos.position - vortexPos.forward * 30;
        Camera.main.transform.LookAt(vortexPos.position + vortexPos.up * 10);
        player.GetComponent<PlayerHealth>().sceneColorManager.SetColor(Color.clear);
        yield return new WaitForSeconds(3);
        vortexPos.GetComponent<DroneVortex>().BlowUp();
        yield return new WaitForSeconds(3);
        player.GetComponent<PlayerHealth>().sceneColorManager.SetColor(Color.black);
        yield return new WaitForSeconds(2);
        overlayText.text = "4 days of rebuilding later...";
        overlayText.gameObject.SetActive(true);
        yield return new WaitForSeconds(4);
        overlayText.gameObject.SetActive(false);
        player.GetComponent<PlayerHealth>().sceneColorManager.SetColor(Color.clear);
        Camera.main.transform.position = endSceneWolf.transform.position + endSceneWolf.transform.right * -3f + Vector3.up * 2f;
        Camera.main.transform.LookAt(endSceneWolf.transform);
        endSceneWolf.SetActive(true);
        yield return new WaitForSeconds(4);
        player.GetComponent<PlayerHealth>().sceneColorManager.SetColor(Color.black);
        yield return new WaitForSeconds(2);
        Camera.main.GetComponent<SplineController>().SplineRoot = finalCameraPan;
        Camera.main.GetComponent<SplineController>().Duration = 24;
        Camera.main.GetComponent<SplineController>().enabled = true;
        Camera.main.GetComponent<SplineInterpolator>().enabled = true;
        Camera.main.GetComponent<SplineController>().Start();
        Camera.main.GetComponent<SplineController>().FollowSpline();
        finalWasp.SetActive(true);
        yield return new WaitForSeconds(1);
        player.GetComponent<PlayerHealth>().sceneColorManager.SetColor(Color.clear);
        yield return new WaitForSeconds(11);
        finalWasp.GetComponent<SplineController>().FollowSpline();
        yield return new WaitForSeconds(24 - 12 - 7);
        player.GetComponent<PlayerHealth>().sceneColorManager.SetColor(Color.black);
        yield return new WaitForSeconds(3);
        credits.gameObject.SetActive(true);
        yield return new WaitForSeconds(creditsWaitTime);
        Application.LoadLevelAsync(0);
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
