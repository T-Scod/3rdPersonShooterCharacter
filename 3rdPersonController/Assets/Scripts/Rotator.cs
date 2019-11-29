using UnityEngine;

/// <summary>
/// Rotates an object around an axis.
/// </summary>
public class Rotator : MonoBehaviour
{
    /// <summary>
    /// Different axis.
    /// </summary>
    public enum Axis
    {
        XAxis,
        YAxis,
        ZAxis
    }

    #region Variables
    #region Public
    /// <summary>
    /// Rotation speed.
    /// </summary>
    [Tooltip("Rotation speed.")]
    public float rotateSpeed = 3.0f;
    /// <summary>
    /// Axis that will be rotated around.
    /// </summary>
    [Tooltip("Axis that will be rotated around.")]
    public Axis axis;
    #endregion
    #endregion

    /// <summary>
    /// Rotates around the axis at the rotation speed.
    /// </summary>
    private void Update()
    {
        // rotates around the specified axis
        switch (axis)
        {
            case Axis.XAxis:
                transform.Rotate(transform.right, rotateSpeed * Time.deltaTime);
                break;
            case Axis.YAxis:
                transform.Rotate(transform.up, rotateSpeed * Time.deltaTime);
                break;
            case Axis.ZAxis:
                transform.Rotate(transform.forward, rotateSpeed * Time.deltaTime);
                break;
            default:
                break;
        }
    }
}