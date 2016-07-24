using UnityEngine;
using System.Collections;

public class SwapTexture : MonoBehaviour
{

    public int texture1, texture2;

    // Use this for initialization
    void Start()
    {
        flipTextures();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            flipTextures();
    }

    public void flipTextures()
    {
        TerrainData terrainData = GetComponent<Terrain>().terrainData;
        //get current paint mask
        float[,,] alphas = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        // make sure every grid on the terrain is modified
        for (int i = 0; i < terrainData.alphamapWidth; i++)
        {
            for (int j = 0; j < terrainData.alphamapHeight; j++)
            {
                float tempAlpha = alphas[i, j, texture1];
                alphas[i, j, texture1] = alphas[i, j, texture2];
                alphas[i, j, texture2] = tempAlpha;
            }
        }
        // apply the new alpha
        terrainData.SetAlphamaps(0, 0, alphas);
    }
}
