using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BoneDamper : MonoBehaviour
{
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
        var inputAxis = PlayerController.localPlayer.inputAxis;
        float dampWeight = 0f;
        if (inputAxis.magnitude > 0.01f)
        {
            dampWeight =  inputAxis.magnitude;
        }
        dampedTransform.weight = dampWeight;
    }
}
