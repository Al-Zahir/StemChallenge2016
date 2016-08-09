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

    void Start()
    {
        if (player == null)
            player = GameObject.Find("Player").transform;
    }

    void OnHit(object[] info)
    {
        if (player == null)
            return;

        string source = (string)info[0];
        Collider other = (Collider)info[1];
        
        bool criticalHit = criticalHits != null && source != null && Array.IndexOf(criticalHits, source) != -1;

        // Arrows create multiple hit requests, if any are critical ignore the hitDelay
        if (!(other.transform.name.ToLower().Contains("arrow") && other.transform.GetComponent<Arrow>().enabled && criticalHit))
        {
            // If its not a hurtObject or last hit too few secs ago, ignore
            if (Array.IndexOf(hurtObjectNames, other.transform.name) == -1 || Time.time < lastHitTime + hitDelay)
                return;

            // If sword hits and player wasnt swinging, ignore
            if (playerMustSwing && other.transform.name.ToLower().Contains("sword") &&
                !Mecanim.inAnyAnim(player.GetComponent<Animator>(), player.GetComponent<PlayerBattleControl>().swordRootMotionAnimations, 0))
                return;

            // If its a fake arrow thats on the bow before being shot
            if (other.transform.name.ToLower().Contains("arrow") && !other.transform.GetComponent<Arrow>().enabled)
                return;
        }

        lastHitTime = Time.time;

        SendMessage("HitByPlayer");

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
