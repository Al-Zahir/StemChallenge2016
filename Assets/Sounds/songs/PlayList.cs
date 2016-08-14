using UnityEngine;
using System.Collections;

public class PlayList : MonoBehaviour {

    public AudioClip[] music;
    private float startVolume;

	// Use this for initialization
    void Start()
    {
        startVolume = GetComponent<AudioSource>().volume;
        if (!GameObject.Find("GameController").GetComponent<GameController>().inStart)
        {
            PlayNextSong();
        }
        else
        {
            GetComponent<AudioSource>().Play();
            Invoke("PlayNextSong", GetComponent<AudioSource>().clip.length);
        }
	}

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            CancelInvoke();
            PlayNextSong();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            GetComponent<AudioSource>().volume = startVolume - GetComponent<AudioSource>().volume;
        }
    }

    void PlayNextSong()
    {
        if (!GameObject.Find("GameController").GetComponent<GameController>().inStart)
        {
            GetComponent<AudioSource>().clip = music[Random.Range(0, music.Length)];
            GetComponent<AudioSource>().Play();
            GetComponent<AudioSource>().loop = false;
        }

        Invoke("PlayNextSong", GetComponent<AudioSource>().clip.length);
    }
}
