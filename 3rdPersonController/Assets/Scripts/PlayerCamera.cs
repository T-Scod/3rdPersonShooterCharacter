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
    /// Reference to the virtual camera that is locked to the player's shoulder.
    /// </summary>
    [Tooltip("The virtual camera that is locked to the player's shoulder.")]
    public CinemachineFreeLook overShoulderCamera;
    /// <summary>
    /// How sensitive the camera is to input on the x axis.
    /// </summary>
    [Tooltip("How sensitive the camera is to input on the X axis. (Rotation around the Y axis)")]
    public float xSensitivityFree = 0.1f;
    /// <summary>
    /// How sensitive the camera is to input on the y axis.
    /// </summary>
    [Tooltip("How sensitive the camera is to input on the Y axis. (Rotation around the X axis)")]
    public float ySensitivityFree = 0.1f;
    /// <summary>
    /// Minimum rotation around the X axis that the over the shoulder camera will be clamped to.
    /// </summary>
    [Tooltip("Minimum rotation around the X axis that the over the shoulder camera will be clamped to.")]
    public float xSensitivityOver = -0.6f;
    /// <summary>
    /// Maximum rotation around the X axis that the over the shoulder camera will be clamped to.
    /// </summary>
    [Tooltip("Maximum rotation around the X axis that the over the shoulder camera will be clamped to.")]
    public float ySensitivityOver = 1.0f;
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
    public float hipFireTime = 0.0f;
    #endregion
    /// <summary>
    /// Determines if the player is aiming.
    /// </summary>
    [HideInInspector]
    public bool aiming = false;
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
    /// <summary>
    /// The rotation the player is rotating towards.
    /// </summary>
    private Quaternion m_playerTargetRot;
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
        m_playerTargetRot = m_player.localRotation;
    }

    /// <summary>
    /// Calculates the
    /// </summary>
    public void LookRotation()
    {
        if (!aiming)
        {
            m_playerTargetRot = Quaternion.Euler(0.0f, freeCamera.m_XAxis.Value, 0.0f);

            Vector3 direction = m_player.GetComponent<Rigidbody>().velocity;
            if (direction.sqrMagnitude > 3.2f)
            {
                direction = Vector3.ProjectOnPlane(direction, Vector3.up);
                direction.Normalize();
                if (direction.x == 0.0f)
                {
                    direction.x += 0.001f;
                }

                m_model.localRotation = Quaternion.Slerp(m_model.localRotation, Quaternion.LookRotation(direction), smoothSpeed * Time.deltaTime);
            }
        }
        else
        {
            m_playerTargetRot = Quaternion.Euler(0.0f, overShoulderCamera.m_XAxis.Value, 0.0f);
        }

        m_player.localRotation = Quaternion.Slerp(m_player.localRotation, m_playerTargetRot, smoothSpeed * Time.deltaTime);
        // updates whether the cursor is locked or not
        UpdateCursorLock();
    }

    public void ResetPlayerModelRotation()
    {
        m_model.localRotation = Quaternion.identity;
    }

    public void ResetPlayerRotation()
    {
        if (aiming)
        {
            m_player.localRotation = Quaternion.Euler(0.0f, overShoulderCamera.m_XAxis.Value, 0.0f);
        }
        else
        {
            m_player.localRotation = Quaternion.Euler(0.0f, freeCamera.m_XAxis.Value, 0.0f);
        }
        m_playerTargetRot = m_player.localRotation;
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