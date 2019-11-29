using UnityEngine;

/// <summary>
/// Helps the animator access the shoot functions.
/// </summary>
public class ShootHelper : MonoBehaviour
{
    /// <summary>
    /// Access the shoot function from the player controller.
    /// </summary>
    public void Shoot()
    {
        GetComponentInParent<PlayerController>().combatSettings.Shoot();
    }

    /// <summary>
    /// Accesses the finish shoot function from the player controller.
    /// </summary>
    public void FinishShoot()
    {
        GetComponentInParent<PlayerController>().combatSettings.FinishShoot();
    }
}