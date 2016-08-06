using UnityEngine;
using System.Collections;
using System;

public class DroneHealth : MonoBehaviour {

    public float health = 100;
    public float hitLoss = 30;

    public string[] hurtObjectNames;
    public string[] criticalHits;
    public float criticalHitLoss = 100;

    public float hitDelay = 0.5f;
    private float lastHitTime;

    public bool playerMustSwing = true;
    public Transform player;

    void OnHit(object[] info)
    {
        string source = (string)info[0];
        Collider other = (Collider)info[1];

        if (Array.IndexOf(hurtObjectNames, other.transform.name) == -1 || Time.time < lastHitTime + hitDelay)
            return;

        if (playerMustSwing && other.transform.name.ToLower().Contains("sword") &&
            !Mecanim.inAnyAnim(player.GetComponent<Animator>(), player.GetComponent<PlayerBattleControl>().swordRootMotionAnimations, 0)) 
            return;

        if (other.transform.name.ToLower().Contains("arrow") && !other.transform.GetComponent<Arrow>().enabled)
            return;

        lastHitTime = Time.time;

        bool criticalHit = criticalHits != null && source != null && Array.IndexOf(criticalHits, source) != -1;
        health -= criticalHit ? criticalHitLoss : hitLoss;

        if(health <= 0)
        {
            GetComponent<Explode>().explode(0, criticalHit, criticalHit, criticalHit);
            Destroy(this);
        }
    }

    void OnHit2(object[] info)
    {
        object[] info2 = new object[2];
        info2[0] = info[0];
        info2[1] = ((Collision)info[1]).collider;
        OnHit(info2);
    }
}
