using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScreenColor : MonoBehaviour {
		
    public float fadeLength = 2;

    public bool isSceneColor = false;
	private Image image;

    private float actionTime;
    private IEnumerator currentLerp;

	void Start(){
		image = GetComponent<Image> ();
        image.enabled = true;

        if (isSceneColor)
            SetColor(Color.clear);
        else
            image.color = Color.clear;
    }

    public void SetColor(Color newColor)
    {
        if (currentLerp != null)
            StopCoroutine(currentLerp);

        currentLerp = LerpToColor(newColor);
        StartCoroutine(currentLerp);
    }

    public void SetColor(Color newColor, float length)
    {
        fadeLength = length;
        SetColor(newColor);
    }

    public IEnumerator LerpToColor(Color newColor)
    {
        actionTime = Time.time;
        Color startColor = image.color;
        while (Time.time < actionTime + fadeLength)
        {
            image.color = Color.Lerp(startColor, newColor, (Time.time - actionTime) / fadeLength);
            yield return new WaitForEndOfFrame();
        }

        image.color = newColor;

        currentLerp = null;
	}
	
}
