using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBone : MonoBehaviour
{
    public float currentAngle;
    public float rotationSpeedMin = 0;
    public float rotationSpeedMax = 100f;
    
    void Update()
    {
        var t = PlayerController.LocalPlayer.NormalizedSpeed;
        var curSpeed = Mathf.Lerp(rotationSpeedMin, rotationSpeedMax, t);
        currentAngle += curSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3( transform.localEulerAngles.x, transform.localEulerAngles.y, currentAngle );
    }
}
