using UnityEngine;


/// <summary>
/// WeaponPickup: Handles the acquisition logic for a weapon object found in the world.
/// Requires a ProjectileLauncher component on the same GameObject.
/// </summary>
public class WeaponPickup : MonoBehaviour
{

    [Header("Pickup Settings")]
    public float pickupDistance = 2.0f;
    public KeyCode pickupKey = KeyCode.E;

    // Internal reference to the weapon's firing mechanics
    private ProjectileLauncher weaponLauncher;

    void Start()
    {
        // Get the firing component from the weapon itself
        weaponLauncher = GetComponent<ProjectileLauncher>();
        if (weaponLauncher == null)
        {
            Debug.LogError("WeaponPickup requires a ProjectileLauncher component on the same object!");
            // Disable the pickup if the core firing logic is missing.
            enabled = false;
        }
    }

    void Update()
    {
        // Simple proximity and input check
        if (Input.GetKeyDown(pickupKey))
        {
            // Find the player object (we assume it is tagged "Player")
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null && Vector3.Distance(transform.position, player.transform.position) <= pickupDistance)
            {
                // Try to get the player's inventory component
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();

                if (inventory != null)
                {
                    // CRITICAL: Hand the weapon's launcher component to the inventory.
                    // The Inventory will handle moving the object and enabling the logic.
                    inventory.AcquireWeapon(gameObject, weaponLauncher);

                    // The pickup script's job is done. Disable it.
                    enabled = false;
                }
            }
        }
    }

}
