using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GolemKin.Swarm
{
    public struct BoidFlockJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<float3> velocities;
        public NativeArray<float3> newVelocities;

        public BoidFlockingSystem.MovementMode movementMode;
        [ReadOnly] public NativeArray<float3> waypointPositions;
        public float waypointThreshold;
        public float3 targetDirection;
        public int currentWaypointIndex;

        public float viewRadius;
        public float separationWeight;
        public float alignmentWeight;
        public float cohesionWeight;
        public float homingThreshold;
        public float homingWeight;
        public float maxSpeed;
        public float maxForce;
        public float boundaryForce;
        public float waypointAttractionWeight;

        public float3 boundarySize;

        public float3 controllerPosition; // Controller's position in world space
        public quaternion controllerRotation; // Controller's rotation

        public void Execute(int index)
        {
            float3 currentPosition = positions[index];
            float3 currentVelocity = velocities[index];
            float3 steeringForce = float3.zero;

            // Apply base flocking behavior (separation, alignment, cohesion)
            float3 flockingForce = ApplyBaseFlockingBehavior(currentPosition, currentVelocity, index);

            if (movementMode == BoidFlockingSystem.MovementMode.Waypoint && waypointPositions.Length > 0)
            {
                // Calculate waypoint steering force
                float3 waypointDirection = math.normalize(waypointPositions[currentWaypointIndex] - currentPosition);

                // Apply waypoint attraction weight to reduce/increase attraction to waypoints
                float3 waypointForce = waypointDirection * waypointAttractionWeight;

                // Combine flocking behavior with waypoint steering
                steeringForce = flockingForce + waypointForce;
            }
            else if (movementMode == BoidFlockingSystem.MovementMode.Directional)
            {
                // Use directional steering force
                steeringForce = math.normalize((float3)targetDirection) + flockingForce;
            }
            else
            {
                // Use only base flocking behavior
                steeringForce = flockingForce;
            }

            // Limit the steering force
            steeringForce = math.normalize(steeringForce) * math.min(math.length(steeringForce), maxForce);

            // Apply boundary force
            float3 boundarySteer = GetBoundarySteeringForce(currentPosition);
            steeringForce += boundarySteer * boundaryForce;

            // Update velocity
            currentVelocity += steeringForce;

            // Clamp velocity to maxSpeed
            if (math.length(currentVelocity) > maxSpeed)
            {
                currentVelocity = math.normalize(currentVelocity) * maxSpeed;
            }

            // Store the updated velocity in the NativeArray
            newVelocities[index] = currentVelocity;
        }

        private float3 ApplyBaseFlockingBehavior(float3 currentPosition, float3 currentVelocity, int index)
        {
            float3 separation = float3.zero;
            float3 alignment = float3.zero;
            float3 cohesion = float3.zero;
            int neighborCount = 0;

            // Iterate over all other boids to calculate flocking behavior
            for (int i = 0; i < positions.Length; i++)
            {
                if (i == index) continue;

                float distance = math.distance(currentPosition, positions[i]);

                // Only consider boids within view radius
                if (distance < viewRadius && distance > math.EPSILON)
                {
                    // Separation: steer to avoid crowding local flockmates
                    separation += (currentPosition - positions[i]) / distance;

                    // Alignment: steer towards the average heading of local flockmates
                    alignment += velocities[i];

                    // Cohesion: steer to move towards the average position of local flockmates
                    cohesion += positions[i];

                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                separation /= neighborCount;
                alignment /= neighborCount;
                cohesion /= neighborCount;

                // Normalize the vectors to limit them by the max speed
                separation = math.normalize(separation) * maxSpeed;
                alignment = math.normalize(alignment) * maxSpeed;
                cohesion = math.normalize(cohesion - currentPosition) * maxSpeed;

                // Apply weights to the steering forces
                float3 steeringForce = (separation * separationWeight) + (alignment * alignmentWeight) +
                                       (cohesion * cohesionWeight);

                // If the boid is too far from the center of the flock, apply a homing force
                float3 centerOfMass = ComputeCenterOfMass(index);
                float distanceToFlockCenter = math.distance(currentPosition, centerOfMass);

                if (distanceToFlockCenter > homingThreshold)
                {
                    float3 homingDirection = math.normalize(centerOfMass - currentPosition) * maxSpeed;
                    steeringForce += homingDirection * homingWeight; // Add homing force
                }

                return steeringForce;
            }

            return float3.zero;
        }

        private float3 GetBoundarySteeringForce(float3 position)
        {
            // Transform position to local space
            float3 localPosition = math.mul(controllerRotation, position - controllerPosition);

            float3 steer = float3.zero;

            // Check boundary in local space
            if (localPosition.x > boundarySize.x / 2) steer.x = -1;
            else if (localPosition.x < -boundarySize.x / 2) steer.x = 1;

            if (localPosition.y > boundarySize.y / 2) steer.y = -1;
            else if (localPosition.y < -boundarySize.y / 2) steer.y = 1;

            if (localPosition.z > boundarySize.z / 2) steer.z = -1;
            else if (localPosition.z < -boundarySize.z / 2) steer.z = 1;

            // Convert steer back to world space
            steer = math.mul(math.inverse(controllerRotation), steer);

            return steer;
        }

        private float3 ComputeCenterOfMass(int excludeIndex)
        {
            float3 center = float3.zero;
            int count = 0;

            for (int i = 0; i < positions.Length; i++)
            {
                if (i == excludeIndex) continue;

                center += positions[i];
                count++;
            }

            return count > 0 ? center / count : center;
        }
    }
}
