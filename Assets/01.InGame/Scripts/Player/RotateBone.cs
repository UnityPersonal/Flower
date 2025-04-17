using System.Collections;
using System.Collections.Generic;
using TreeEditor;
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
        transform.localEulerAngles = new Vector3( transform.localEulerAngles.x, transform.localEulerAngles.y, currentAngle );
    }
}
