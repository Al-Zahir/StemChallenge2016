using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class TreeExtender : MonoBehaviour
{

    private Terrain thisTerrain;
    public GameObject[] extraPrefabs;
    public string[] treeNames;
    private int lastTreeCount;
    private Dictionary<string, int> nameIndex;

    // Use this for initialization
    void Start()
    {
        thisTerrain = GetComponent<Terrain>();
        if (!Application.isPlaying)
        {
            UpdateIndex();
            ApplyExtras();
        }
        /*
        thisTerrain = GetComponent<Terrain>();
        TreeInstance[] trees = thisTerrain.terrainData.treeInstances;
        List<TreeInstance> treeInstances = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treeInstances);
        foreach(TreeInstance tree in trees)
        {
            // Remove the tree from the terrain tree list
            treeInstances.Remove(tree);
            thisTerrain.terrainData.treeInstances = treeInstances.ToArray();
        }

        // Now refresh the terrain, getting rid of the darn collider
        float[,] heights = thisTerrain.terrainData.GetHeights(0, 0, 1, 1);
        thisTerrain.terrainData.SetHeights(0, 0, heights);
         */
    }

    public void ApplyExtras()
    {
        if (thisTerrain == null)
            return;

        TreeInstance[] trees = thisTerrain.terrainData.treeInstances;
        GameObject extrasHolder = GameObject.Find(name + "-TreeExtras");
        if (extrasHolder != null)
            DestroyImmediate(extrasHolder);
        extrasHolder = new GameObject(name + "-TreeExtras");

        foreach (TreeInstance tree in trees)
        {
            int index;
            if (!nameIndex.TryGetValue(thisTerrain.terrainData.treePrototypes[tree.prototypeIndex].prefab.name, out index))
                continue;

            Vector3 thisTreePos = Vector3.Scale(tree.position, thisTerrain.terrainData.size) + thisTerrain.transform.position;
            GameObject prefab = (GameObject)Instantiate(extraPrefabs[index], thisTreePos, Quaternion.AngleAxis(tree.rotation * 180 / (Mathf.PI), Vector3.up));
            prefab.transform.localScale = Vector3.Scale(prefab.transform.localScale, new Vector3(1, tree.heightScale, 1));
            prefab.transform.parent = extrasHolder.transform;
        }
    }

    public void UpdateIndex()
    {
        if (nameIndex == null)
            nameIndex = new Dictionary<string, int>();
        else
            nameIndex.Clear();

        for (int i = 0; i < treeNames.Length; i++)
            try { nameIndex.Add(treeNames[i], i); }
            catch (Exception ex) { }
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            int curCount = thisTerrain.terrainData.treeInstanceCount;
            if (lastTreeCount != curCount)
            {
                UpdateIndex();
                ApplyExtras();
                lastTreeCount = curCount;
            }
        }
    }
}