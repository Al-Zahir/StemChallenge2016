using UnityEngine;
using System.Collections;

public class PlayList : MonoBehaviour {

    public AudioClip[] music;
    private float startVolume;
    private bool pressedN = false;

	// Use this for initialization
    void Start()
    {
        startVolume = GetComponent<AudioSource>().volume;
        /*if (!GameObject.Find("GameController").GetComponent<GameController>().inStart)
        {
            PlayNextSong();
        }
        else
        {
            GetComponent<AudioSource>().Play();
            Invoke("PlayNextSong", GetComponent<AudioSource>().clip.length);
        }*/

        StartCoroutine(PlayNextSong());
	}

    void Update()
    {
        if (GameObject.Find("Player").GetComponent<PlayerMovement>().isDisabledByCutscene)
            return;

        if (Input.GetKeyDown(KeyCode.N) && !GameObject.Find("GameController").GetComponent<GameController>().inStart)
        {
            pressedN = true;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            GetComponent<AudioSource>().volume = startVolume - GetComponent<AudioSource>().volume;
        }
    }

    private IEnumerator PlayNextSong()
    {
        /*
        if (!GameObject.Find("GameController").GetComponent<GameController>().inStart)
        {
            GetComponent<AudioSource>().clip = music[Random.Range(0, music.Length)];
            GetComponent<AudioSource>().Play();
            GetComponent<AudioSource>().loop = false;
        }

        Invoke("PlayNextSong", GetComponent<AudioSource>().clip.length);
         */

        while (this)
        {
            if (!(GameObject.Find("GameController").GetComponent<GameController>().inStart || GameObject.Find("Player").GetComponent<PlayerMovement>().isDisabledByCutscene) && 
                (!GetComponent<AudioSource>().isPlaying || pressedN))
            {
                int id = Random.Range(0, music.Length);
                while (music[id] == GetComponent<AudioSource>().clip)
                    id = Random.Range(0, music.Length);

                GetComponent<AudioSource>().clip = music[id];
                GetComponent<AudioSource>().Play();
                GetComponent<AudioSource>().time = 0;
                GetComponent<AudioSource>().loop = false;
                pressedN = false;
            }

            yield return new WaitForSeconds(1);
        }
    }
}
