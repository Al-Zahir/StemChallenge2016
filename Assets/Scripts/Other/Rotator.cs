using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public class Rotator : MonoBehaviour {
    public Transform sunRotate;
    public Transform sun, moon;
    public Transform lookTransform;
    public GameObject stars;
    public GameObject[] clouds;
    public GameObject snow;
    public GlobalFog fogScript;
    private bool fogOn;
    public float increment = 0.08f;
    public float sunset = 185;
    public float sunrise = 355;
    public float cloudFadeIn = 15;
    public float cloudFadeOut = 195;
    public float cloudFadeDuration = 5;
    public float snowFadeIn = 20;
    public float snowFadeOut = 200;
    public float starFadeIn = 200;
    public float starFadeOut = 10;
    private float originalIntensity, originalSet1, originalSet2;
    private Light sunLight, moonLight;
    private ParticleSystem starSystem;
    private ParticleSystem snowSystem;
    public float moonLightIntensity = 1;

    public Texture2D waterDay, waterNight;
    public Renderer water;
    public Color waterColorDay, waterColorNight;

    public float nightBillboardColor = 0.2f;
    public float dayBillboardColor = 1f;

    void Start()
    {
        sunLight = sun.GetComponent<Light>();
        moonLight = moon.GetComponent<Light>();
        starSystem = stars.GetComponent<ParticleSystem>();
        fogOn = fogScript != null && fogScript.enabled;
        //snowSystem = snow.GetComponent<ParticleSystem>();
        //cloudSystem = new CloudsToy[clouds.Length];
        //originalCloudOpacity = new float[clouds.Length];
        /*for (int i=0; i<cloudSystem.Length; i++)
        {
            cloudSystem[i] = clouds[i].GetComponent<CloudsToy>();
            originalCloudOpacity[i] = cloudSystem[i].CloudColor.a;
        }*/
        originalIntensity = sunLight.intensity;
        originalSet1 = RenderSettings.ambientIntensity;
        originalSet2 = RenderSettings.reflectionIntensity;
        moonLight.intensity = 0;

        water.sharedMaterial = new Material(water.sharedMaterial);
        water.sharedMaterial.SetTexture("_ReflectiveColor", waterDay);
        water.sharedMaterial.SetColor("_HorizonColor", waterColorDay);
        Shader.SetGlobalFloat("_BillboardCurrentColor", dayBillboardColor);
	}
	
	void FixedUpdate () {
        float previousAngle = GetAdjustedAngle();
        sunRotate.Rotate(increment*Time.deltaTime, 0, 0);
        sun.LookAt(lookTransform);
        float currentAngle = GetAdjustedAngle();
        if (currentAngle >= sunrise && previousAngle <= sunrise)
            OnSunrise();
        if (currentAngle >= sunset && previousAngle <= sunset)
            OnSunset();
        if (currentAngle >= cloudFadeIn && previousAngle <= cloudFadeIn + cloudFadeDuration)
            OnCloudFadeIn();
        if (currentAngle >= cloudFadeOut && previousAngle <= cloudFadeOut + cloudFadeDuration)
            OnCloudFadeOut();
        if (currentAngle >= snowFadeIn && previousAngle <= snowFadeIn)
            OnSnowFadeIn();
        if (currentAngle >= snowFadeOut && previousAngle <= snowFadeOut)
            OnSnowFadeOut();
        if (currentAngle >= starFadeIn && previousAngle <= starFadeIn)
            OnStarFadeIn();
        if (currentAngle >= starFadeOut && previousAngle <= starFadeOut)
            OnStarFadeOut();
	}

    private void OnSunrise()
    {
        sunLight.intensity = originalIntensity;
        moonLight.intensity = 0;
        RenderSettings.ambientIntensity = originalSet1;
        RenderSettings.reflectionIntensity = originalSet2;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        if (fogOn)
        {
            RenderSettings.fog = true;
            fogScript.enabled = true;
        }
        water.sharedMaterial.SetTexture("_ReflectiveColor", waterDay);
        water.sharedMaterial.SetColor("_HorizonColor", waterColorDay);
        Shader.SetGlobalFloat("_BillboardCurrentColor", dayBillboardColor);
    }

    private void OnSunset()
    {
        sunLight.intensity = 0;
        moonLight.intensity = moonLightIntensity;
        RenderSettings.ambientIntensity = 0;
        RenderSettings.reflectionIntensity = 0;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        if (fogOn)
        {
            RenderSettings.fog = false;
            fogScript.enabled = false;
        }
        water.sharedMaterial.SetTexture("_ReflectiveColor", waterNight);
        water.sharedMaterial.SetColor("_HorizonColor", waterColorNight);
        Shader.SetGlobalFloat("_BillboardCurrentColor", nightBillboardColor);
    }

    private void OnCloudFadeIn()
    {
        float step = (GetAdjustedAngle() - cloudFadeIn) / cloudFadeDuration;
        /*for (int i=0; i<cloudSystem.Length; i++)
            cloudSystem [i].CloudColor = new Color(cloudSystem [i].CloudColor.r,
                                                   cloudSystem [i].CloudColor.g,
                                                   cloudSystem [i].CloudColor.b,
                                                   originalCloudOpacity [i] * step);*/
    }

    private void OnCloudFadeOut()
    {
        float step = 1f - (GetAdjustedAngle() - cloudFadeOut) / cloudFadeDuration;
        /*for (int i=0; i<cloudSystem.Length; i++)
            cloudSystem [i].CloudColor = new Color(cloudSystem [i].CloudColor.r,
                                                  cloudSystem [i].CloudColor.g,
                                                  cloudSystem [i].CloudColor.b,
                                                  originalCloudOpacity [i] * step);*/
    }
    
    private void OnSnowFadeIn()
    {
        //snowSystem.enableEmission = true;
        //snowSystem.Play();
    }
    
    private void OnSnowFadeOut()
    {
        //snowSystem.enableEmission = false;
    }

    private void OnStarFadeIn()
    {
        starSystem.enableEmission = true;
        starSystem.Play();
    }
    
    private void OnStarFadeOut()
    {
        starSystem.enableEmission = false;
    }

    private float GetAdjustedAngle()
    {
        if (Mathf.Abs(sunRotate.eulerAngles.y - 180f) < 0.00001)
        {
            if(sunRotate.eulerAngles.x >= 0 && sunRotate.eulerAngles.x <= 90)
            {
                return 90 + 90 - sunRotate.eulerAngles.x;
            }
            else if(sunRotate.eulerAngles.x >= 270 && sunRotate.eulerAngles.x <= 360)
            {
                return 180 + 360 - sunRotate.eulerAngles.x;
            }
            else
            {
                return sunRotate.eulerAngles.x;
            }
        } 
        else
        {
            return sunRotate.eulerAngles.x;
        }
    }
}
