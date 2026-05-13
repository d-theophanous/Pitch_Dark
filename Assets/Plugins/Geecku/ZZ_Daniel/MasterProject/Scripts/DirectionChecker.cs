using UnityEngine;
using UnityEngine.AI;

namespace Daniel.Master
{

public class DirectionChecker : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform doorTransform;

    [Header("Settings")]
    [Tooltip("How closely movement must align with the next navmesh waypoint (0.5 = ~60 degrees)")]
    public float alignmentThreshold = 0.5f;

    [Tooltip("How often the navmesh path is recalculated (in seconds)")]
    public float pathUpdateInterval = 0.3f;

    private NavMeshPath path;
    private float pathUpdateTimer = 0f;
    private Vector2 correctDirection;
    private bool isMovingCorrectly = false;

    void Start()
    {
        path = new NavMeshPath();
        RecalculatePath();
    }

    void Update()
    {
        // Recalculate path periodically (not every frame, for performance)
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateInterval)
        {
            pathUpdateTimer = 0f;
            RecalculatePath();
        }

            Vector2 inputDirection = GameManager.Instance.Player.Movement;

        // Only check if the player is actually moving
        if (inputDirection.magnitude < 0.1f)
        {
            isMovingCorrectly = false;
            return;
        }

        inputDirection.Normalize();

        // Compare input with the correct navmesh direction
        float dot = Vector2.Dot(inputDirection, correctDirection);

        if (dot >= alignmentThreshold)
        {
            if (!isMovingCorrectly)
            {
                isMovingCorrectly = true;
                Debug.Log("Moving in the right direction toward the door!");
            }
        }
        else
        {
            isMovingCorrectly = false;
        }
    }

    void RecalculatePath()
    {
        // Calculate the NavMesh path from player to door
        bool pathFound = NavMesh.CalculatePath(
            playerTransform.position,
            doorTransform.position,
            NavMesh.AllAreas,
            path
        );

        if (pathFound && path.corners.Length >= 2)
        {
            // The first corner is the player's position, the second is the next waypoint
            Vector3 nextWaypoint = path.corners[1];
            Vector3 toWaypoint3D = nextWaypoint - playerTransform.position;

            // Flatten to 2D (ignore Y)
            correctDirection = new Vector2(toWaypoint3D.x, toWaypoint3D.z).normalized;
            
        }
        else
        {
            // No valid path found, no correct direction
            correctDirection = Vector2.zero;
            Debug.LogWarning("No NavMesh path found to the door!");
        }
    }
        private void OnDrawGizmosSelected()
        {
            if (correctDirection != null)
            {
                foreach (Vector3 corner in path.corners)
                {
                    Gizmos.DrawSphere(corner, 2f);
                }
            }
        }
    }
    
}