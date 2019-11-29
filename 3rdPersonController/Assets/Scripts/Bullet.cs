using System.Collections;
using UnityEngine;

/// <summary>
/// Object that flies straight and reduces health on enemies.
/// </summary>
public class Bullet : MonoBehaviour
{
    #region Variables
    #region Public
    /// <summary>
    /// Fly speed of the bullet.
    /// </summary>
    [Tooltip("Fly speed of the bullet.")]
    public float bulletSpeed = 10.0f;
    /// <summary>
    /// How long the bullet is alive for before it resets.
    /// </summary>
    [Tooltip("How long the bullet is alive for before it resets.")]
    public float bulletLifeTime = 4.0f;
    /// <summary>
    /// Amount of damage a shotgun shot does to an enemy.
    /// </summary>
    [Tooltip("Amount of damage a shotgun shot does to an enemy.")]
    public int shotgunShotDamage = 1;
    /// <summary>
    /// Multiplies the damage of the bullet if it hits a critical point.
    /// </summary>
    [Tooltip("The amount the damage of a bullet is multiplied by if it hits a critical point.")]
    public int criticalDamageMultiplier = 3;
    /// <summary>
    /// Amount of damage a railgun shot does to an enemy.
    /// </summary>
    [Tooltip("Amount of damage a railgun shot does to an enemy.")]
    public int railgunShotDamage = 3;
    #endregion
    #region Private
    /// <summary>
    /// Determines if the bullet is charged.
    /// </summary>
    private bool m_isRailgunShot = false;
    /// <summary>
    /// Times how long the bullet is alive for.
    /// </summary>
    private float m_lifeTimer = 0.0f;
    /// <summary>
    /// Position the bullet is flying towards.
    /// </summary>
    private Vector3 m_target = Vector3.zero;
    /// <summary>
    /// Reference to the player.
    /// </summary>
    private PlayerController m_player;
    #endregion
    #endregion

    /// <summary>
    /// Initialises the reference to the player.
    /// </summary>
    private void Start()
    {
        m_player = FindObjectOfType<PlayerController>();
    }

    /// <summary>
    /// Sets the bullet up to start flying towards the target.
    /// </summary>
    /// <param name="target">The location the bullet will fly towards.</param>
    /// <param name="isRailgunShot">Determines if the shot is a railgun shot.</param>
    public void Shoot(Vector3 target, bool isRailgunShot = false)
    {
        // turns the bullet on and rotates it towards the target
        gameObject.SetActive(true);
        transform.LookAt(target);
        m_target = target;
        transform.localScale = Vector3.one;
        // stores whether or not it is a railgun shot
        m_isRailgunShot = isRailgunShot;
    }

    /// <summary>
    /// Increases life timer and moves towards target.
    /// </summary>
    private void Update()
    {
        // exits early if there is no target
        if (m_target == Vector3.zero)
        {
            return;
        }

        // increases the lifetime of the bullet
        m_lifeTimer += Time.deltaTime;

        // checks if it is not a railgun shot
        if (!m_isRailgunShot)
        {
            // moves towards target
            if (m_lifeTimer < bulletLifeTime)
            {
                // the vector between the bullet and the target
                Vector3 displacement = (m_target - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, m_target, bulletSpeed * Time.deltaTime);
                // moves target further away by the displacement vector
                m_target += displacement * bulletSpeed * Time.deltaTime;
            }
            // resets bullet
            else
            {
                m_lifeTimer = 0.0f;
                // adds the bullet back into the object pool
                m_player.combatSettings.ResetBullet(gameObject);
            }
        }
        // if the bullet is a railgun shot then increase scale along z axis by lifetime
        else if (m_lifeTimer < bulletLifeTime)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f + (m_lifeTimer * bulletSpeed));
        }
        // resets bullet
        else
        {
            m_lifeTimer = 0.0f;
            // adds the bullet back into the object pool
            m_player.combatSettings.ResetBullet(gameObject);
        }
    }

    /// <summary>
    /// Reacts based on the object the bullet collided with.
    /// </summary>
    /// <param name="other">Object being collided with.</param>
    private void OnTriggerEnter(Collider other)
    {
        // ignores collision with player and abilities
        if (other.tag != "Player" && other.tag != "Ability")
        {
            // determines the damage based on whether or not it is a railgun shot
            int damage = (m_isRailgunShot) ? railgunShotDamage : shotgunShotDamage;

            // decreases enemy health
            if (other.tag == "Enemy")
            {
                // attempts to find enemy script
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy == null)
                {
                    enemy = other.GetComponentInParent<Enemy>();
                    if (enemy == null)
                    {
                        enemy = other.GetComponentInChildren<Enemy>();
                    }
                }

                // applies damage to enemy if script is found
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
            // decreases enemy health by more if the bullet hit a critical point
            else if (other.tag == "Critical")
            {
                // attemps to find enemy script
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy == null)
                {
                    enemy = other.GetComponentInParent<Enemy>();
                    if (enemy == null)
                    {
                        enemy = other.GetComponentInChildren<Enemy>();
                    }
                }

                // applies critical damage to enemy
                enemy.TakeCriticalDamage(damage * criticalDamageMultiplier);
            }

            // shrinks the bullet if it is a railgun shot
            if (m_isRailgunShot)
            {
                StartCoroutine(RailgunShotShrink());
                return;
            }

            // resets the bullet
            m_lifeTimer = 0.0f;
            // adds the bullet back into the object pool
            m_player.combatSettings.ResetBullet(gameObject);
        }
    }

    /// <summary>
    /// Shrinks the bullet.
    /// </summary>
    /// <returns>Returns an enumeration based on the part the coroutine is up to.</returns>
    private IEnumerator RailgunShotShrink()
    {
        // checks if the bullet has not finished shrinking
        while (transform.localScale.z > 0.0f)
        {
            m_target = Vector3.zero;
            // reduces the bullet scale along z axis
            transform.localScale = new Vector3(1.0f, 1.0f, transform.localScale.z - (bulletSpeed * Time.deltaTime * 0.5f));
            // moves the bullet forward to make it look like the back of the bullet is moving towards the contact point
            transform.Translate(transform.forward * bulletSpeed * Time.deltaTime, Space.World);

            // if the scale of the bullet along the z axis is less than 0 then set it to 0
            if (transform.localScale.z < 0.0f)
            {
                transform.localScale = new Vector3(1.0f, 1.0f);
            }

            yield return null;
        }

        // resets the bullet
        m_lifeTimer = 0.0f;
        // adds the bullet back into the object pool
        m_player.combatSettings.ResetBullet(gameObject);
    }
}