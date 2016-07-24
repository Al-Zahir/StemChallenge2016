using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public class TerrainTransparencySettings : MonoBehaviour
{
	public bool m_disableBasemap = true;
	public bool m_cutoutMode = true;
	public float m_cutoutModeHideAlpha = .5f;
	
	void Update()
	{
		if(m_disableBasemap && !Application.isPlaying) //Only reset on update in Editor
			GetComponent<Terrain>().basemapDistance = 1000000;
		if(m_cutoutMode)
		{
			Shader.SetGlobalFloat("_CutoutModeHideAlpha", m_cutoutModeHideAlpha);
			Shader.SetGlobalFloat("_CutoutModeHideAlpha2", m_cutoutModeHideAlpha);
		}
		else
		{
			Shader.SetGlobalFloat("_CutoutModeHideAlpha", 9f); //Nine signifies -1/null
			Shader.SetGlobalFloat("_CutoutModeHideAlpha2", -1f);
		}
	}
}