using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneMovementController : MonoBehaviour
{
    public Transform target;
    public float damping = 0.1f; //
    
    public Vector3 velocity = Vector3.zero;
    private float boneDistance;
    public float BoneDistance => boneDistance;

    public void Setup(Transform source, float boneDistance)
    {
        target = source;
        this.boneDistance = boneDistance;
    }
    
    void Update()
    {
        float dampWeight = Mathf.Clamp(1f -PlayerController.localPlayer.NormalizedSpeed, 0.2f, 0.8f);

        var targetPosition = target.position - target.forward * boneDistance;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dampWeight);
        if(velocity.magnitude < 0.01f)
            velocity = target.forward;
        transform.forward = velocity.normalized;        
    }
}
