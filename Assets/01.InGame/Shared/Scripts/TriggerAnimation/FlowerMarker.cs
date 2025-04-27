using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FlowerMarker : MonoBehaviour
{
    private TriggerItem animationTrigger;

    private MeshRenderer meshRenderer;
    private Camera camera1;

    private void Start()
    {
        camera1 = Camera.main;
    }

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        animationTrigger = GetComponentInParent<TriggerItem>(true);
        if (animationTrigger == null)
        {
            Debug.LogError("FlowerMarker: No TriggerItem component attached");
        }
        animationTrigger.callbacks.OnTriggerd += FadeoutMarker;
    }

    private void Update()
    {
        Vector3 targetForward = camera1.transform.forward;
        Vector3 targetup = camera1.transform.up;
        //targetForward.y = 0.01f;   // Not zero to avoid issues.
        transform.rotation = Quaternion.LookRotation(targetForward.normalized, targetup);
    }

    void FadeoutMarker()
    {
        meshRenderer.material.DOFloat(0, "_EmissionWeight", 0.5f);
    }

}
