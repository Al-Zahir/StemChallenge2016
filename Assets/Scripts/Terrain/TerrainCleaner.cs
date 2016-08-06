using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainCleaner : MonoBehaviour
{
    
    public Terrain[] inputTerrains;
    
    /*Add terrains in rows starting at the left, with the "Top" view in Unity.
    Example:
    ------------------
    |   0    |    1  |
    |        |       |
    |----------------|
    |        |       |
    |   2    |    3  |
    ------------------
    */
    
    //Specify num of rows and columns
    public int rows, columns;
    private Terrain[,] terrains;
    public bool reset = false; //Toggle to undo the terrain data
    public bool cleanEdge = false;
    
    private List<float[,]> originalHeightMaps = new List<float[,]>();
    
    // Use this for initialization
    void Start()
    {
        terrains = new Terrain[rows, columns];
        for (int i=0; i<rows; i++)
        {
            for (int j=0; j<columns; j++)
            {
                int index = i * columns + j;
                terrains [i, j] = index < inputTerrains.Length ? inputTerrains [index] : null;
                originalHeightMaps.Add(GetHeightMap(inputTerrains [index]));
            }
        }
        
        if(cleanEdge)
            CleanEdge();
    }

    private void CleanEdge()
    {
        for (int i = 0; i < terrains.GetLength(0); i++)
        {
            for (int j = 0; j < terrains.GetLength(1); j++)
            {
                Terrain current = terrains[i, j];
                float[,] currentHeightMap = GetHeightMap(current);

                int k = 0;
                for (int l = 0; l < currentHeightMap.GetLength(1); l++)
                    currentHeightMap[k, l] = 0;
                k = currentHeightMap.GetLength(0)-1;
                for (int l = 0; l < currentHeightMap.GetLength(1); l++)
                    currentHeightMap[k, l] = 0;
                k = 0;
                for (int l = 0; l < currentHeightMap.GetLength(0); l++)
                    currentHeightMap[l, k] = 0; k = 0;
                k = currentHeightMap.GetLength(1)-1;
                for (int l = 0; l < currentHeightMap.GetLength(0); l++)
                    currentHeightMap[l, k] = 0;

                SetHeightMap(current, currentHeightMap);
                current.Flush();
            }
        }
    }
    
    private void Update()
    {
        if (reset)
        {
            for (int i=0; i<rows; i++)
            {
                for (int j=0; j<columns; j++)
                {
                    int index = i * columns + j;
                    terrains [i, j] = index < inputTerrains.Length ? inputTerrains [index] : null;
                }
            }
            ResetTerrains();
            reset = !reset;
        }
    }
    
    private void ResetTerrains()
    {
        for (int i=0; i<inputTerrains.Length; i++)
        {
            SetHeightMap(inputTerrains[i], originalHeightMaps[i]);
        }
    }
    
    private float[,] GetHeightMap(Terrain input)
    {
        return input.terrainData.GetHeights(0, 0, input.terrainData.heightmapWidth, input.terrainData.heightmapHeight);
        
    }
    
    private void SetHeightMap(Terrain input, float[,] heightMap)
    {
        input.terrainData.SetHeights(0, 0, heightMap);
    }
}