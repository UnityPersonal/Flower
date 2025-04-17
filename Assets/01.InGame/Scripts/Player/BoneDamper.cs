using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BoneDamper : MonoBehaviour
{
    public Transform target;
    public float damping = 0.1f; //
    
    private Vector3 velocity = Vector3.zero;
    
    DampedTransform dampedTransform;
    private void Awake()
    {
        dampedTransform = GetComponent<DampedTransform>();
    }

    public void Setup(Transform source, Transform constrained)
    {
        dampedTransform.data.sourceObject = source;
        dampedTransform.data.constrainedObject = constrained;
    }

    void Update()
    {
        float dampWeight = Mathf.Clamp(PlayerController.localPlayer.NormalizedSpeed, 0.2f, 1.0f);
        dampedTransform.weight = dampWeight;
        
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, dampWeight);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * dampWeight);        
    }
}
