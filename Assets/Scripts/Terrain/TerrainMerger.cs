using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainMerger : MonoBehaviour
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
    public bool remerge = false; //Toggle to smooth again
    public bool onlyMergeTextures = false; //Enable to only combine textures using SetNeighbor
    
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
        
        Merge();
    }
    
    private void Merge()
    {
        if (!onlyMergeTextures)
        {
            for (int i=0; i<terrains.GetLength(0); i++)
            {
                for (int j=0; j<terrains.GetLength(1); j++)
                {
                    //For every cell, merge the bottom side, right side, and bottom-right corner if they exist
                    Terrain current = terrains [i, j];
                    float[,] currentHeightMap = GetHeightMap(current);
                    float[,] rightHeightMap = null;
                    float[,] bottomHeightMap = null;
                    if (HasRightNeighbor(i, j))
                    {
                        rightHeightMap = GetHeightMap(terrains [i, j + 1]);
                        for (int l=0; l<currentHeightMap.GetLength(0); l++) //Each row to merge
                        {
                            float currentHeight = currentHeightMap [l, currentHeightMap.GetLength(1) - 1];
                            float rightHeight = rightHeightMap [l, 0];
                            float average = (currentHeight + rightHeight) / 2F;
                            currentHeightMap [l, rightHeightMap.GetLength(1) - 1] = average;
                            rightHeightMap [l, 0] = average;
                        }
                        SetHeightMap(current, currentHeightMap);
                        SetHeightMap(terrains [i, j + 1], rightHeightMap);
                    }
                    if (HasBottomNeighbor(i, j))
                    {
                        bottomHeightMap = GetHeightMap(terrains [i + 1, j]);
                        for (int l=0; l<currentHeightMap.GetLength(1); l++) //Each column to merge
                        {
                            float currentHeight = currentHeightMap [0, l];
                            float bottomHeight = bottomHeightMap [currentHeightMap.GetLength(0) - 1, l];
                            float average = (currentHeight + bottomHeight) / 2F;
                            currentHeightMap [0, l] = average;
                            bottomHeightMap [currentHeightMap.GetLength(0) - 1, l] = average;
                        }
                        SetHeightMap(current, currentHeightMap);
                        SetHeightMap(terrains [i + 1, j], bottomHeightMap);
                    }
                    if (HasRightNeighbor(i, j) && HasBottomNeighbor(i, j))
                    {
                        //Corner is the average of 4 terrain points instead of 2
                        //If adjacent, guaranteed to have had a right and a bottom so no need to reassign heightmaps
                        float[,] adjacentHeightMap = GetHeightMap(terrains [i + 1, j + 1]);
                    
                        float bottomRightCorner = currentHeightMap [0, currentHeightMap.GetLength(1) - 1];
                        float bottomLeftCorner = rightHeightMap [0, 0];
                        float topRightCorner = bottomHeightMap [bottomHeightMap.GetLength(0) - 1, bottomHeightMap.GetLength(1) - 1];
                        float topLeftCorner = adjacentHeightMap [adjacentHeightMap.GetLength(0) - 1, 0];
                        float average = (bottomRightCorner + bottomLeftCorner + topRightCorner + topLeftCorner) / 4F;
                        currentHeightMap [0, currentHeightMap.GetLength(1) - 1] = average;
                        rightHeightMap [0, 0] = average;
                        bottomHeightMap [bottomHeightMap.GetLength(0) - 1, bottomHeightMap.GetLength(1) - 1] = average;
                        adjacentHeightMap [adjacentHeightMap.GetLength(0) - 1, 0] = average;
                        SetHeightMap(current, currentHeightMap);
                        SetHeightMap(terrains [i + 1, j], bottomHeightMap);
                        SetHeightMap(terrains [i, j + 1], rightHeightMap);
                        SetHeightMap(terrains [i + 1, j + 1], adjacentHeightMap);
                    }
                }
            }
        }

        for (int i=0; i<terrains.GetLength(0); i++)
        {
            for (int j=0; j<terrains.GetLength(1); j++)
            {

                Terrain current = terrains [i, j];
                Terrain left = HasLeftNeighbor(i, j) ? terrains [i, j - 1] : null;
                Terrain top = HasTopNeighbor(i, j) ? terrains [i - 1, j] : null;
                Terrain right = HasRightNeighbor(i, j) ? terrains [i, j + 1] : null;
                Terrain bottom = HasBottomNeighbor(i, j) ? terrains [i + 1, j] : null;
        
                current.SetNeighbors(left, top, right, bottom);
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
        
        if (remerge)
        {
            for (int i=0; i<rows; i++)
            {
                for (int j=0; j<columns; j++)
                {
                    int index = i * columns + j;
                    terrains [i, j] = index < inputTerrains.Length ? inputTerrains [index] : null;
                }
            }
            Merge();
            remerge = !remerge;
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
    
    private bool HasLeftNeighbor(int row, int col)
    {
        return col < terrains.GetLength(1) && col > 0 && terrains[row,col-1] != null;
    }
    
    private bool HasRightNeighbor(int row, int col)
    {
        return col < terrains.GetLength(1) - 1 && col >= 0 && terrains[row,col+1] != null;
    }
    
    private bool HasBottomNeighbor(int row, int col)
    {
        return row < terrains.GetLength(0) - 1 && row >= 0 && terrains[row+1,col] != null;
    }
    
    private bool HasTopNeighbor(int row, int col)
    {
        return row < terrains.GetLength(0) && row > 0 && terrains[row-1,col] != null;
    }
    
    private bool HasBottomRightAdjacent(int row, int col)
    {
        return row < terrains.GetLength(0) - 1 && row >= 0 && col < terrains.GetLength(1) - 1 && col >= 0 && terrains[row+1,col+1] != null;
    }
}