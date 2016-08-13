﻿using UnityEngine;
using System.Collections;

public class Explode : MonoBehaviour {

    private GameObject explosion, explosionBlue;
    public GameObject[] disableParts, extraParts;

    private bool changeColor;

	// Use this for initialization
    void Start()
    {
        Transform temp = transform.FindChild("Explosion");
        if (temp != null)
            explosion = temp.gameObject;
        temp = transform.FindChild("ExplosionBlue");
        if (temp != null)
            explosionBlue = temp.gameObject;
	}
	
	public void explode(float timeToFallApart, bool changeColor, bool showExplosion, bool blue)
    {
        if (explosion != null && !blue)
        {
            explosion.SetActive(showExplosion);
            explosion.transform.parent = null;
            Destroy(explosion, 10);
        }
        else if (explosionBlue != null && blue)
        {
            explosionBlue.SetActive(showExplosion);
            explosionBlue.transform.parent = null;
            Destroy(explosionBlue, 10);
        }

        StartCoroutine(FallApart(timeToFallApart, changeColor));
        SendMessage("Explosion", SendMessageOptions.DontRequireReceiver);
    }

    private IEnumerator FallApart(float timeToFallApart, bool changeColor)
    {
        this.changeColor = changeColor;

        if(timeToFallApart != 0)
            yield return new WaitForSeconds(timeToFallApart);

        foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
            HandleExplosion(m);

        foreach (GameObject g in extraParts)
        {
            MeshRenderer renderer = g.GetComponent<MeshRenderer>();

            if (renderer != null)
                HandleExplosion(renderer);
        }

        foreach (GameObject g in disableParts)
            g.SetActive(false);

        Destroy(this);
    }

    private void HandleExplosion(MeshRenderer m)
    {
        if (m.GetComponent<Rigidbody>() == null)
            m.gameObject.AddComponent<Rigidbody>();

        if (m.GetComponent<HingeJoint>())
            Destroy(m.GetComponent<HingeJoint>());

        if (m.GetComponent<MeshCollider>() != null)
            m.GetComponent<MeshCollider>().convex = true;

        if (changeColor)
        {
            Material[] mats = m.materials;
            foreach (Material mat in mats)
            {
                mat.SetFloat("_Metallic", 1);
                mat.SetFloat("_Glossiness", 1);
            }
        }
    }
}
