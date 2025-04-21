using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerMarker : MonoBehaviour
{
    private TriggerItem animationTrigger;

    void Awake()
    {
        animationTrigger = GetComponentInParent<TriggerItem>(true);
        if (animationTrigger == null)
        {
            Debug.LogError("FlowerBudAnimator: No TriggerItem component attached");
        }
        animationTrigger.callbacks.OnTriggerd += FadeoutMarker;
    }

    void FadeoutMarker()
    {
        gameObject.SetActive(false);
    }

}
