using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class Petal : MonoBehaviour
{
    public enum SimulationType
    {
        Spawn,
        Follow,
    }
    SimulationType simulationType = SimulationType.Spawn;
    
    [System.Serializable]
    public enum Type
    {
        Unknown,
        Yellow,
        Orange,
        Purple,
        Red,
    }
    [SerializeField] Type petaltype = Type.Unknown;
    public Type PetalType => petaltype;
    
    public PetalRope rope;

    [Range(0f, 1f)] public float minIndex = 0.1f;
    [Range(0f, 1f)] public float maxIndex = 0.8f;

    public float normalizePosition;

    public float spawnTime;
    
    private void Start()
    {
        transform.localRotation = Quaternion.Euler(new Vector3(Random.Range(0, 180), Random.Range(0,  180), Random.Range(0,  180)));
        spawnTime = Time.time;
    }

    private void Update()
    {
        switch (simulationType)
        {
            case SimulationType.Spawn:
                UpdatePetalSpawnMovement();
                break;
            case SimulationType.Follow:
                UpdatePetalMovement();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int index = 0;
    
    void UpdatePetalSpawnMovement()
    {
        float petalSpeed = PetalSettings.Instance.petalFollowSpeed;
        transform.Translate(Vector3.up * (petalSpeed * Time.deltaTime), Space.World );
        if (spawnTime + 1f < Time.time)
        {
            simulationType = SimulationType.Follow;
        }
    }

    void UpdatePetalMovement()
    {
        float petalSpeed = PetalSettings.Instance.petalFollowSpeed;
        float normalSpeed = PlayerController.LocalPlayer.NormalizedSpeed;
        normalSpeed = Math.Clamp(normalSpeed,0.1f,1f);
        transform.position = 
            Vector3.Lerp(
                transform.position, 
                rope.refPosition, Time.deltaTime * petalSpeed * normalSpeed);
        //particleTransform.forward = -Camera.main.transform.forward;
    }


   
}
