using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    public static HealthManager instance;

    public int absoluteMaxHealth = 8;
    public int maxHealth = 8;
    public int currentHealth;

    public bool isInvincible = false;
    public float invincibleTime = 2f;

    [HideInInspector] 
    public HealthBarFlashing currentUIFlash;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void DamagePlayer(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentUIFlash != null)
        {
            currentUIFlash.TriggerHitFlash();
            currentUIFlash.SetLowHealthState(currentHealth == 1);
        }

        if (currentHealth <= 0)
        {
            RespawnController.instance.Respawn();
        }
    }

    public void UpgradeMaxHealth(int amount)
    {
        // Increase max health but never exceed the absolute maximum
        maxHealth = Mathf.Clamp(maxHealth + amount, 0, absoluteMaxHealth);

        // Heal the player to the new max
        currentHealth = maxHealth;

        // Update UI if needed
        if (currentUIFlash != null)
            currentUIFlash.SetLowHealthState(false);

        Debug.Log("Max health upgraded! New max = " + maxHealth);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (currentUIFlash != null)
            currentUIFlash.SetLowHealthState(currentHealth == 1);
    }

    public void FillHealth()
    {
        currentHealth = maxHealth;

        if (currentUIFlash != null)
            currentUIFlash.SetLowHealthState(false);
    }

    public void TriggerInvincibility()
    {
        StopAllCoroutines();
        StartCoroutine(InvincibilityCo());
    }

    private IEnumerator InvincibilityCo()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }
}
