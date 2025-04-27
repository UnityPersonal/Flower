using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GolemKin.Swarm
{
    public class BoidFlockingSystem : MonoBehaviour
    {
        public enum MovementMode
        {
            Base, // Base flocking behavior (separation, alignment, cohesion)
            Waypoint, // Move towards defined waypoints
            Directional // Move in a specific direction (targetDirection)
        }

        [Header("Boid Settings")] [SerializeField]
        private GameObject boidPrefab; // Exposed in Inspector

        [SerializeField] private int numberOfBoids = 500; // Exposed in Inspector
        [SerializeField] private Vector3 spawnArea = new Vector3(50, 50, 50); // Exposed in Inspector

        [Header("Movement Mode Settings")] [SerializeField]
        private MovementMode movementMode = MovementMode.Base; // Exposed in Inspector

        [SerializeField] private Transform[] waypoints; // Exposed in Inspector
        [SerializeField] private float waypointThreshold = 5f; // Exposed in Inspector
        [SerializeField] private Vector3 targetDirection = Vector3.forward; // Exposed in Inspector

        [Header("Flocking Settings")] [SerializeField]
        private float viewRadius = 5f; // Exposed in Inspector

        [SerializeField] private float separationWeight = 1.5f; // Exposed in Inspector
        [SerializeField] private float alignmentWeight = 1f; // Exposed in Inspector
        [SerializeField] private float cohesionWeight = 1f; // Exposed in Inspector
        [SerializeField] private float maxSpeed = 5f; // Exposed in Inspector
        [SerializeField] private float maxForce = 0.5f; // Exposed in Inspector
        [SerializeField] private float waypointAttractionWeight = 0.5f;

        [Header("Homing Settings")] [SerializeField]
        private float homingThreshold = 20f; // Exposed in Inspector

        [SerializeField] private float homingWeight = 2f; // Exposed in Inspector

        [Header("Boundary Settings")] [SerializeField]
        private Vector3 boundarySize = new Vector3(100, 100, 100); // Exposed in Inspector

        [SerializeField] private float boundaryForce = 1.0f; // Exposed in Inspector

        private NativeArray<float3> waypointPositions;
        private Transform[] boids;
        private NativeArray<float3> positions;
        private NativeArray<float3> velocities;
        private NativeArray<float3> newVelocities;
        private int currentWaypointIndex = 0;

        private void Start()
        {
            // Spawn boids dynamically
            SpawnBoids();

            // Initialize NativeArrays
            positions = new NativeArray<float3>(boids.Length, Allocator.Persistent);
            velocities = new NativeArray<float3>(boids.Length, Allocator.Persistent);
            newVelocities = new NativeArray<float3>(boids.Length, Allocator.Persistent);

            // Initialize waypoints as NativeArray or empty array
            if (waypoints != null && waypoints.Length > 0)
            {
                waypointPositions = new NativeArray<float3>(waypoints.Length, Allocator.Persistent);
                for (int i = 0; i < waypoints.Length; i++)
                {
                    waypointPositions[i] = waypoints[i].position;
                }
            }
            else
            {
                // Initialize an empty array to avoid errors in the job system
                waypointPositions = new NativeArray<float3>(0, Allocator.Persistent);
            }
        }

        private void OnDestroy()
        {
            // Dispose of NativeArrays
            if (positions.IsCreated) positions.Dispose();
            if (velocities.IsCreated) velocities.Dispose();
            if (newVelocities.IsCreated) newVelocities.Dispose();
            if (waypointPositions.IsCreated) waypointPositions.Dispose();
        }

        private void Update()
        {
            float3 controllerPosition = transform.position;
            quaternion controllerRotation = transform.rotation;
            for (int i = 0; i < boids.Length; i++)
            {
                positions[i] = boids[i].position;
                velocities[i] = boids[i].forward;
            }

            BoidFlockJob flockJob = new BoidFlockJob
            {
                positions = positions,
                velocities = velocities,
                newVelocities = newVelocities,
                movementMode = movementMode,
                waypointPositions = waypointPositions,
                waypointThreshold = waypointThreshold,
                waypointAttractionWeight = waypointAttractionWeight,
                targetDirection = targetDirection,
                currentWaypointIndex = currentWaypointIndex,
                viewRadius = viewRadius,
                separationWeight = separationWeight,
                alignmentWeight = alignmentWeight,
                cohesionWeight = cohesionWeight,
                homingThreshold = homingThreshold,
                homingWeight = homingWeight,
                maxSpeed = maxSpeed,
                maxForce = maxForce,
                boundarySize = boundarySize,
                boundaryForce = boundaryForce,
                // Pass the controller's transform data to the job
                controllerPosition = controllerPosition,
                controllerRotation = controllerRotation
            };

            JobHandle jobHandle = flockJob.Schedule(boids.Length, 64);
            jobHandle.Complete();

            // Apply new velocities to boids
            for (int i = 0; i < boids.Length; i++)
            {
                Vector3 newDirection = newVelocities[i];

                if (IsValidVector3(newDirection))
                {
                    boids[i].forward = Vector3.Slerp(boids[i].forward, newDirection, Time.deltaTime * 5f);
                    boids[i].position += newDirection * maxSpeed * Time.deltaTime; // Move boid
                }
            }

            if (movementMode == MovementMode.Waypoint && waypoints.Length > 0)
            {
                if (Vector3.Distance(boids[0].position, waypoints[currentWaypointIndex].position) < waypointThreshold)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                }
            }
        }

        private void SpawnBoids()
        {
            boids = new Transform[numberOfBoids];
            for (int i = 0; i < numberOfBoids; i++)
            {
                // Generate random local positions within the defined spawn area
                Vector3 randomLocalPosition = new Vector3(
                    UnityEngine.Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                    UnityEngine.Random.Range(-spawnArea.y / 2, spawnArea.y / 2),
                    UnityEngine.Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
                );

                // Instantiate the boid prefab at the local position and make it a child of this object
                GameObject
                    boidInstance = Instantiate(boidPrefab, transform); // Set this object's transform as the parent

                // Set the boid's local position relative to its parent (this object)
                boidInstance.transform.localPosition = randomLocalPosition;

                // Store the transform in the boids array
                boids[i] = boidInstance.transform;
            }
        }


        private bool IsValidVector3(Vector3 vector)
        {
            return !float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) &&
                   !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y) && !float.IsInfinity(vector.z);
        }


        private void OnDrawGizmos()
        {
            // Draw the boundary box (yellow)
            Gizmos.color = Color.yellow;

            // Set the Gizmos matrix to transform the boundary relative to the controller's position
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, boundarySize); // Draw the boundary relative to the object's local space

            // Reset Gizmos matrix to identity for waypoints to be drawn in world space
            Gizmos.matrix = Matrix4x4.identity;

            // Draw waypoints and lines if we are in Waypoint mode
            if (movementMode == MovementMode.Waypoint && waypoints != null && waypoints.Length > 0)
            {
                Gizmos.color = Color.green;

                // Draw spheres at each waypoint position
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i] != null)
                    {
                        Gizmos.DrawSphere(waypoints[i].position, 1.0f); // Draw sphere at waypoint (in world space)
                    }
                }

                // Draw lines connecting the waypoints
                for (int i = 0; i < waypoints.Length - 1; i++)
                {
                    if (waypoints[i] != null && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position,
                            waypoints[i + 1].position); // Draw line between waypoints
                    }
                }

                // Optionally, connect the last waypoint back to the first for looping paths
                if (waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
                {
                    Gizmos.DrawLine(waypoints[waypoints.Length - 1].position,
                        waypoints[0].position); // Loop the last waypoint back to the first
                }
            }

            // Draw directional arrow if in Directional mode
            if (movementMode == MovementMode.Directional)
            {
                Gizmos.color = Color.blue;

                // Draw a forward direction arrow in the Scene to visualize the movement direction
                Vector3 arrowStart = transform.position; // Use the GameObject's position
                Vector3 arrowEnd = arrowStart + targetDirection.normalized * 10f; // Extend the arrow by 10 units

                Gizmos.DrawLine(arrowStart, arrowEnd); // Draw the direction line
                Gizmos.DrawSphere(arrowEnd, 0.5f); // Draw a sphere at the end of the direction to indicate the point
            }
        }




    }
}
