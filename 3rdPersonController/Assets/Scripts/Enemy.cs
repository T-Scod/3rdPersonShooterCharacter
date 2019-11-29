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
    /// <summary>
    /// How fast the enemy flashes when damaged.
    /// </summary>
    [Tooltip("How fast the enemy flashes red when damaged.")]
    public float flashSpeed = 10.0f;
    #endregion
    #region Private
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
    /// <summary>
    /// Reference to the mesh renderer so that the material can be accessed.
    /// </summary>
    private MeshRenderer m_meshRenderer;
    #endregion
    #endregion

    /// <summary>
    /// Gets the mesh renderer component.
    /// </summary>
    private void Awake()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Checks the health of the enemy and whether it is flashing.
    /// </summary>
    protected void Update()
    {
        // checks if the enemy has run out of health and should be destroyed.
        if (health <= 0)
        {
            Destroy(gameObject, 0.3f);
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
            // applies the colour to the enemy
            m_meshRenderer.material.color = flashColour;
        }
    }

    /// <summary>
    /// Decreases the enemy's health and flashes yellow.
    /// </summary>
    /// <param name="damage">Amount of damage taken.</param>
    public void TakeDamage(int damage)
    {
        // flashes the material and decrements the health
        m_flashing = true;
        m_flashTimer = 0.0f;
        health -= damage;
    }

    /// <summary>
    /// Decreases the enemy's health and flashes red.
    /// </summary>
    /// <param name="damage">Amount of damage taken.</param>
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
}