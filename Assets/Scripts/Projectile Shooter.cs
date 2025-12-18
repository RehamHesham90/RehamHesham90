using UnityEngine;

/// <summary>
/// ProjectileShooter: INPUT HANDLING MODULE
///
/// This script is the "trigger finger" component of the player.
/// It is responsible ONLY for detecting the player's intention to shoot (Input.GetButton("Fire1"))
/// and delegating the action to the currently equipped weapon.
///
/// It requires a 'PlayerInventory' component on the same GameObject to function.
/// </summary>
public class ProjectileShooter : MonoBehaviour
{
    // A private reference to the central Inventory system on the player.
    // This component is the only source of truth for whether the player is armed.
    private PlayerInventory inventory;

    void Start()
    {
        // Initialization: Get the PlayerInventory component once at the start.
        // This is much more efficient than calling GetComponent<>() every frame.
        inventory = GetComponent<PlayerInventory>();

        // CRITICAL DEPENDENCY CHECK: Ensure the required component is present.
        if (inventory == null)
        {
            Debug.LogError("ProjectileShooter requires a PlayerInventory component on the same GameObject!");
            // If the dependency is missing, disable the component to prevent NullReferenceExceptions in Update().
            enabled = false;
        }
    }

    void Update()
    {
        // 1. INPUT CHECK: Detect continuous 'Fire1' input (Left Mouse Button by default).
        // Using GetButton allows the player to hold the mouse down for automatic weapons.
        if (Input.GetButton("Fire1"))
        {
            // 2. PERMISSION CHECK: Ask the inventory for the currently active weapon's launcher.
            // This relies on the public 'GetCurrentLauncher()' method in your PlayerInventory Canvas.
            ProjectileLauncher currentLauncher = inventory.GetCurrentLauncher();

            // CRITICAL CONDITION: Only proceed if a launcher is equipped (i.e., not null).
            if (currentLauncher != null)
            {
                // 3. DELEGATION: Call the weapon's public firing API.
                // The 'TryFire()' method in the ProjectileLauncher is responsible for:
                // a) Checking the internal cooldown timer (Time.time >= nextFireTime).
                // b) Updating the cooldown timer if the shot is successful.
                // c) Instantiating the projectile.
                bool firedSuccessfully = currentLauncher.TryFire();

                if (!firedSuccessfully)
                {
                    // Optional: If the weapon is on cooldown, you can add code here 
                    // to give the player feedback (e.g., a weapon 'clack' or UI prompt).
                }
            }
            // If currentLauncher is null, the player is unarmed, and pressing the fire button 
            // does nothing, as desired.
        }
    }
}