using MyUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// Projectile class handles all logic for a projectile in the game. A projectile
/// is an object that can move, collide with units, move to specific locations etc.
/// </summary>
public class Projectile : MonoBehaviour
{
    private enum MovementMode
    {
        None,
        Forward,
        Arc,
        Target
    }

    public bool isTemplate = true;
    public string category;

    // Projectile values.
    private MovementMode currentMovementMode = MovementMode.None;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Unit targetUnit;
    private float lifetime;
    private float destroyAt = 999999;
    private float speed;
    private float arcHeight = 0;
    private float heightOffset = 0;

    // Store the owner, faction, and engine seperately, so collisions can still
    // be handled even if the 'owner' is destroyed.
    private Unit owner;
    private Unit.Faction ownerFaction;
    private IVisualCodeHandler engineHandler;

    // Constant properties.
    private const float DEFAULT_LIFETIME = 10;
    private const float PROJECTILE_HEIGHT_SMOOTHING = 10;

    /// <summary>
    /// Perform initial setup for the projectile.
    /// </summary>
    public void Setup(IVisualCodeHandler handler)
    {
        AssignProjectileOwnership(handler);
        SetInitialHeightOffset();
        SetLifetime(DEFAULT_LIFETIME);
    }

    /// <summary>
    /// Handle all frame-specific projectile updates.
    /// </summary>
    private void Update()
    {
        UpdateTargetPosition();
        HandleMovement();
    }

    /// <summary>
    /// Assign ownership data for this projectile.
    /// </summary>
    private void AssignProjectileOwnership(IVisualCodeHandler handler)
    {
        this.owner = handler.GetOwner();
        this.engineHandler = handler;
        if (owner != null) ownerFaction = owner.GetFaction();
    }

    /// <summary>
    /// Set up the initial position and height offset for the projectile.
    /// </summary>
    public void SetInitialHeightOffset()
    {
        
        startPosition = transform.position;
        heightOffset = transform.position.y - 
            Utilities.GetValidNavMeshPosition(transform.position).y;
        targetPosition = transform.position + Vector3.up * heightOffset;
    }

    /// <summary>
    /// Handle all movement actions for the projectile.
    /// </summary>
    private void HandleMovement ()
    {
        // Check for a timeout.
        if (Time.time > destroyAt) 
            Timeout();

        // Handle movement.
        switch (currentMovementMode)
        {
            case MovementMode.Forward:
                MoveForwards();
                break;

            case MovementMode.Arc:
                MoveInArc();
                break;

            case MovementMode.Target:
                MoveToGoal();
                break;
        }
    }

    /// <summary>
    /// Return a height-adjusted position for the projectile, keeping it near the ground
    /// (even if it goes up or down terrain).
    /// </summary>
    public Vector3 HeightAdjustedTransform()
    {
        Vector3 position = Utilities.GetValidNavMeshPosition(transform.position);
        return new Vector3(transform.position.x, position.y + heightOffset, transform.position.z);
    }

    /// <summary>
    /// If the projectile is targeting a unit, update the target location to the location
    /// of that unit.
    /// </summary>
    private void UpdateTargetPosition()
    {
        if (targetUnit != null) targetPosition = targetUnit.transform.position;
    }

    /// <summary>
    /// Move the projectile straight (for the frame).
    /// </summary>
    private void MoveForwards ()
    {
        float distanceToTravel = speed * Time.deltaTime;
        Vector3 newPosition = transform.position + transform.forward * distanceToTravel;
        transform.position = newPosition;
        transform.position = Vector3.Lerp(transform.position, HeightAdjustedTransform(), 
            Time.deltaTime * PROJECTILE_HEIGHT_SMOOTHING);
    }

    /// <summary>
    /// Move the projectile towards the target goal.
    /// </summary>
    private void MoveToGoal ()
    {
        // Calculate distance to travel, and total remaining distance.
        float distanceToTravel = speed * Time.deltaTime;
        Vector3 diff = targetPosition - transform.position;
        diff.y = 0;
        float remainingDistance = diff.magnitude;

        // If the projectile has reached the goal, trigger goal events and stop moving.
        if (distanceToTravel > remainingDistance)
        {
            currentMovementMode = MovementMode.None;
            GoalReached();
        }

        // Otherwise, move the projectile.
        else
        {
            transform.position += (targetPosition - transform.position).normalized * distanceToTravel;
            transform.position = Vector3.Lerp(transform.position, HeightAdjustedTransform(),
            Time.deltaTime * PROJECTILE_HEIGHT_SMOOTHING);
        }
    }

    /// <summary>
    /// Move the projectile in its arc (for the frame).
    /// </summary>
    private void MoveInArc ()
    {
        float perc = 1 - (destroyAt - Time.time) / lifetime;
        transform.position = Vector3.Lerp(startPosition, targetPosition, perc) + 
            Vector3.up * arcHeight * GameManager.assets.smoothInOutCurve.Evaluate(perc);
        if (perc >= 1) GoalReached();
    }

    /// <summary>
    /// Handle all collision logic for the projectile.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        Unit unit = other.GetComponent<Unit>();
        if (CanCollideWithWall() && IsWall(other))
        {
            WallCollision();
            DestroyProjectile();
        }
        else if (IsValidEnemy(unit)) 
        {
            ProjectileCollision(unit);
        }
    }

    /// <summary>
    /// Return true if the given unit is a valid enemy target, otherwise false.
    /// </summary>
    private bool IsValidEnemy (Unit unit)
    {
        return (unit != null && unit.GetFaction() != ownerFaction);
    }

    /// <summary>
    /// Return true if the projectile can collide with a wall, otherwise false.
    /// </summary>
    private bool CanCollideWithWall ()
    {
        return (arcHeight == 0);
    }

    /// <summary>
    /// Return true if the collider is a wall, otherwise returen false.
    /// </summary>
    private bool IsWall (Collider other)
    {
        return (other.gameObject.layer == LayerMask.NameToLayer("Wall"));
    }

    /// <summary>
    /// Rotate the projectile by the given amount (in degrees) on the Y-axis (up-down).
    /// </summary>
    public void Rotate(float amount)
    {
        transform.Rotate(Vector3.up, amount, Space.World);
    }

    /// <summary>
    /// Set the lifetime of the projectile (the time until it is auto-destroyed).
    /// </summary>
    public void SetLifetime(float lifetime)
    {
        this.lifetime = lifetime;
        this.destroyAt = Time.time + lifetime;
    }

    /// <summary>
    /// Have this projectile face towards a unit.
    /// </summary>
    public void FaceProjectileTowardsPoint (Vector3 point)
    {
        point.y = transform.position.y;
        transform.LookAt(point);
    }

    /// <summary>
    /// Move this projectile forwards at a set speed.
    /// </summary>
    public void MoveProjectileForwards (float speed)
    {
        currentMovementMode = MovementMode.Forward;
        targetPosition = transform.position + transform.forward * 1000;
        this.speed = speed;
        this.startPosition = transform.position;
    }

    /// <summary>
    /// Move this projectile towards a goal point at a set speed.
    /// </summary>
    public void MoveProjectileTowardsPoint (Vector3 point, float speed)
    {
        currentMovementMode = MovementMode.Target;
        targetPosition = point;
        this.speed = speed;
    }

    /// <summary>
    /// Move this projectile towards a goal in an arc.
    /// </summary>
    public void MoveProjectileInArcTowardsPoint (Vector3 point, float time, float arcHeight)
    {
        targetPosition = point;
        currentMovementMode = MovementMode.Arc;
        this.speed = Vector3.Distance(point, transform.position) / time;
        this.arcHeight = arcHeight;
        SetLifetime(time);
    }

    /// <summary>
    /// Destroy the projectile, performing no further actions.
    /// </summary>
    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Perform all required actions when the projectile collides with a wall. Call
    /// all required events, but keep the projectile 'alive'.
    /// </summary>
    private void WallCollision ()
    {
        Dictionary<string, object> presets = new Dictionary<string, object>
            {
                { LogicEngine.PRESET_CASTING_UNIT, owner },
                { LogicEngine.PRESET_EVENT_PROJECTILE, this }
            };
        engineHandler.GetEngine().TriggerEvent(presets, LogicEngine.EVENT_PROJECTILE_COLLIDES_WITH_TERRAIN);
    }

    /// <summary>
    /// Perform all required actions when the projectile collides with an enemy. Call
    /// all required events, but keep the projectile 'alive'.
    /// </summary>
    private void ProjectileCollision (Unit enemy)
    {
        Dictionary<string, object> presets = new Dictionary<string, object>
        {
            { LogicEngine.PRESET_CASTING_UNIT, owner },
            { LogicEngine.PRESET_EVENT_PROJECTILE, this },
            { LogicEngine.PRESET_COLLIDING_UNIT, enemy },
        };
        engineHandler.GetEngine().TriggerEvent(presets, LogicEngine.EVENT_PROJECTILE_OWNED_COLLIDES_WITH_UNIT);
    }

    /// <summary>
    /// Perform all required actions when the projectile reaches its goal. Call
    /// all required events, then destroy the projectile.
    /// </summary>
    private void GoalReached ()
    {
        Dictionary<string, object> presets = new Dictionary<string, object>
        {
            { LogicEngine.PRESET_CASTING_UNIT, owner },
            { LogicEngine.PRESET_EVENT_PROJECTILE, this },
            { LogicEngine.PRESET_GOAL_UNIT, targetUnit },
            { LogicEngine.PRESET_GOAL_POSITION, targetPosition }
        };
        engineHandler.GetEngine().TriggerEvent(presets, LogicEngine.EVENT_PROJECTILE_REACHES_GOAL);
        DestroyProjectile();
    }

    /// <summary>
    /// Perform all required actions when the projectile times out. Call
    /// all required events, then destroy the projectile.
    /// </summary>
    private void Timeout ()
    {
        Dictionary<string, object> presets = new Dictionary<string, object>
        {
            { LogicEngine.PRESET_CASTING_UNIT, owner },
            { LogicEngine.PRESET_EVENT_PROJECTILE, this }
        };
        engineHandler.GetEngine().TriggerEvent(presets, LogicEngine.EVENT_PROJECTILE_TIMES_OUT);
        DestroyProjectile();
    }
}
