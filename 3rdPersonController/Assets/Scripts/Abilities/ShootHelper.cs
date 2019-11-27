using UnityEngine;

/// <summary>
/// Helps the animator access the shoot function.
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

    public void FinishShoot()
    {
        GetComponentInParent<PlayerController>().combatSettings.FinishShoot();
    }
}