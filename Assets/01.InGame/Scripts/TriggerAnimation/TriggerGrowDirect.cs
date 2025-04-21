using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TriggerGrowDirect : MonoBehaviour
{
    [SerializeField] TriggerItem[] questTargets;
    [SerializeField] private float growDuration = 1;
    private TriggerItem[] triggerItemsToGrow;
    private int currentProgress = 0;

    void Start()
    {
        if (questTargets.Length == 0)
            return;
        
        triggerItemsToGrow = GetComponentsInChildren<TriggerItem>();
        
        foreach (TriggerItem item in questTargets)
        {
            item.callbacks.OnTriggerd += OnTriggeredTarget;
        }

        foreach (var item in triggerItemsToGrow)
        {
            item.gameObject.SetActive(false);
        }
    }


    public void DoGrow()
    {
        foreach (var item in triggerItemsToGrow)
        {
            item.gameObject.SetActive(true);
            item.gameObject.transform.localScale = Vector3.zero;
            
            item.gameObject.transform.DOScale(Vector3.one, growDuration);

        }
    }

    void OnTriggeredTarget()
    {
        currentProgress++;
        if (currentProgress >= questTargets.Length)
        {
            DoGrow();
        }
    }
}
