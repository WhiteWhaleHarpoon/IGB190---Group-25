using MyUtilities;
using UnityEngine;

/// <summary>
/// Controls all of the logic for a health globe. The player must collide with a health globe
/// to pick it up, and on collision it will restore health to them.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    public GameFeedback feedback;
    public string pickupText = "Health Restored!";

    // Varibles to control the pickup movment.
    private float spawnedAt;
    private float effectDuration = 0.6f;
    private Vector3 startPosition;
    private Vector3 goalPosition;
    public static int activeHealthGlobes = 0;

    /// <summary>
    /// Handle all initial setup actions when the globe is created.
    /// </summary>
    private void OnEnable()
    {
        activeHealthGlobes++;
        spawnedAt = Time.time;
        startPosition = transform.position;
        Vector3 point = Random.insideUnitSphere;
        point.y = 0;
        goalPosition = transform.position + point.normalized * GameManager.healthGlobeValues.spawnRadius;
        goalPosition = Utilities.GetValidNavMeshPosition(goalPosition);
    }

    /// <summary>
    /// Handle all cleanup actions when the health globe is destroyed.
    /// </summary>
    private void OnDisable()
    {
        activeHealthGlobes--;
    }

    /// <summary>
    /// Handle all frame-specific updates for the pickup.
    /// </summary>
    private void Update()
    {
        HandleMovement();
        CheckForTimeout();
    }

    /// <summary>
    /// Handle collision actions for the health globe.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (CanPickup(other.GetComponent<Player>()))
        {
            PickupHealth();
        }
    }

    /// <summary>
    /// Check to see if the health globe has timed out, and destroy it if needed.
    /// </summary>
    private void CheckForTimeout ()
    {
        if (Time.time > spawnedAt + GameManager.healthGlobeValues.lifetime)
            Destroy(gameObject);
    }

    /// <summary>
    /// Handle movement for the health globe.
    /// </summary>
    private void HandleMovement ()
    {
        float perc = (Time.time - spawnedAt) / effectDuration;
        if (Time.time < spawnedAt + effectDuration)
            transform.position = Vector3.Lerp(startPosition, goalPosition, perc) + 
                GameManager.assets.smoothInOutCurve.Evaluate(perc) * 1 * Vector3.up;
    }

    /// <summary>
    /// Return true if the player can pickup this health globe, otherwise false.
    /// </summary>
    private bool CanPickup (Player player)
    {
        return (player != null && Time.time > spawnedAt + effectDuration);
    }

    /// <summary>
    /// Pickup the health globe, restoring health and performing all other required actions.
    /// </summary>
    private void PickupHealth ()
    {
        feedback.ActivateFeedback(GameManager.player.gameObject);
        GameManager.player.AddHealth(GameManager.healthGlobeValues.healthGlobeHealthRestore);
        GameManager.events.OnHealthPickedUp.Invoke(GameManager.healthGlobeValues.healthGlobeHealthRestore);
        ObjectPooler.DestroyPooled(gameObject);
        StatusMessageUI.Spawn(transform.position + Vector3.up, pickupText, Color.green);
    }

    /// <summary>
    /// Spawn a health pickup at the specified position.
    /// </summary>
    public static void Spawn (Vector3 position)
    {
        ObjectPooler.InstantiatePooled(GameManager.assets.healthPickup.gameObject, position, Quaternion.identity);
    }
}
