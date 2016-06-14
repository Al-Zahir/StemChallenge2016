using UnityEngine;
using System.Collections;

public class LastPlayerSighting : MonoBehaviour {

	public Vector3 position = new Vector3(1000f, 1000f, 1000f);
	public Vector3 resetPosition = new Vector3(1000f, 1000f, 1000f);
	public float musicFadeSpeed = 1f;

	private AudioSource primaryMusic;
	private AudioSource secondaryMusic;

	void Awake(){

		primaryMusic = GetComponent<AudioSource> ();
		secondaryMusic = transform.FindChild ("secondaryMusic").GetComponent<AudioSource>();

	}

	void Update(){

		MusicFading ();

	}

	void MusicFading(){

		if (false) { //add a condition for when the music changes to secondary
		
			primaryMusic.volume = Mathf.Lerp(primaryMusic.volume, 0f, musicFadeSpeed * Time.deltaTime);
			secondaryMusic.volume = Mathf.Lerp(secondaryMusic.volume, 0.8f, musicFadeSpeed * Time.deltaTime);
		
		} else {
		
			primaryMusic.volume = Mathf.Lerp(primaryMusic.volume, 0.8f, musicFadeSpeed * Time.deltaTime);
			secondaryMusic.volume = Mathf.Lerp(secondaryMusic.volume, 0f, musicFadeSpeed * Time.deltaTime);
		
		}

	}

}
