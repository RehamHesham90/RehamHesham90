using UnityEngine;

/// <summary>
/// Health: Manages the health state of an entity.
/// Attach this script to the Zombie GameObject.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;

    // Private state to track current health.
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// PUBLIC API: Receives damage from external sources (like a Bullet).
    /// </summary>
    /// <param name="damageAmount">The amount of damage to subtract.</param>
    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0) return; // Already dead

        currentHealth -= damageAmount;

        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Remaining health: " + currentHealth);

        // Check for death condition
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles the consequences of the entity's health reaching zero.
    /// </summary>
    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");

        // --- Death Logic ---

        // 1. Disable components (e.g., movement, input)
        // Example: GetComponent<PlayerMovement>()?.enabled = false;

        // 2. Play death animation/sound

        // 3. Destroy the GameObject after a delay (if it's an enemy)
        Destroy(gameObject, 3f);
    }


}
