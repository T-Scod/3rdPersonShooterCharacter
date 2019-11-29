using UnityEngine;

/// <summary>
/// Controls the player's movement, combat and health.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    #region Public
    /// <summary>
    /// Contains the settings controlling movement.
    /// </summary>
    public PlayerMovement movementSettings = new PlayerMovement();
    /// <summary>
    /// Contains the settings controlling camera movement.
    /// </summary>
    public PlayerCamera cameraSettings = new PlayerCamera();
    /// <summary>
    /// Contains the settings controlling combat.
    /// </summary>
    public PlayerCombat combatSettings = new PlayerCombat();
    #endregion
    #region Private
    /// <summary>
    /// Reference to the player's rigidbody.
    /// </summary>
    private Rigidbody m_rigidBody;
    /// <summary>
    /// Reference to the camera.
    /// </summary>
    private Transform m_cam;
    /// <summary>
    /// Reference to the animator.
    /// </summary>
    private Animator m_anim;
    #endregion
    #endregion

    /// <summary>
    /// Gets the components from the player and initialises the settings.
    /// </summary>
    private void Start()
    {
        m_rigidBody = GetComponent<Rigidbody>();
        m_cam = Camera.main.transform;

        movementSettings.Init(this);
        cameraSettings.Init(transform);
        combatSettings.Init();
        // passes the animator onto the settings
        m_anim = GetComponentInChildren<Animator>();
        combatSettings.animator = m_anim;
    }

    /// <summary>
    /// Handles combat input.
    /// </summary>
    private void Update()
    {
        // allows the player to break out into scene view
        if (Input.GetButtonDown("Exit"))
        {
            Debug.Break();
        }

        // checks if the player is now aiming
        if (Input.GetButtonDown("Aim"))
        {
            cameraSettings.aiming = true;

            // sets the animation to aiming
            m_anim.SetBool("Aim", true);
            m_anim.SetLayerWeight(1, 0.0f);
            m_anim.SetLayerWeight(2, 1.0f);
            combatSettings.crosshairObject.GetComponent<Animator>().SetBool("Aim", true);

            // moves the player to the direction the camera is pointing
            transform.rotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, cameraSettings.freeCamera.m_XAxis.Value, transform.localEulerAngles.z));
            cameraSettings.ResetPlayerRotation();

            // transitions from the free camera to the over the shoulder camera
            cameraSettings.overShoulderCamera.gameObject.SetActive(true);
            cameraSettings.overShoulderCamera.m_XAxis.Value = cameraSettings.freeCamera.m_XAxis.Value;
            cameraSettings.overShoulderCamera.m_YAxis.Value = cameraSettings.freeCamera.m_YAxis.Value;
            cameraSettings.freeCamera.gameObject.SetActive(false);
        }
        // checks if the player is no long aiming
        else if (Input.GetButtonUp("Aim"))
        {
            cameraSettings.aiming = false;

            // sets the animation to not aiming if the player is not shooting
            if (combatSettings.combatState != PlayerCombat.CombatState.Shooting)
            {
                m_anim.SetBool("Aim", false);
                m_anim.SetLayerWeight(1, 1.0f);
                m_anim.SetLayerWeight(2, 0.0f);
            }
            else
            {
                cameraSettings.previousRot = transform.localRotation;
            }
            combatSettings.crosshairObject.GetComponent<Animator>().SetBool("Aim", false);

            // transitions from the over the shoulder camera to the free camera
            cameraSettings.freeCamera.gameObject.SetActive(true);
            cameraSettings.freeCamera.m_XAxis.Value = cameraSettings.overShoulderCamera.m_XAxis.Value;
            cameraSettings.freeCamera.m_YAxis.Value = cameraSettings.overShoulderCamera.m_YAxis.Value;
            cameraSettings.overShoulderCamera.gameObject.SetActive(false);
        }

        // determines the current combat state
        combatSettings.ChargeShot();
        // rotates the camera based on the player
        RotateView();
    }

    /// <summary>
    /// Moves the player based on input.
    /// </summary>
    private void FixedUpdate()
    {
        // gets the input
        Vector2 input = new Vector2
        {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };

        // updates the movement speed based on the input
        movementSettings.UpdateTargetSpeed(input);

        Vector3 desiredMove = m_cam.forward;
        // checks if input was given
        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon))
        {
            // always move along the camera forward as it is the direction that it being aimed at
            desiredMove = m_cam.forward * input.y + m_cam.right * input.x;

            // only normalises if the magnitude is greater than 1
            // if it less than 1 there it is not normalised which means that there is smoothing on the input
            desiredMove = Vector3.ProjectOnPlane(desiredMove, Vector3.up);
            if (desiredMove.magnitude > 1.0f)
            {
                desiredMove.Normalize();
            }

            // multiplies the desired move direction
            desiredMove *= movementSettings.currentTargetSpeed * Time.deltaTime;
            if (cameraSettings.aiming)
            {
                desiredMove *= movementSettings.aimMultiplier;
            }
            // applies the vector to the player as a force
            m_rigidBody.AddForce(desiredMove, ForceMode.Impulse);
        }

        // passes the speed of the player to the animator so that it can blend between idle, walking and running
        float animSpeed = m_anim.GetFloat("Speed");
        float targetSpeed = (movementSettings.currentTargetSpeed / movementSettings.forwardSpeed) *
            ((movementSettings.currentSpeedType == PlayerMovement.CurrentSpeedType.Backward) ? -1.0f : 1.0f);
        animSpeed = Mathf.Lerp(animSpeed, targetSpeed, movementSettings.animationSpeed * Time.deltaTime);
        m_anim.SetFloat("Speed", animSpeed);
    }

    /// <summary>
    /// Rotates the camera based on the player.
    /// </summary>
    private void RotateView()
    {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon)
        {
            return;
        }

        cameraSettings.LookRotation();

        if (cameraSettings.aiming)
        {
            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;
            // rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            m_rigidBody.velocity = velRotation * m_rigidBody.velocity;
        }
    }
}