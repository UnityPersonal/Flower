using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Petal : MonoBehaviour
{
    public PlayerBone forwardBone;
    public PlayerBone backwardBone;
    [Range(0, 1)] public float normalizedPosition;
    public float radius;
    
    public float angleOffset;

    // Update is called once per frame
    void Update()
    {
        var p0 = forwardBone.transform.position;
        var p1 = backwardBone.transform.position;
        
        Vector3 to = Vector3.Lerp(p0, p1, normalizedPosition);
        
        var f0 = forwardBone.transform.forward;
        var f1 = backwardBone.transform.forward;
        
        Vector3 forward = Vector3.Slerp(f0, f1, normalizedPosition).normalized;
        
        var r0 = forwardBone.transform.right;
        var r1 = backwardBone.transform.right;
        
        Vector3 right = Vector3.Slerp(r0, r1, normalizedPosition).normalized;
        
        right = Quaternion.Euler(0, 0, angleOffset) * right;
        to += right * radius;
        
        
        
        transform.position = Vector3.Lerp(transform.position,to, Time.deltaTime);
        
    }
}
