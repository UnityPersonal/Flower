using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class FlowerBudAnimator : MonoBehaviour
{
    private TriggerItem animationTrigger;
    // Start is called before the first frame update
    [SerializeField] private float closedSize = 0.5f;
    [SerializeField] private float openedSize = 1f;
    [SerializeField] private float duration = 0.25f;

    void Awake()
    {
        transform.localScale = Vector3.one * closedSize;

        animationTrigger = GetComponentInParent<TriggerItem>(true);
        if (animationTrigger == null)
        {
            Debug.LogError("FlowerBudAnimator: No TriggerItem component attached");
        }
        animationTrigger.callbacks.OnTriggerd += OpenBud;
    }

    void OpenBud()
    {
        transform.DOScale(Vector3.one * openedSize, duration);
        
    }

}
