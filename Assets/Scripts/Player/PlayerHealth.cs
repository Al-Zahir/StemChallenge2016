using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{

    public float health = 100f;
    public float notority = 0f; //how suspicious the player is
    public float resetAfterDeathTime = 5f;
    public AudioClip deathClip;

    private Animator anim;
    private PlayerMovement playerMovement;
    public ScreenColor sceneColorManager, hitColorManager, healthColorManager;
    private AudioSource footstepsAudio;
    private bool playerDead;
    private Color hitColor, healthColor;
    public float hitLength = 0.2f;
    public float healthRegenRate = 5f;

    public AudioClip[] hurtSounds;

    public Transform playerSpawn;

    private IEnumerator currentLoseHealth;

    void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        footstepsAudio = GetComponent<AudioSource>();
        playerDead = false;
        hitColor = hitColorManager.GetComponent<Image>().color;
        healthColor = healthColorManager.GetComponent<Image>().color;
    }

    void Update()
    {
        if (health <= 0 && !playerDead)
            StartCoroutine(DeathSequence());
        else if(health < 100)
        {
            health += healthRegenRate * Time.deltaTime;
            UpdateHealth(false);
        }
    }

    private IEnumerator DeathSequence()
    {
        playerDead = true;
        anim.SetFloat("Speed", 0.0f);
        playerMovement.enabled = false;
        anim.SetTrigger("Death");
        sceneColorManager.SetColor(Color.black);
        yield return new WaitForSeconds(resetAfterDeathTime);
        playerMovement.transform.position = playerSpawn.position;
        health = 100;
        UpdateHealth(false);
        sceneColorManager.SetColor(Color.clear);
        playerMovement.enabled = true;
        playerDead = false;
    }

    private void UpdateHealth(bool isSmooth)
    {
        if (!isSmooth)
            healthColorManager.GetComponent<Image>().color = new Color(healthColor.r, healthColor.g, healthColor.b, (100f - health) / 110);

        healthColorManager.SetColor(new Color(healthColor.r, healthColor.g, healthColor.b, (100f - health) / 110), 1);
    }

    public void TakeDamage(float amount)
    {
        //Debug.Log(amount);
        health -= amount;
        UpdateHealth(true);

        if (currentLoseHealth != null)
            StopCoroutine(currentLoseHealth);

        currentLoseHealth = LoseHealth();
        StartCoroutine(currentLoseHealth);

        GetComponent<AudioSource>().clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
        GetComponent<AudioSource>().Play();
    }

    private IEnumerator LoseHealth()
    {
        hitColorManager.SetColor(hitColor, hitLength);
        yield return new WaitForSeconds(hitLength);
        hitColorManager.SetColor(Color.clear, hitLength);
        yield return new WaitForSeconds(hitLength);

        currentLoseHealth = null;
    }
}
