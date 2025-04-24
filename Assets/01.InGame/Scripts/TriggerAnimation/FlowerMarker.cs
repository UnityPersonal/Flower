using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FlowerMarker : MonoBehaviour
{
    private TriggerItem animationTrigger;

    private MeshRenderer meshRenderer;
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        animationTrigger = GetComponentInParent<TriggerItem>(true);
        if (animationTrigger == null)
        {
            Debug.LogError("FlowerBudAnimator: No TriggerItem component attached");
        }
        animationTrigger.callbacks.OnTriggerd += FadeoutMarker;
    }

    void FadeoutMarker()
    {
        meshRenderer.material.DOFloat(0, "_EmissionWeight", 0.5f);
    }

}
