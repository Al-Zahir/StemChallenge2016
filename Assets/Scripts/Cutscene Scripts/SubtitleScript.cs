using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SubtitleScript : MonoBehaviour {

	public string[] lines;
	public float[] times;

	private Text text;
	private int index;

	void Awake(){

		text = GetComponent<Text> ();
		index = 0;

		StartCoroutine (NextLine());

	}

	IEnumerator NextLine(){

		if (index < lines.Length) {
		
			yield return new WaitForSeconds (times[index]);
			text.text = lines [index];
			index++;
			StartCoroutine (NextLine());
		
		}

	}

}
