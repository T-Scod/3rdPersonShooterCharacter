using Cinemachine;
using UnityEngine;
using System;

/// <summary>
/// Handles the camera transitions and movement.
/// </summary>
[Serializable]
public class PlayerCamera
{
    #region Variables
    #region Public
    /// <summary>
    /// Reference to the free camera that is able to look all around the player.
    /// </summary>
    [Tooltip("The virtual camera that is able to look all around the player.")]
    public CinemachineFreeLook freeCamera;
    /// <summary>
    /// Reference to the free camera that is locked to the player's shoulder.
    /// </summary>
    [Tooltip("The virtual camera that is locked to the player's shoulder.")]
    public CinemachineFreeLook overShoulderCamera;
    /// <summary>
    /// How quickly the player rotation moves towards the target rotation.
    /// </summary>
    [Tooltip("Speed at which the player rotation mvoes towards the target rotation.")]
    public float smoothSpeed = 10.0f;
    /// <summary>
    /// Determines if the cursor should be locked.
    /// </summary>
    [Tooltip("Determines if the cursor is locked.")]
    public bool lockCursor = true;
    #endregion
    /// <summary>
    /// Determines if the player is aiming.
    /// </summary>
    [HideInInspector]
    public bool aiming = false;
    /// <summary>
    /// The previous rotation of the model.
    /// </summary>
    [HideInInspector]
    public Quaternion previousRot;
    #region Private
    /// <summary>
    /// Reference to the player.
    /// </summary>
    private Transform m_player;
    /// <summary>
    /// Reference to the player model.
    /// </summary>
    private Transform m_model;
    /// <summary>
    /// Determines if the cursor is locked.
    /// </summary>
    private bool m_cursorIsLocked = true;
    #endregion
    #endregion

    /// <summary>
    /// Initialises the target rotations based on the player transform.
    /// </summary>
    /// <param name="player">Reference to the player's transform.</param>
    public void Init(Transform player)
    {
        m_player = player;
        m_model = player.GetComponentInChildren<Animator>().transform;
        previousRot = m_model.rotation;
    }

    /// <summary>
    /// Calculates the direction the player and model should be facing.
    /// </summary>
    public void LookRotation()
    {
        // checks if the player is not aiming
        if (!aiming)
        {
            // smooths the player's rotation towards the rotation of the camera
            m_player.localRotation = Quaternion.Slerp(m_player.localRotation, Quaternion.Euler(0.0f, freeCamera.m_XAxis.Value, 0.0f), smoothSpeed * Time.deltaTime);


            if (m_player.GetComponent<PlayerController>().combatSettings.combatState == PlayerCombat.CombatState.Shooting)
            {
                // sets the model's rotation to what it was in the previous frame
                m_model.rotation = previousRot;
            }
            else
            {
                // gets the direction of the player's velocity
                Vector3 direction = m_player.GetComponent<Rigidbody>().velocity;
                // checks if the player is moving
                if (direction.sqrMagnitude > 3.2f)
                {
                    direction = Vector3.ProjectOnPlane(direction, Vector3.up);
                    direction.Normalize();
                    if (direction.x == 0.0f)
                    {
                        direction.x += 0.001f;
                    }

                    // rotates the model towards the direction they are moving and stores the current rotation
                    m_model.rotation = Quaternion.Slerp(m_model.rotation, Quaternion.LookRotation(direction), smoothSpeed * Time.deltaTime);
                    previousRot = m_model.rotation;
                }
                // the player is not moving
                else
                {
                    // sets the model's rotation to what it was in the previous frame
                    m_model.rotation = previousRot;
                }
            }
        }
        // the player is aiming
        else
        {
            // sets the player's rotation to the direction the camera is facing
            m_player.localRotation = Quaternion.Euler(0.0f, overShoulderCamera.m_XAxis.Value, 0.0f);
        }

        // updates whether the cursor is locked or not
        UpdateCursorLock();
    }

    /// <summary>
    /// Resets the player to face towards the direction the camera is facing.
    /// </summary>
    public void ResetPlayerRotation()
    {
        m_player.localRotation = Quaternion.Euler(0.0f, overShoulderCamera.m_XAxis.Value, 0.0f);
        m_model.localRotation = Quaternion.identity;
        previousRot = Quaternion.identity;
    }

    /// <summary>
    /// Sets whether the cursor is locked or not.
    /// </summary>
    /// <param name="value">The value of the cursor lock.</param>
    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {
            // force unlock the cursor if the user disable the cursor locking helper
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Updates the cursor lock if it should be locked.
    /// </summary>
    public void UpdateCursorLock()
    {
        // checks if the cursor should be properly locked
        if (lockCursor)
        {
            InternalLockUpdate();
        }
    }

    /// <summary>
    /// Properly locks the cursor.
    /// </summary>
    private void InternalLockUpdate()
    {
        // unlocks the cursor when a user tries to escape
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            m_cursorIsLocked = false;
        }
        // relocks the cursor when the game is focused on
        else if (Input.GetMouseButtonUp(0))
        {
            m_cursorIsLocked = true;
        }

        // sets the lock state of the cursor
        if (m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}