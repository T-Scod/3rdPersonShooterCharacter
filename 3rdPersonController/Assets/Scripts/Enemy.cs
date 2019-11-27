using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Variables
    #region Public
    /// <summary>
    /// Enemy health.
    /// </summary>
    [Tooltip("Enemy health.")]
    public float health;
    public Rigidbody disableObject;
    /// <summary>
    /// How fast the enemy flashes red when damaged.
    /// </summary>
    [Tooltip("How fast the enemy flashes red when damaged.")]
    public float flashSpeed = 10.0f;
    /// <summary>
    /// Health drop prefab.
    /// </summary>
    [Tooltip("Health drop prefab.")]
    public GameObject healthDrop;
    /// <summary>
    /// Chance of health pack dropping.
    /// </summary>
    [Tooltip("Chance of health pack dropping."), Range(0, 100)]
    public float healthDropChance = 50.0f;
    /// <summary>
    /// Chance of player doing a voice line.
    /// </summary>
    [Tooltip("Chance of player doing a voice line."), Range(0, 100)]
    public float voiceOverChance = 50.0f;
    public GameObject explosionParticle;
    public MeshRenderer[] meshes;
    public float speakTimer = 20.0f;
    #endregion
    #region protected
    /// <summary>
    /// mkaing it possible to find player
    /// </summary>
    protected PlayerController m_player;
    /// <summary>
    /// setting the enemy to diable or not
    /// </summary>
    protected bool m_isDisabled;
    /// <summary>
    /// checking if the player is detected
    /// </summary>
    protected bool m_playerDetected;
    #endregion
    #region Private
    /// <summary>
    /// timer for how long to diable enemy for
    /// </summary>
    private float m_disableTime;
    /// <summary>
    /// Determines if the enemy got hit.
    /// </summary>
    private bool m_flashing = false;
    /// <summary>
    /// Determines if the enemy got hit critically.
    /// </summary>
    private bool m_critical = false;
    /// <summary>
    /// Times how long the material has been flashing for.
    /// </summary>
    private float m_flashTimer = 0.0f;
    private Color[] m_colours;
    private float m_speakTimer;
    #endregion
    #region Audio
    public AudioSource emp;
    
    #endregion
    #endregion

    // Start is called before the first frame update
    protected void Start()
    {
        m_player = FindObjectOfType<PlayerController>();
        m_colours = new Color[meshes.Length];
        for (int i = 0; i < meshes.Length; i++)
        {
            m_colours[i] = meshes[i].material.color;
        }
        m_speakTimer = speakTimer;
    }

    // Update is called once per frame
    protected void Update()
    {
        m_speakTimer -= Time.deltaTime;
        if (m_disableTime > 0)
        {
            m_disableTime -= Time.deltaTime;
        }
        else
        {
            Reboot();
        }

        if (health <= 0)
        {
            Death();
        }

        // checks if the enemy was damaged
        if (m_flashing || m_critical)
        {
            // increases the time passed into the method
            m_flashTimer += Time.deltaTime;
            // gets the flash value at the time
            float value = Flash(m_flashTimer);
            Color flashColour;
            if (m_flashing)
            {
                // changes the green and blue values of the colour
                flashColour = new Color(1, 1, value);
            }
            else
            {
                // changes the blue value of the colour
                flashColour = new Color(1, value, value);
            }

            foreach (var mesh in meshes)
            {
                mesh.material.color = flashColour;
            }
        }
        
    }

    public void SetDisableDuration(float duration)
    {
        m_disableTime = duration;
        Disable();
    }

    public void TakeDamage(int damage)
    {
        // flashes the material and decrements the health
        m_flashing = true;
        m_flashTimer = 0.0f;
        health -= damage;
    }

    public void TakeCriticalDamage(int damage)
    {
        // flashes the material and decrements the health
        m_critical = true;
        m_flashTimer = 0.0f;
        health -= damage;
    }

    /// <summary>
    /// Gets a colour value based on the given time.
    /// </summary>
    /// <param name="time">The x value on a cartesian plane.</param>
    /// <returns>Returns a colour value based on the time.</returns>
    private float Flash(float time)
    {
        // y = 4(xt - 0.5)^2
        float value = 4.0f * (flashSpeed * time - 0.5f) * (flashSpeed * time - 0.5f);
        // stops the flashing if the value exceeds 1 becaues it has done a loop
        if (value > 1.0f)
        {
            value = 1.0f;
            m_flashing = false;
            m_critical = false;
        }

        return value;
    }

    virtual protected void Death()
    {
        PlayExplosionEffect();
        m_isDisabled = true;
        DropHealthPack();
        Destroy(gameObject);
    }

    protected void PlayExplosionEffect()
    {
        if (explosionParticle != null)
        {
            explosionParticle.transform.GetChild(0).gameObject.SetActive(true);
            explosionParticle.transform.position = disableObject.position;
            explosionParticle.transform.parent = null;
            //explosionParticle.GetComponent<AudioSource>().Play();
        }
    }

    virtual protected void Disable()
    {
        emp.Play();
        m_isDisabled = true;
        disableObject.isKinematic = false;
    }

    virtual protected void Reboot()
    {
        m_isDisabled = false;
        disableObject.isKinematic = true;
    }

    /// <summary>
    /// Pseudo randomly drops a health pack.
    /// </summary>
    protected void DropHealthPack()
    {
        // determines if a health pack is dropped
        bool drop = false;

        // 0% chance of dropping
        if (healthDropChance <= 0.0f)
        {
            drop = false;
        }
        // 100% chance of dropping
        else if (healthDropChance >= 100.0f)
        {
            drop = true;
        }
        else
        {
            // gets a random number between 0 and 100 inclusive
            float prng = Random.Range(0.0f, 100.0f);
            // applies the health drop chance and checks if the result is on the lower or higher end of the drop chance
            drop = (prng <= 100.0f - healthDropChance) ? false : true;
        }

        if (drop)
        {
            // creates a health pack
            Instantiate(healthDrop, disableObject.position, disableObject.rotation);
        }
    }
}