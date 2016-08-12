using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FallThroughColliders
{	
	InThisObject,
	InThisObjectAndChildren,
	NoneOrManual
}
public class ObjectFallThrough : MonoBehaviour
{
	public GameObject[] m_terrainObjects;
	public bool m_autoAddTerrainObjects = true;
	public float m_fallThroughOpacity = .75f;
	public FallThroughColliders m_fallThroughColliders = FallThroughColliders.InThisObjectAndChildren;
	public Collider[] m_extraFallThroughColliders;
	public bool m_snapUpOnTerrainContact = true;
	public float m_snapUpDistanceOffset = 1;
	public float m_maxSnapUpDistance = 2;
    public NoTerrainZone currentNoTerrainZone;
	
	private List<GameObject> m_terrainObjectList = new List<GameObject>();
	private List<TerrainData> m_terrainDataList = new List<TerrainData>();
    private List<float[,,]> m_terrainAlphamapsList = new List<float[,,]>();
	private List<bool> m_fallenThroughList = new List<bool>();
	
	void Start()
	{
		foreach(GameObject terrainObject in m_terrainObjects)
			AddTerrainObjectToList(terrainObject);
	}
	
	void OnControllerColliderHit(ControllerColliderHit col) //If using a character controller
	{
		if(m_autoAddTerrainObjects && col.collider is TerrainCollider && !m_terrainObjectList.Contains(col.collider.gameObject))
			AddTerrainObjectToList(col.collider.gameObject);
	}

    void OnCollisionEnter(Collision col) //If using a regular collider
    {
        if (m_autoAddTerrainObjects && col.collider is TerrainCollider && !m_terrainObjectList.Contains(col.collider.gameObject))
            AddTerrainObjectToList(col.collider.gameObject);
    }

    void OnTriggerEnter(Collider col)
    {
        NoTerrainZone colTerrainZone = col.GetComponent<NoTerrainZone>();
        if (colTerrainZone != null)
        {
            currentNoTerrainZone = colTerrainZone;
            GameObject[] terrains = currentNoTerrainZone.interaction;
            foreach (GameObject terrain in terrains)
                UpdateCollision(true, terrain);
        }
    }

    void OnTriggerExit(Collider col)
    {
        NoTerrainZone colTerrainZone = col.GetComponent<NoTerrainZone>();
        if (currentNoTerrainZone != null)
        {
            GameObject[] terrains = currentNoTerrainZone.interaction;
            foreach (GameObject terrain in terrains)
                UpdateCollision(false, terrain);

            currentNoTerrainZone = null;
        }
    }
	
	void AddTerrainObjectToList(GameObject terObj)
	{
		m_terrainObjectList.Add(terObj);
		
		TerrainData terrainData = terObj.GetComponent<Terrain>().terrainData; //Only grab once
		
		m_terrainDataList.Add(terrainData);		
		m_terrainAlphamapsList.Add(terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapResolution));
		m_fallenThroughList.Add(false);
	}
	
	void FixedUpdate()
	{
		for(int i = 0;i < m_terrainObjectList.Count;i++)
		{
			GameObject terrainObject = m_terrainObjectList[i];
			if(terrainObject == null) //Must have been destroyed by another script
				continue;
			TerrainData terrainData = m_terrainDataList[i];
			float[,,] terrainAlphamaps = m_terrainAlphamapsList[i];
			bool fallenThrough = m_fallenThroughList[i];
			
			float opacity = GetOpacityAt(transform.position - terrainObject.transform.position, terrainData, terrainAlphamaps, terrainObject.GetComponent<TerrainTransparencySettings>()); //Use position relative to terrain origin
			bool fallThrough = opacity <= m_fallThroughOpacity;
			
			if(m_snapUpOnTerrainContact && fallThrough != fallenThrough && !fallThrough) //If snapping is enabled, and fall-through status is going to change to above-ground
			{
				float dif = (terrainObject.GetComponent<Terrain>().SampleHeight(transform.position) - transform.position.y) + m_snapUpDistanceOffset;	
				if(dif > 0) //If ground is above us (we don't want to 'waste' our 'snap back up' when terrain's actually under us)
				{
					if(dif < m_maxSnapUpDistance) //If snap distance is within max
						transform.Translate(new Vector3(0, dif, 0)); //Snap up
					else
						fallThrough = fallenThrough; //Cancel changing of fall-through status to above-ground
				}
			}
			
			if(fallThrough != fallenThrough) //If fall-through status is about to change
			{
                m_fallenThroughList[i] = fallThrough;
                UpdateCollision(fallThrough, terrainObject);			
			}
		}
	}

    private void UpdateCollision(bool fallThrough, GameObject terrainObject)
    {
        if (m_fallThroughColliders == FallThroughColliders.InThisObject)
        {
            foreach (Collider tCol in terrainObject.GetComponents<Collider>())
            {
                foreach (Collider col in GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(tCol, col, fallThrough);
                foreach (Collider col in m_extraFallThroughColliders)
                    Physics.IgnoreCollision(tCol, col, fallThrough);
            }
        }
        else if (m_fallThroughColliders == FallThroughColliders.InThisObjectAndChildren)
        {
            foreach (Collider tCol in terrainObject.GetComponents<Collider>())
            {
                foreach (Collider col in GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(tCol, col, fallThrough);
                foreach (Collider col in m_extraFallThroughColliders)
                    Physics.IgnoreCollision(tCol, col, fallThrough);
            }
        }
        else //The user is setting all object-to-fall-through colliders manually
        {
            foreach (Collider tCol in terrainObject.GetComponents<Collider>())
            {
                foreach (Collider col in m_extraFallThroughColliders)
                    Physics.IgnoreCollision(tCol, col, fallThrough);
            }
        }
    }

	private float GetOpacityAt(Vector3 position, TerrainData terrainData, float[,,] terrainAlphamaps, TerrainTransparencySettings terrainTransparencySettings)
	{
		for (int splatCount = 0; splatCount < terrainData.alphamapLayers; splatCount++)
	    {			
	        SplatPrototype terrainTexture = terrainData.splatPrototypes[splatCount];
			
			Vector2 currentLocationOnTerrainmap = new Vector2((terrainData.alphamapResolution / terrainData.size.x) * position.x, (terrainData.alphamapResolution / terrainData.size.z) * position.z);
	
		    int xPos = (int)currentLocationOnTerrainmap.y; //For some reason we need to flip them
		    int yPos = (int)currentLocationOnTerrainmap.x;
			
			if(xPos >= 0 && yPos >= 0 && splatCount >= 0 && terrainAlphamaps.GetLength(0) > xPos && terrainAlphamaps.GetLength(1) > yPos && terrainAlphamaps.GetLength(2) > splatCount) //If in-bounds
			{
				float textureStrength = terrainAlphamaps[xPos, yPos, splatCount];
				if(terrainTexture.texture.name == "Transparent")
				{
					float opacity = 1.0f - textureStrength;
					if(terrainTransparencySettings != null && terrainTransparencySettings.m_cutoutMode)
					{
						if(opacity > terrainTransparencySettings.m_cutoutModeHideAlpha)
							opacity = 1;
						else
							opacity = 0;
					}
					return opacity;
				}
			}
	    }
		return 1.0f; //If no texture detected underneath, assume we're on solid/opaque terrain
	}
}