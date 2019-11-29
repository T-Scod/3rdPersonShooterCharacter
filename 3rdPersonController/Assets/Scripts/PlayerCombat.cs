using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

/// <summary>
/// Handles the player shooting and abilities.
/// </summary>
[Serializable]
public class PlayerCombat
{
    /// <summary>
    /// The type of state the player is in.
    /// </summary>
    public enum CombatState
    {
        Idle,
        Charging,
        PreReloading,
        Reloading,
        Shooting,
    }

    #region Variables
    #region Public
    /// <summary>
    /// Reference to the bullet prefab.
    /// </summary>
    [Tooltip("Reference to the bullet prefab.")]
    public GameObject shotgunBulletPrefab;
    /// <summary>
    /// Reference to the crosshair object in the scene.
    /// </summary>
    [Tooltip("Reference to the crosshair object in the scene.")]
    public RectTransform crosshairObject;
    /// <summary>
    /// Offsets the scale of the crosshair to get a gap between the reticle images.
    /// </summary>
    [Tooltip("Offsets the scale of the crosshair to get a gap between the reticle images.")]
    public float crosshairScaleOffset = 0.2f;
    /// <summary>
    /// Contains all the images of the crosshair.
    /// </summary>
    [Tooltip("Contains all the images of the crosshair.")]
    public Image[] crosshairImages;
    /// <summary>
    /// Contains all the ammo images.
    /// </summary>
    [Tooltip("Contains all the ammo images.")]
    public Image[] ammoImages;
    /// <summary>
    /// Reference to where the objects will be spawnned from.
    /// </summary>
    [Tooltip("Reference to where the objects will be spawnned from.")]
    public Transform gunTransform;
    /// <summary>
    /// All the layers that the bullets can collide with.
    /// </summary>
    [Tooltip("All the layers that the bullets can collide with.")]
    public LayerMask collidableLayers;
    /// <summary>
    /// Time it takes for the reticle to return to its original transformation after a shot.
    /// </summary>
    [Tooltip("Time it takes for the reticle to return to its original transformation after a shot.")]
    public float shotDelaySpeed = 1.0f;
    /// <summary>
    /// How quickly a shot charges.
    /// </summary>
    [Tooltip("How quickly a shot charges.")]
    public float chargeSpeed = 2.0f;
    /// <summary>
    /// How quickly a cartridge is emptied when the player reloads.
    /// </summary>
    [Tooltip("How quickly a cartridge is emptied when the player reloads.")]
    public float preReloadSpeed = 3.5f;
    /// <summary>
    /// How quickly the crosshair rotates.
    /// </summary>
    [Tooltip("How quickly the crosshair rotates.")]
    public float crosshairRotateSpeed = 1.0f;
    /// <summary>
    /// How quickly the crosshair shrinks.
    /// </summary>
    [Tooltip("How quickly the crosshair shrinks.")]
    public float crosshairShrinkSpeed = 1.0f;
    /// <summary>
    /// The extents of the shot spread.
    /// </summary>
    [Tooltip("The extents of the shot spread.")]
    public float spreadShotExtents = 30.0f;
    /// <summary>
    /// Amount of bullets fired in a shotgun shot.
    /// </summary>
    [Tooltip("Amount of bullets fired in a shotgun shot.")]
    public int spreadAmount = 6;
    #endregion
    #region Hidden
    /// <summary>
    /// Reference to the player animator.
    /// </summary>
    [HideInInspector]
    public Animator animator;
    /// <summary>
    /// Stores the current combat state.
    /// </summary>
    [HideInInspector]
    public CombatState combatState = CombatState.Idle;
    #endregion
    #region Private
    /// <summary>
    /// Object pool of bullets.
    /// </summary>
    private Queue<GameObject> m_bullets = new Queue<GameObject>();
    /// <summary>
    /// Current amount of ammo count.
    /// </summary>
    private int m_ammoCount;
    /// <summary>
    /// The current extents of the spread shot.
    /// </summary>
    private float m_spreadShotExtents = 0.0f;
    /// <summary>
    /// Reference to the player.
    /// </summary>
    private PlayerController m_player;
    #endregion
    #endregion

    /// <summary>
    /// Fills the pool with the objects.
    /// </summary>
    public void Init()
    {
        // initialises the reference to the player
        m_player = UnityEngine.Object.FindObjectOfType<PlayerController>();

        // gets the max ammo amount and squares that amount for the number of bullets that will be instantiated.
        m_ammoCount = ammoImages.Length;
        int bulletAmount = m_ammoCount * m_ammoCount;

        // fills the object pool
        for (int i = 0; i < bulletAmount; i++)
        {
            GameObject bullet = UnityEngine.Object.Instantiate(shotgunBulletPrefab, gunTransform);
            bullet.SetActive(false);
            m_bullets.Enqueue(bullet);
        }

        m_spreadShotExtents = spreadShotExtents;
    }

    /// <summary>
    /// Determines the current combat state.
    /// </summary>
    public void ChargeShot()
    {
        switch (combatState)
        {
            case CombatState.Idle:
                // checks if the player is not shooting
                if (!animator.GetCurrentAnimatorStateInfo(2).IsName("Shoot"))
                {
                    // if the reload button is pressed and bullets have been used then reload
                    if (Input.GetButtonDown("Reload") && m_ammoCount < ammoImages.Length)
                    {
                        combatState = CombatState.PreReloading;
                    }
                    // if the player is aiming and the shoot button is held down then start charging a shot
                    else if (m_player.cameraSettings.aiming && Input.GetButtonDown("Shoot"))
                    {
                        combatState = CombatState.Charging;
                    }
                }
                // zooms out the crosshair
                ScopeOut(shotDelaySpeed);
                break;
            case CombatState.Charging:
                // sends the combat state back to idle if the player is not aiming.
                if (!m_player.cameraSettings.aiming)
                {
                    combatState = CombatState.Idle;
                }
                // checks if the player is not already shooting
                if (!animator.GetCurrentAnimatorStateInfo(2).IsName("Shoot"))
                {
                    // triggers the shoot animation if the shoot button let go of
                    if (Input.GetButtonUp("Shoot"))
                    {
                        TriggerShoot();
                    }
                    else
                    {
                        // zooms in the crosshair
                        ScopeIn(chargeSpeed);
                    }
                }
                break;
            case CombatState.PreReloading:
                // checks if the crosshair is not zoomed in all the way
                if (crosshairObject.localEulerAngles.z > 302.0f)
                {
                    ScopeIn(preReloadSpeed);
                }
                // zoomed in all the way
                else
                {
                    // empties the ammo
                    m_ammoCount = 0;
                    foreach (Image image in ammoImages)
                    {
                        image.gameObject.SetActive(false);
                    }
                    combatState = CombatState.Reloading;
                }
                break;
            case CombatState.Reloading:
                // checks if the crosshair is not zoomed out all the way
                if (crosshairObject.localEulerAngles.z < 356.0f)
                {
                    ScopeOut(shotDelaySpeed);
                }
                // zoomed out all the way
                else
                {
                    // refills the ammo
                    m_ammoCount = ammoImages.Length;
                    foreach (Image image in ammoImages)
                    {
                        image.gameObject.SetActive(true);
                    }
                    combatState = CombatState.Idle;
                }
                break;
            case CombatState.Shooting:
                // ensures that if the player is shooting, that no other action can occur
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Zooms in the crosshair.
    /// </summary>
    /// <param name="speed">How quickly the crosshair is transformed.</param>
    private void ScopeIn(float speed)
    {
        // expands the reticle images so that the gaps are closed
        foreach (Image image in crosshairImages)
        {
            image.rectTransform.localScale = Vector3.Lerp(image.rectTransform.localScale, Vector3.one, speed * crosshairShrinkSpeed * Time.deltaTime);
            image.transform.parent.GetComponent<RectTransform>().localScale = Vector3.Lerp(image.transform.parent.GetComponent<RectTransform>().localScale, Vector3.one * 0.5f, speed * crosshairShrinkSpeed * Time.deltaTime);
        }
        // rotates the crosshair
        crosshairObject.localRotation = Quaternion.Slerp(crosshairObject.localRotation, Quaternion.Euler(0.0f, 0.0f, 300.0f), speed * crosshairRotateSpeed * Time.deltaTime);
        // reduces the spread
        m_spreadShotExtents = Mathf.Lerp(m_spreadShotExtents, 0.0f, speed * crosshairShrinkSpeed * Time.deltaTime);

        // checks if the shot is fully charged
        if (combatState == CombatState.Charging && crosshairObject.localEulerAngles.z < 301.0f)
        {
            // triggers the shoot animation
            animator.SetTrigger("Shoot");
            combatState = CombatState.Shooting;
        }
    }

    /// <summary>
    /// Zooms out the crosshair.
    /// </summary>
    /// <param name="speed">How quickly the crosshair transforms.</param>
    private void ScopeOut(float speed)
    {
        // shrinks the reticle images so that the gaps open back up
        foreach (Image image in crosshairImages)
        {
            image.rectTransform.localScale = Vector3.Lerp(image.rectTransform.localScale, Vector3.one * (1.0f - crosshairScaleOffset), speed * crosshairShrinkSpeed * Time.deltaTime);
            image.transform.parent.GetComponent<RectTransform>().localScale = Vector3.Lerp(image.transform.parent.GetComponent<RectTransform>().localScale, Vector3.one * (1.0f + crosshairScaleOffset), speed * crosshairShrinkSpeed * Time.deltaTime);
        }
        // rotates the crosshair
        crosshairObject.localRotation = Quaternion.Slerp(crosshairObject.localRotation, Quaternion.identity, speed * crosshairRotateSpeed * Time.deltaTime);
        // increases the spread
        m_spreadShotExtents = Mathf.Lerp(m_spreadShotExtents, spreadShotExtents, speed * crosshairShrinkSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Gets a target from a ray.
    /// </summary>
    /// <param name="isRailgunShot">Determines if the shot is a railgun shot.</param>
    /// <returns>Returns the position that the ray hit the object.</returns>
    private Vector3 GetTarget(bool isRailgunShot = false)
    {
        Ray ray;

        // if the shot is a railgun shot then use a straight ray from the camera as the ray
        if (isRailgunShot)
        {
            ray = RectTransformUtility.ScreenPointToRay(Camera.main, Camera.main.pixelRect.center);
        }
        // if the shot is a shotgun shot then get a random position within the spread extents for the ray origin
        else
        {
            // creates a random offset to apply to the camera origin
            Vector2 offset = new Vector2
            {
                x = UnityEngine.Random.Range(-m_spreadShotExtents, m_spreadShotExtents),
                y = UnityEngine.Random.Range(-m_spreadShotExtents, m_spreadShotExtents)
            };

            ray = RectTransformUtility.ScreenPointToRay(Camera.main, Camera.main.pixelRect.center + offset);
        }

        RaycastHit raycastHit;
        // stores the hit information from the ray in the raycast hit object
        Physics.Raycast(ray, out raycastHit, collidableLayers);
        return raycastHit.point;
    }

    /// <summary>
    /// Triggers the shoot animation.
    /// </summary>
    public void TriggerShoot()
    {
        animator.SetTrigger("Shoot");
        combatState = CombatState.Shooting;
    }

    /// <summary>
    /// Returns the combat state to the start.
    /// </summary>
    public void FinishShoot()
    {
        // if the player is no longer shooting then change the layer back over
        if (!m_player.cameraSettings.aiming)
        {
            Animator animator = m_player.GetComponentInChildren<Animator>();
            animator.SetBool("Aim", false);
            animator.SetLayerWeight(1, 1.0f);
            animator.SetLayerWeight(2, 0.0f);
        }

        // reloads if out of ammo
        if (m_ammoCount <= 0)
        {
            combatState = CombatState.PreReloading;
        }
        // goes back to idle
        else
        {
            combatState = CombatState.Idle;
        }
    }

    /// <summary>
    /// Removes the bullet from the pool and makes it start flying.
    /// </summary>
    public void Shoot()
    {
        if (combatState != CombatState.Shooting)
        {
            return;
        }

        // checks if the crosshair is zoomed in completely
        if (crosshairObject.localEulerAngles.z < 305.5f)
        {
            // removes the bullet from the collection and unparents it
            GameObject bullet = m_bullets.Dequeue();
            bullet.transform.parent = null;
            // makes the bullet start flying through the air
            bullet.GetComponent<Bullet>().Shoot(GetTarget(true), true);
            // empties the ammo
            m_ammoCount = 0;
            foreach (Image image in ammoImages)
            {
                image.gameObject.SetActive(false);
            }
        }
        // shotgun shot
        else
        {
            // shoots as many bullets as the spread amount specifies
            GameObject[] bullets = new GameObject[spreadAmount];
            for (int i = 0; i < spreadAmount; i++)
            {
                bullets[i] = m_bullets.Dequeue();
                bullets[i].transform.parent = null;
                bullets[i].GetComponent<Bullet>().Shoot(GetTarget());
            }

            // reduces the ammo count
            m_ammoCount--;
            ammoImages[m_ammoCount].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Adds the bullet back to the pool.
    /// </summary>
    /// <param name="bullet">The bullet that is being added back in.</param>
    public void ResetBullet(GameObject bullet)
    {
        // turns the bullet off and reparents it
        bullet.SetActive(false);
        bullet.transform.position = gunTransform.position;
        bullet.transform.parent = gunTransform;
        // adds the bullet back to the collecion
        m_bullets.Enqueue(bullet);
    }
}