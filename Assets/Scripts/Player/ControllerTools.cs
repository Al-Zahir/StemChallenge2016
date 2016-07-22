using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ControllerTools
{
    // Cuts all triangles above [height] from the model.
    // Used to remove the head of the player.
    public static void RemoveTriangles(Mesh mesh, float height)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < triangles.Length / 3; i++)
        {
            int triIndex = i * 3;
            if (vertices[triangles[triIndex]].y > height)
            {
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = 0;
                triangles[triIndex + 2] = 0;
            }
        }

        mesh.triangles = triangles;
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    public static Vector3 RotateAround(Vector3 original, Vector3 center, Vector3 axis, float angle)
    {
        Vector3 pos = original;
        Quaternion rot = Quaternion.AngleAxis(angle, axis); // get the desired rotation
        Vector3 dir = pos - center; // find current direction relative to center
        dir = rot * dir; // rotate the direction
        original = center + dir; // define new position
        return original;
    }

    public static float[] GetTextureMix(Vector3 WorldPos, Terrain terrain, TerrainData terrainData)
    {
        Vector3 terrainPos = terrain.transform.position;
        // returns an array containing the relative mix of textures
        // on the main terrain at this world position.

        // The number of values in the array will equal the number
        // of textures added to the terrain.

        // calculate which splat map cell the worldPos falls within (ignoring y)
        int mapX = (int)(((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
        float[, ,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        // extract the 3D array data to a 1D array:
        float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

        for (int n = 0; n < cellMix.Length; n++)
        {
            cellMix[n] = splatmapData[0, 0, n];
        }
        return cellMix;
    }

    public static int GetMainTexture(Vector3 WorldPos, Terrain terrain, TerrainData terrainData)
    {
        // returns the zero-based index of the most dominant texture
        // on the main terrain at this world position.
        float[] mix = GetTextureMix(WorldPos, terrain, terrainData);

        float maxMix = 0;
        int maxIndex = 0;

        // loop through each mix value and find the maximum
        for (int n = 0; n < mix.Length; n++)
        {
            if (mix[n] > maxMix)
            {
                maxIndex = n;
                maxMix = mix[n];
            }
        }
        return maxIndex;
    }

}