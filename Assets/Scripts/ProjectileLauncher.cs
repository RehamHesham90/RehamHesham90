using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{

    // --- PROJECTILE SETTINGS ---
    [Header("Shooting Settings")]
    public float launchForce = 10.0f;
    public float fireRate = 0.2f;  // Time (in seconds) between shots.
    //public GameObject projectilePrefab;
    //public float launchForce = 10f;
    
   
    [Header("Weapon Configuration")]
    //public GameObject projectilePrefab;       
    public GameObject bulletPrefab;     // The bullet or physical projectile to instantiate.
    public Transform firePoint;      // A point on your gun/player where bullets spawn // The position/direction from which the projectile launches.

    // PRIVATE STATE: This tracks when the next shot is allowed.
    private float nextFireTime = 0f;

    /// <summary>
    /// PUBLIC API: This is the method called by the controller (ProjectileShooter)
    /// every time the user presses the fire button.
    /// </summary>
    /// <returns>True if the shot was fired, false if it was on cooldown.</returns>
    public bool TryFire()
    {
        // 1. STATE CHECK: Check if enough time has passed since the last shot.
        // This logic is completely contained within the weapon component.
        if (Time.time >= nextFireTime)
        {
            // 2. ONE-SHOT ACTION: Update the cooldown state for the next shot.
            nextFireTime = Time.time + fireRate;

            // 3. EXECUTION: If the check passes, execute the firing logic.
            LaunchProjectile();

            return true;
        }

        // If the time check failed, return false.
        return false;
    }

    /// <summary>
    /// Private function that handles the actual instantiation and launch of the projectile.
    /// This should only be called internally by TryFire().
    /// </summary>
    private void LaunchProjectile()
    {
        // Example Firing Logic: Instantiate the projectile.
        if (bulletPrefab != null && firePoint != null)
        {
            // Instantiate the projectile at the designated fire point.
            // Quaternion.identity means no extra rotation, using the firePoint's rotation.
            GameObject projectile = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // You would typically add velocity/force here:
            projectile.GetComponent<Rigidbody>().linearVelocity = firePoint.forward * launchForce;

            // Optional: Play a sound effect or muzzle flash animation here.
            Debug.Log("Projectile Fired! Current time: " + Time.time);
        }
        else
        {
            Debug.LogError("ProjectileLauncher requires a Prefab and a Fire Point to function.");
        }
    }
    
   
}