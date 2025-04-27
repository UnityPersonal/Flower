
Swarm System Documentation
==============================
Swarm Flocking System - Version 1.0
Author: GolemKin

**IMPORTANT** Unity Math Library is required for this package to work. Please ensure that you have the Unity.Mathematics package installed in your project. You can find it in the Unity Package Manager under the name "Mathematics"., You can also run the Swarm Installer in the tools menu to easily install it

Overview:
The Swarm Flocking System simulates the behavior of boids (birds, fish, etc.) in a flock, using flocking behavior such as separation, alignment, and cohesion. This system supports both waypoint and directional movement modes, allowing the boids to follow a set of waypoints or a predefined direction.

Instructions:
1. Add the SwarmFlockingSystem script to a GameObject in your Unity scene.
2. Assign a boid prefab that represents an individual boid in the flock.
3. Adjust the number of boids and the spawn area for the initial placement of boids.
4. Choose between 'Base', 'Waypoint', or 'Directional' modes:
   - Waypoint: Boids will follow a series of waypoints.
   - Directional: Boids will move in a specific direction.
   - Base: Boids will move according to basic flocking behaviors.
5. Fine-tune flocking settings such as view radius, separation weight, alignment weight, cohesion weight, max speed, and max force.
6. Adjust homing settings if using waypoint or base mode to control how boids regroup if they drift too far.
7. Define a boundary to keep boids inside a specified area and apply boundary force to steer them back if they leave.
8. Use the custom editor to visually set up and adjust settings using sliders, and see fields dynamically hidden based on the selected mode.

Movement Modes:
- Base: Standard boid flocking behavior where each boid moves based on separation, alignment, and cohesion.
- Waypoint: Boids will move toward a set of waypoints. Once a waypoint is reached, they will move to the next waypoint.
  - Waypoint Threshold: The minimum distance to a waypoint for a boid to consider it reached.
  - Waypoint Attraction Weight: Controls how strongly boids are attracted to the waypoint compared to flocking behavior.
- Directional: Boids move in a fixed direction.
  - Target Direction: The vector representing the direction the boids will follow.

Flocking Settings:
- View Radius: The radius within which boids will react to each other.
- Separation Weight: Controls how much boids avoid each other.
- Alignment Weight: Controls how much boids align their movement with others.
- Cohesion Weight: Controls how much boids try to move toward the center of the group.
- Max Speed: The maximum speed a boid can travel.
- Max Force: The maximum force applied to boids when adjusting their velocity.

Homing Settings:
- Homing Threshold: The distance a boid can move away from the flock center before being "called" back.
- Homing Weight: Controls how strongly a boid is pulled back toward the center if it strays too far.

Boundary Settings:
- Boundary Size: The size of the boundary that confines the boids.
- Boundary Force: The force used to steer boids back when they approach or cross the boundary.

Custom Editor:
The SwarmFlockingSystem includes a custom editor for ease of use:
- Sliders are provided for adjusting numerical values like boid count, weights, and forces.
- Fields are hidden or shown dynamically based on the selected movement mode.
- The editor features a clean layout with headers and instructions to guide users through the setup.

Best Practices:
- Start with default flocking settings and experiment with the weights to achieve desired behaviors.
- For a tighter flock, increase cohesion and alignment, and lower separation.
- Use the waypoint mode to create paths for boids to follow.
- Keep the boundary size large enough to allow natural movement but small enough to avoid boids flying too far off.

API Reference:
----------------
Class: BoidFlockingSystem

- void Start()
  Initializes boid spawning and native arrays. It sets up the flock based on user settings.

- void Update()
  The main update loop where boid positions and velocities are updated. It handles the application of flocking forces and manages waypoint transitions.

- void SpawnBoids()
  Dynamically spawns boids into the scene based on the number specified in the inspector.

- MovementMode Enum
  Defines the different movement modes: Base, Waypoint, and Directional.

- NativeArray<float3> positions
  Holds the current positions of all boids.

- NativeArray<float3> velocities
  Stores the current velocities of all boids.

- NativeArray<float3> waypointForces
  Used to store the forces acting on boids toward their next waypoint.

Class: BoidFlockJob (IJobParallelFor)

- Execute(int index)
  Runs per boid to apply the flocking behavior, including separation, alignment, cohesion, and waypoint forces.

- float3 ApplyBaseFlockingBehavior(float3 currentPosition, float3 currentVelocity, int index)
  Handles the separation, alignment, and cohesion logic for each boid in relation to its neighbors.

- float3 GetBoundarySteeringForce(float3 position)
  Calculates a force to steer boids back within the defined boundary.

Properties:

- float waypointThreshold
  The distance at which a boid considers it has "reached" a waypoint and moves to the next.

- float waypointAttractionWeight
  Determines how strongly the boids are attracted to the waypoint compared to their flocking behavior.

- float viewRadius
  Defines the distance within which boids react to other boids for flocking behavior.

- float separationWeight, alignmentWeight, cohesionWeight
  Control the relative strength of separation, alignment, and cohesion behaviors.

- float maxSpeed, maxForce
  Limit the speed and force of the boid movements.

Version History:
1.0 - Initial release


Support:
--------
For any issues, questions, or support requests, please contact:
- Email: support@golemkin.com
