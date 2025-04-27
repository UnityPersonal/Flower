using UnityEditor;
using UnityEngine;

namespace GolemKin.Swarm
{
    [CustomEditor(typeof(BoidFlockingSystem))]
    public class BoidFlockingSystemEditor : Editor
    {
        private SerializedProperty movementModeProp;
        private SerializedProperty boidPrefabProp;
        private SerializedProperty numberOfBoidsProp;
        private SerializedProperty spawnAreaProp;

        private SerializedProperty viewRadiusProp;
        private SerializedProperty separationWeightProp;
        private SerializedProperty alignmentWeightProp;
        private SerializedProperty cohesionWeightProp;
        private SerializedProperty maxSpeedProp;
        private SerializedProperty maxForceProp;

        private SerializedProperty homingThresholdProp;
        private SerializedProperty homingWeightProp;

        private SerializedProperty boundarySizeProp;
        private SerializedProperty boundaryForceProp;

        private SerializedProperty waypointsProp;
        private SerializedProperty waypointThresholdProp;
        private SerializedProperty waypointAttractionWeightProp;

        private SerializedProperty targetDirectionProp;

        private void OnEnable()
        {
            // Cache the properties
            movementModeProp = serializedObject.FindProperty("movementMode");
            boidPrefabProp = serializedObject.FindProperty("boidPrefab");
            numberOfBoidsProp = serializedObject.FindProperty("numberOfBoids");
            spawnAreaProp = serializedObject.FindProperty("spawnArea");

            viewRadiusProp = serializedObject.FindProperty("viewRadius");
            separationWeightProp = serializedObject.FindProperty("separationWeight");
            alignmentWeightProp = serializedObject.FindProperty("alignmentWeight");
            cohesionWeightProp = serializedObject.FindProperty("cohesionWeight");
            maxSpeedProp = serializedObject.FindProperty("maxSpeed");
            maxForceProp = serializedObject.FindProperty("maxForce");

            homingThresholdProp = serializedObject.FindProperty("homingThreshold");
            homingWeightProp = serializedObject.FindProperty("homingWeight");

            boundarySizeProp = serializedObject.FindProperty("boundarySize");
            boundaryForceProp = serializedObject.FindProperty("boundaryForce");

            waypointsProp = serializedObject.FindProperty("waypoints");
            waypointThresholdProp = serializedObject.FindProperty("waypointThreshold");
            waypointAttractionWeightProp = serializedObject.FindProperty("waypointAttractionWeight");

            targetDirectionProp = serializedObject.FindProperty("targetDirection");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Boid Flocking System", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Author: GolemKin", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Version: 1", EditorStyles.miniLabel);

            // Instructions
            EditorGUILayout.HelpBox(
                "This system simulates boid flocking behavior with waypoint and directional modes. Adjust the settings below to control the boid behavior.",
                MessageType.Info);
            EditorGUILayout.Space();

            // Boid Settings
            EditorGUILayout.LabelField("Boid Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(boidPrefabProp, new GUIContent("Boid Prefab"));
            EditorGUILayout.IntSlider(numberOfBoidsProp, 10, 1000, new GUIContent("Number of Boids"));
            EditorGUILayout.PropertyField(spawnAreaProp, new GUIContent("Spawn Area"));
            EditorGUILayout.Space();

            // Movement Mode
            EditorGUILayout.LabelField("Movement Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(movementModeProp, new GUIContent("Movement Mode"));

            // Depending on the selected movement mode, display relevant fields
            BoidFlockingSystem.MovementMode movementMode =
                (BoidFlockingSystem.MovementMode)movementModeProp.enumValueIndex;

            if (movementMode == BoidFlockingSystem.MovementMode.Waypoint)
            {
                // Show waypoint-related fields
                EditorGUILayout.PropertyField(waypointsProp, new GUIContent("Waypoints"), true);
                EditorGUILayout.Slider(waypointThresholdProp, 0.1f, 50f, new GUIContent("Waypoint Threshold"));
                EditorGUILayout.Slider(waypointAttractionWeightProp, 0.1f, 10f,
                    new GUIContent("Waypoint Attraction Weight"));
            }
            else if (movementMode == BoidFlockingSystem.MovementMode.Directional)
            {
                // Show directional mode-related fields
                EditorGUILayout.PropertyField(targetDirectionProp, new GUIContent("Target Direction"));
            }

            EditorGUILayout.Space();

            // Flocking Settings
            EditorGUILayout.LabelField("Flocking Settings", EditorStyles.boldLabel);
            EditorGUILayout.Slider(viewRadiusProp, 0.1f, 50f, new GUIContent("View Radius"));
            EditorGUILayout.Slider(separationWeightProp, 0f, 5f, new GUIContent("Separation Weight"));
            EditorGUILayout.Slider(alignmentWeightProp, 0f, 5f, new GUIContent("Alignment Weight"));
            EditorGUILayout.Slider(cohesionWeightProp, 0f, 5f, new GUIContent("Cohesion Weight"));
            EditorGUILayout.Slider(maxSpeedProp, 0.1f, 20f, new GUIContent("Max Speed"));
            EditorGUILayout.Slider(maxForceProp, 0.1f, 5f, new GUIContent("Max Force"));
            EditorGUILayout.Space();

            // Homing Settings
            EditorGUILayout.LabelField("Homing Settings", EditorStyles.boldLabel);
            EditorGUILayout.Slider(homingThresholdProp, 0.1f, 50f, new GUIContent("Homing Threshold"));
            EditorGUILayout.Slider(homingWeightProp, 0.1f, 10f, new GUIContent("Homing Weight"));
            EditorGUILayout.Space();

            // Boundary Settings
            EditorGUILayout.LabelField("Boundary Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(boundarySizeProp, new GUIContent("Boundary Size"));
            EditorGUILayout.Slider(boundaryForceProp, 0.1f, 10f, new GUIContent("Boundary Force"));
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
