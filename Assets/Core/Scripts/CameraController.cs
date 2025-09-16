using UnityEngine;

/// <summary>
/// Handles all camera movement and actions. Keeps a constant distance between the camera
/// and the player.
/// </summary>
public class CameraController : MonoBehaviour
{
    private GameObject target;
    public float cameraMovementSmoothing = 30.0f;
    public float cameraZoomSmoothing = 2.0f;
    private float currentOffsetDistance;
    private float desiredOffsetDistance;
    private Vector3 offsetDirection;
    
    /// <summary>
    /// Calculate the default offset for the camera.
    /// </summary>
    void Start()
    {
        SetTarget(GameManager.player.gameObject);
        offsetDirection = (transform.position - target.transform.position).normalized;
        currentOffsetDistance = (transform.position - target.transform.position).magnitude;
        desiredOffsetDistance = currentOffsetDistance;
    }

    /// <summary>
    /// Constantly update the camera to keep the same offset between the player and the camera.
    /// </summary>
    void Update()
    {
        FollowTarget();
    }

    /// <summary>
    /// Set the zoom distance of the camera, allowing the camera to zoom in and out as needed.
    /// </summary>
    private void SetOffsetDistance (float distance)
    {
        desiredOffsetDistance = distance;
    }

    /// <summary>
    /// Set the target of the camera, and store the initial offset.
    /// </summary>
    private void SetTarget (GameObject target)
    {
        this.target = target;
    }

    /// <summary>
    /// Move smoothly towards the target. 
    /// </summary>
    private void FollowTarget ()
    {
        currentOffsetDistance = Mathf.Lerp(currentOffsetDistance, desiredOffsetDistance, 
            Time.deltaTime * cameraZoomSmoothing);
        transform.position = Vector3.Lerp(transform.position, target.transform.position + 
            offsetDirection * currentOffsetDistance, Time.deltaTime * cameraMovementSmoothing);
    }
}
