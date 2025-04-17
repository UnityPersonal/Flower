using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class RotateBone : MonoBehaviour
{
    public Transform toFollow;
    public float currentAngle;
    public float rotationSpeedMin = 0;
    public float rotationSpeedMax = 100f;
    
    void Start()
    {
        
    }

    void Update()
    {
        var t = PlayerController.localPlayer.NormalizedSpeed;
        var curSpeed = Mathf.Lerp(rotationSpeedMin, rotationSpeedMax, t);
        currentAngle += curSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3( transform.localEulerAngles.x, transform.localEulerAngles.y, currentAngle );
    }
}
