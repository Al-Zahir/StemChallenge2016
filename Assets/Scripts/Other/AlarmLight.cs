using UnityEngine;
using System.Collections;

public class AlarmLight : MonoBehaviour {

    public float period = 2;
    private Light thisLight;
    private float initialIntensity;

    void Start()
    {
        thisLight = GetComponent<Light>();
        initialIntensity = thisLight.intensity;
    }

	// Update is called once per frame
	void Update () {
        thisLight.intensity = Mathf.Lerp(0, initialIntensity, Mathf.SmoothStep(0, 1, Mathf.PingPong(Time.time, period) / period));
	}
}
