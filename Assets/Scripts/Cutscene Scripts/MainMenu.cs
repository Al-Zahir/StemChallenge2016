using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	public Canvas canvas;
	private Text[] text;

	public float minAlpha = 0.5f;
	public float maxAlpha = 1.0f;

	private float targetAlpha;
	private float startAlpha;
	private float startTime;

	public float smoothTime = 0.1f;

	public bool flyIn = false;
	private bool isLoaded = false;

	public ScreenFadeInOut fader;

	public AudioSource audio;

	void Awake(){

		text = canvas.GetComponentsInChildren<Text> ();

		flyIn = false;

		targetAlpha = 1.0f;
		startTime = Time.time;
		startAlpha = text[0].color.a;

	}

	void Update(){

		//foreach(Text t in text)
		text[0].color = new Color(text[0].color.r, text[0].color.g, text[0].color.b, Mathf.Lerp(startAlpha, targetAlpha, (Time.time - startTime)));

		if (text[0].color.a == maxAlpha) {
			targetAlpha = minAlpha;
			startTime = Time.time;
			startAlpha = text[0].color.a;
		} else if (text[0].color.a == minAlpha) {
			targetAlpha = maxAlpha;
			startTime = Time.time;
			startAlpha = text[0].color.a;
		}


		if (flyIn) {
		
			transform.Translate (transform.forward * 2 * Time.deltaTime);

			if(transform.position.z >= 0)
				audio.volume = 0.5f - 0.05f * transform.position.z;

			if (transform.position.z > 10 && !isLoaded)
				FlyInComplete ();
		}

	}

	public void StartGame(){

		foreach (Text t in text)
			t.gameObject.SetActive (false);
		//canvas.gameObject.SetActive (false);
		flyIn = true;

	}

	public void FlyInComplete(){
		
		fader.StartCoroutine (fader.EndScene(Application.loadedLevel + 1));
		isLoaded = true;

	}


}
