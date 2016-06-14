using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ModelCleanerEditMode : MonoBehaviour
{

    List<Transform> temp = new List<Transform>();
    List<Transform> toDelete = new List<Transform>();

    void Start()
    {
        CollectLevel(transform);
        foreach (Transform t in temp)
            t.parent = transform;
        foreach (Transform t in toDelete)
            if(t != transform)
                DestroyImmediate(t.gameObject);
        DestroyImmediate(transform.GetComponent<ModelCleanerEditMode>());
    }

    void CollectLevel(Transform currentParent)
    {
        //Collect all top level gameobjects and add them to parent, erase empty ones
        for (int i = 0; i < currentParent.childCount; i++)
        {
            Transform t = currentParent.GetChild(i);
            if (t.GetComponents<Component>().Length > 1 && t.GetComponent<Animator>() == null || t.GetComponents<Component>().Length > 2) //Has more than a transform component
                temp.Add(t);
            else
                CollectLevel(t);
        }
        toDelete.Add(currentParent);
    }
}
