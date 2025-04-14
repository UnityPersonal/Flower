using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBone : MonoBehaviour
{
    public Transform toFollow;
    public float currentAngle;
    public float rotationSpeed = 100f;
    
    void Start()
    {
        
    }

    void Update()
    {
        currentAngle += rotationSpeed * Time.deltaTime;
    }
}
