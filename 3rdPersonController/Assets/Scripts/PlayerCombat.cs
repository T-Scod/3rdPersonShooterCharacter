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
    public GameObject shotgunBulletPrefab;
    public RectTransform crosshairObject;
    public float crosshairScaleOffset = 0.2f;
    public Image[] crosshairImages;
    public Image[] ammoImages;
    /// <summary>
    /// Reference to where the objects will be spawnned from.
    /// </summary>
    [Tooltip("Reference to where the objects will be spawnned from.")]
    public Transform gunTransform;
    public LayerMask collidableLayers;
    /// <summary>
    /// Time it takes for the reticle to return to its original transformation after a shot.
    /// </summary>
    public float shotDelaySpeed = 1.0f;
    /// <summary>
    /// Time it take for a shot to be max charged.
    /// </summary>
    public float chargeSpeed = 2.0f;
    public float preReloadSpeed = 3.5f;
    public float crosshairRotateSpeed = 1.0f;
    public float crosshairShrinkSpeed = 1.0f;
    public float spreadShotExtents = 30.0f;
    public int spreadAmount = 6;
    #endregion
    #region Hidden
    /// <summary>
    /// Reference to the player animator.
    /// </summary>
    [HideInInspector]
    public Animator animator;
    //[HideInInspector]
    public CombatState combatState = CombatState.Idle;
    #endregion
    #region Private
    /// <summary>
    /// Object pool of bullets.
    /// </summary>
    private Queue<GameObject> m_bullets = new Queue<GameObject>();
    //private Vector2 m_crosshairStartPos;
    private int m_ammoCount;
    private float m_spreadShotExtents = 0.0f;
    private PlayerController m_player;
    #endregion
    #endregion

    /// <summary>
    /// Gets the max heights of the indicators and loads the ammo clip.
    /// </summary>
    public void Init()
    {
        m_player = UnityEngine.Object.FindObjectOfType<PlayerController>();
        m_ammoCount = ammoImages.Length;
        int bulletAmount = (m_ammoCount * m_ammoCount) * 2;

        // fills the object pool
        for (int i = 0; i < bulletAmount; i++)
        {
            GameObject bullet = UnityEngine.Object.Instantiate(shotgunBulletPrefab, gunTransform);
            bullet.SetActive(false);
            m_bullets.Enqueue(bullet);
        }

        //m_crosshairStartPos = new Vector2(crosshairImages[0].rectTransform.localPosition.x, crosshairImages[0].rectTransform.localPosition.y);
        m_spreadShotExtents = spreadShotExtents;
    }

    public void ChargeShot()
    {
        switch (combatState)
        {
            case CombatState.Idle:
                if (!animator.GetCurrentAnimatorStateInfo(2).IsName("Shoot"))
                {
                    if (Input.GetButtonDown("Reload") && m_ammoCount < ammoImages.Length)
                    {
                        combatState = CombatState.PreReloading;
                    }
                    else if (m_player.cameraSettings.aiming && Input.GetButtonDown("Shoot"))
                    {
                        combatState = CombatState.Charging;
                    }
                }

                ScopeOut(shotDelaySpeed);
                break;
            case CombatState.Charging:
                if (!m_player.cameraSettings.aiming)
                {
                    combatState = CombatState.Idle;
                }
                if (!animator.GetCurrentAnimatorStateInfo(2).IsName("Shoot"))
                {
                    if (Input.GetButtonUp("Shoot"))
                    {
                        TriggerShoot();
                    }
                    else
                    {
                        ScopeIn(chargeSpeed);
                    }
                }
                break;
            case CombatState.PreReloading:
                if (crosshairObject.localEulerAngles.z > 302.0f)
                {
                    ScopeIn(preReloadSpeed);
                }
                else
                {
                    m_ammoCount = 0;
                    foreach (Image image in ammoImages)
                    {
                        image.gameObject.SetActive(false);
                    }
                    combatState = CombatState.Reloading;
                }
                break;
            case CombatState.Reloading:
                if (crosshairObject.localEulerAngles.z < 356.0f)
                {
                    ScopeOut(shotDelaySpeed);
                }
                else
                {
                    m_ammoCount = ammoImages.Length;
                    foreach (Image image in ammoImages)
                    {
                        image.gameObject.SetActive(true);
                    }
                    combatState = CombatState.Idle;
                }
                break;
            case CombatState.Shooting:
                break;
            default:
                break;
        }
    }

    private void ScopeIn(float speed)
    {
        foreach (Image image in crosshairImages)
        {
            image.rectTransform.localScale = Vector3.Lerp(image.rectTransform.localScale, Vector3.one, speed * crosshairShrinkSpeed * Time.deltaTime);
            image.transform.parent.GetComponent<RectTransform>().localScale = Vector3.Lerp(image.transform.parent.GetComponent<RectTransform>().localScale, Vector3.one * 0.5f, speed * crosshairShrinkSpeed * Time.deltaTime);
        }
        crosshairObject.localRotation = Quaternion.Slerp(crosshairObject.localRotation,
            Quaternion.Euler(0.0f, 0.0f, 300.0f), speed * crosshairRotateSpeed * Time.deltaTime);
        m_spreadShotExtents = Mathf.Lerp(m_spreadShotExtents, 0.0f, speed * crosshairShrinkSpeed * Time.deltaTime);

        if (combatState == CombatState.Charging && crosshairObject.localEulerAngles.z < 301.0f)
        {
            // triggers the shoot animation
            animator.SetTrigger("Shoot");
            crosshairObject.GetComponent<Animator>().SetTrigger("Shoot");
            combatState = CombatState.Shooting;
        }
    }

    private void ScopeOut(float speed)
    {
        foreach (Image image in crosshairImages)
        {
            image.rectTransform.localScale = Vector3.Lerp(image.rectTransform.localScale, Vector3.one * (1.0f - crosshairScaleOffset), speed * crosshairShrinkSpeed * Time.deltaTime);
            image.transform.parent.GetComponent<RectTransform>().localScale = Vector3.Lerp(image.transform.parent.GetComponent<RectTransform>().localScale, Vector3.one * (1.0f + crosshairScaleOffset), speed * crosshairShrinkSpeed * Time.deltaTime);
        }
        crosshairObject.localRotation = Quaternion.Slerp(crosshairObject.localRotation,
            Quaternion.identity, speed * crosshairRotateSpeed * Time.deltaTime);

        if (m_player.cameraSettings.aiming)
        {
            m_spreadShotExtents = Mathf.Lerp(m_spreadShotExtents, spreadShotExtents, speed * crosshairShrinkSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetTarget(bool isRailgunShot = false)
    {
        Ray ray;

        if (isRailgunShot)
        {
            ray = RectTransformUtility.ScreenPointToRay(Camera.main, Camera.main.pixelRect.center);
        }
        else
        {
            Vector2 offset = new Vector2
            {
                x = UnityEngine.Random.Range(-m_spreadShotExtents, m_spreadShotExtents),
                y = UnityEngine.Random.Range(-m_spreadShotExtents, m_spreadShotExtents)
            };

            ray = RectTransformUtility.ScreenPointToRay(Camera.main, Camera.main.pixelRect.center + offset);
        }

        RaycastHit raycastHit;
        // checks if it hit anything
        if (Physics.Raycast(ray, out raycastHit, 100000.0f, collidableLayers))
        {
            return raycastHit.point;
        }
        // the ray did not hit anything
        else
        {
            return ray.GetPoint(100.0f);
        }
    }

    public void TriggerShoot()
    {
        // triggers the shoot animation
        animator.SetTrigger("Shoot");
        crosshairObject.GetComponent<Animator>().SetTrigger("Shoot");
        combatState = CombatState.Shooting;
    }

    public void FinishShoot()
    {
        if (!m_player.cameraSettings.aiming)
        {
            Animator animator = m_player.GetComponentInChildren<Animator>();
            animator.SetBool("Aim", false);
            animator.SetLayerWeight(1, 1.0f);
            animator.SetLayerWeight(2, 0.0f);
            animator.SetLayerWeight(3, 0.0f);
        }

        if (m_ammoCount <= 0)
        {
            combatState = CombatState.PreReloading;
        }
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

        if (crosshairObject.localEulerAngles.z < 305.5f)
        {
            // removes the bullet from the collection and unparents it
            GameObject bullet = m_bullets.Dequeue();
            bullet.transform.parent = null;
            // makes the bullet start flying through the air
            bullet.GetComponent<Bullet>().Shoot(GetTarget(true), true);
            m_ammoCount = 0;
            foreach (Image image in ammoImages)
            {
                image.gameObject.SetActive(false);
            }

            combatState = CombatState.Reloading;
        }
        else
        {
            GameObject[] bullets = new GameObject[spreadAmount];
            for (int i = 0; i < spreadAmount; i++)
            {
                bullets[i] = m_bullets.Dequeue();
                bullets[i].transform.parent = null;
                bullets[i].GetComponent<Bullet>().Shoot(GetTarget());
            }

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