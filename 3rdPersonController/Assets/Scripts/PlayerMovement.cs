using UnityEngine;
using System;

/// <summary>
/// This class contains all basic movement settings.
/// </summary>
[Serializable]
public class PlayerMovement
{
    /// <summary>
    /// Movement direction or type.
    /// </summary>
    public enum CurrentSpeedType
    {
        Forward,
        Backward,
        Strafe,
        Run,
        Still
    }

    #region Variables
    #region Public
    /// <summary>
    /// The speed the player moves while heading forward.
    /// </summary>
    [Tooltip("Speed while moving forward.")]
    public float forwardSpeed = 8.0f;
    /// <summary>
    /// The speed the player moves while heading backward.
    /// </summary>
    [Tooltip("Speed while moving backward.")]
    public float backwardSpeed = 4.0f;
    /// <summary>
    /// The speed the player moves while heading left or right.
    /// </summary>
    [Tooltip("Speed while moving left or right.")]
    public float strafeSpeed = 4.0f;
    /// <summary>
    /// Multiplies the current direction speed when running.
    /// </summary>
    [Tooltip("Multiplies the current direction speed when running.")]
    public float runMultiplier = 2.0f;
    /// <summary>
    /// Multiplies the current direction speed when aiming.
    /// </summary>
    [Tooltip("Multiplies the current direction speed when aiming.")]
    public float aimMultiplier = 0.5f;
    /// <summary>
    /// How quickly the player animation changes from one to another.
    /// </summary>
    [Tooltip("How quickly the player animation changes from one to another.")]
    public float animationSpeed = 10.0f;
    #endregion
    #region Hidden
    /// <summary>
    /// The current speed of the player.
    /// </summary>
    [HideInInspector]
    public float currentTargetSpeed = 30.0f;
    /// <summary>
    /// Current movement direction or type.
    /// </summary>
    [HideInInspector]
    public CurrentSpeedType currentSpeedType;
    #endregion
    /// <summary>
    /// Reference to the player.
    /// </summary>
    private PlayerController m_player;
    #endregion

    /// <summary>
    /// Initialises the reference to the player.
    /// </summary>
    /// <param name="player">Reference to the player.</param>
    public void Init(PlayerController player)
    {
        m_player = player;
    }

    /// <summary>
    /// Affects the current movement speed based on the direction of the input.
    /// </summary>
    /// <param name="input">The direction of the player's input.</param>
    public void UpdateTargetSpeed(Vector2 input)
    {
        // checks if no input is given and exits accordingly
        if (input == Vector2.zero)
        {
            currentTargetSpeed = 0;
            currentSpeedType = CurrentSpeedType.Still;
            return;
        }

        // checks if the player is aiming
        if (m_player.cameraSettings.aiming/* || m_player.cameraSettings.hipFireTimer < m_player.cameraSettings.hipFireTime*/)
        {
            // checks if the movement is left or right
            if (input.x > 0 || input.x < 0)
            {
                currentSpeedType = CurrentSpeedType.Strafe;
                currentTargetSpeed = strafeSpeed;
            }
            // checks if the movement is backwards or forwards
            if (input.y < 0)
            {
                currentSpeedType = CurrentSpeedType.Backward;
                currentTargetSpeed = backwardSpeed;
            }
            if (input.y > 0)
            {
                currentSpeedType = CurrentSpeedType.Forward;
                currentTargetSpeed = forwardSpeed;
            }
        }
        // sets the speed type to forward if the player is not aiming and is giving input
        else
        {
            if (input.x != 0.0f || input.y != 0.0f)
            {
                currentSpeedType = CurrentSpeedType.Forward;
                currentTargetSpeed = forwardSpeed;
            }
        }

        // checks if the run key was pressed
        if (Input.GetButton("Run"))
        {
            // multiplies the current movement speed
            currentSpeedType = CurrentSpeedType.Run;
            currentTargetSpeed *= runMultiplier;
        }
    }
}