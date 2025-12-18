using UnityEngine;

public class Projectile : MonoBehaviour
{
    // The projectile will be automatically destroyed after this time
    public float lifetime = 5.0f;
    // The damage value this specific bullet inflicts
    public float bulletDamage = 10f;
    void Start()
    {
         //Destroy(gameObject, lifetime);
    }

    // Check for collisions
    // Unity's method triggered when two rigidbodies/colliders touch
    private void OnCollisionEnter(Collision collision)
    {

        // Check if the collided object has the AdvancedZombieAI component
        AdvancedZombieAI zombieAI = collision.gameObject.GetComponent<AdvancedZombieAI>();

        // === ZOMBIE COMMUNICATION ===
        // 1. If the component is found (meaning we hit a zombie)
        if (zombieAI != null)
        {
            // 2. We call the public method on the zombie's script instance.
            // This is the command telling the zombie to take damage.
            zombieAI.TakeDamage(bulletDamage);
            Destroy(collision.gameObject,lifetime);
        }
        // Check if we hit a 'Zombie' or another target
        if (collision.gameObject.CompareTag("Zombie"))
        {
            // Put your hit logic here (e.g., call a 'TakeDamage' function on the zombie)
            Debug.Log(zombieAI);

            // Destroy the zombie object (optional: implement health instead)
            Destroy(collision.gameObject, lifetime);
        }

        // Always destroy the projectile upon impact
        Destroy(gameObject, lifetime);
    }
}