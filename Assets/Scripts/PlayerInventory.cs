using UnityEngine;

/// <summary>
/// PlayerInventory: Manages the player's equipped weapon and provides the 
/// active ProjectileLauncher reference to the input handlers.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Mount Point")]
    // The visual location in the player's hierarchy where the weapon will be parented.
    // CRITICAL NOTE: This Transform must be a child of the object that handles 
    // the camera's vertical (up/down) rotation for the weapon to follow the mouse look.
    public Transform weaponMountPoint;

    // The currently equipped weapon's firing script. This is the source of truth.
    private ProjectileLauncher currentLauncher = null;

    /// <summary>
    /// Called by the WeaponPickup script when the player acquires the weapon.
    /// </summary>
    public void AcquireWeapon(GameObject weaponObject, ProjectileLauncher launcher)
    {
        // 1. Un-equip any previous weapon (Cleanup placeholder)
        if (currentLauncher != null)
        {
            // Drop current weapon or hide it
            currentLauncher.gameObject.SetActive(false);
        }

        // 2. Set the new weapon reference.
        currentLauncher = launcher;

        // --- FIXES FOR SCALE, ROTATION, AND SWINGING ---

        // 3. Move the physical object into the player's hand hierarchy.
        weaponObject.transform.SetParent(weaponMountPoint);

        // FIX (Rotation/Drifting): Reset local rotation to Quaternion.identity.
        weaponObject.transform.localRotation = Quaternion.identity;

        // FIX (Position/Snapping): Reset local position to zero.
        // This makes the weapon snap perfectly to the exact center of the mount point.
        weaponObject.transform.localPosition = Vector3.zero;

        // FIX (Scale Diffusion): Explicitly set local scale to ensure correct size.
        // This is crucial because the weapon was likely scaled differently on the ground.
        //weaponObject.transform.localScale = Vector3.one;

        // FIX (Swinging/Physics Interference): Stop the Rigidbody.
        // This prevents residual forces from the ground state from making the weapon "swing".
        Rigidbody rb = weaponObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Cleanup: Disable the now-obsolete pickup script.
        WeaponPickup pickupScript = weaponObject.GetComponent<WeaponPickup>();
        if (pickupScript != null) pickupScript.enabled = false;

        // 4. Activate the weapon object
        weaponObject.SetActive(true);

        Debug.Log("Weapon Acquired: " + weaponObject.name);
    }

    /// <summary>
    /// PUBLIC API: Allows the input handler (ProjectileShooter) to safely get 
    /// the current active launcher. If null, the player is unarmed.
    /// </summary>
    public ProjectileLauncher GetCurrentLauncher()
    {
        return currentLauncher;
    }
}
