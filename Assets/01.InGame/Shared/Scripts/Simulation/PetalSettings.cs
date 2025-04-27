using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetalSettings : Singleton<PetalSettings>
{
    
    [Header("Petal Rope Settings")]
    [Range(0f, 1f)] public float minIndex = 0.1f;
    [Range(0f, 1f)] public float maxIndex = 0.8f;

    public float minRopeStretchScale = 2;
    public float maxRopeStretchScale = 5;

    [Header("Petal Settings")]
    public float petalFollowSpeed = 1;

    [Header("Petal Animation Settings")]
    public float boneDistanceMin = 0.1f;
    public float boneDistanceMax = 2f;

    public float boneDamperMin = 0.2f;
    public float boneDamperMax = 0.8f;

}
