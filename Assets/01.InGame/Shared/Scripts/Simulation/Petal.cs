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
        Base,
        Follow,
    }
    public SimulationType simulationType;
    
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
    public Transform particleTransform;

    [Range(0f, 1f)] public float minIndex = 0.1f;
    [Range(0f, 1f)] public float maxIndex = 0.8f;

    public float normalizePosition;
   
    
    private void Start()
    {
        particleTransform.localRotation = Quaternion.Euler(new Vector3(Random.Range(0, 180), Random.Range(0,  180), Random.Range(0,  180))); 
    }

    private void Update()
    {
        switch (simulationType)
        {
            case SimulationType.Base:
                UpdatePetalMovement();
                break;
            case SimulationType.Follow:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int index = 0;

    void UpdatePetalMovement()
    {
        float petalSpeed = PetalSettings.Instance.petalFollowSpeed;
        float normalSpeed = PlayerController.localPlayer.NormalizedSpeed;
        normalSpeed = Math.Clamp(normalSpeed,0.1f,1f);
        particleTransform.position = 
            Vector3.Lerp(
                particleTransform.position, 
                rope.refPosition, Time.deltaTime * petalSpeed * normalSpeed);
        //particleTransform.forward = -Camera.main.transform.forward;
    }


   
}
