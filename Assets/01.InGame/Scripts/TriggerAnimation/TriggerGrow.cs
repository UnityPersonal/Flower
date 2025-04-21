using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TriggerGrow : MonoBehaviour
{

    [SerializeField] TriggerItem[] questTargets;
    
    private TriggerItem[] triggerItemsToGrow;
    private int currentProgress = 0;

    [SerializeField] private PlayableDirector director;
    [SerializeField] SignalReceiver signalReceiver;
    [SerializeField] private SignalAsset begin;
    [SerializeField] private SignalAsset end;
    [SerializeField] private SignalAsset grow;
    [SerializeField] private Ease easeType = Ease.OutElastic;
    
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

        UnityEvent evt;
        evt = signalReceiver.GetReaction(begin);
        evt.AddListener(PlayerController.localPlayer.Lock);
        
        evt = signalReceiver.GetReaction(end);
        evt.AddListener(PlayerController.localPlayer.UnLock);
        
        evt = signalReceiver.GetReaction(grow);
        evt.AddListener(DoGrow);
        
    }

    public void DoGrow()
    {
        foreach (var item in triggerItemsToGrow)
        {
            item.gameObject.SetActive(true);
            item.gameObject.transform.localScale = Vector3.zero;
            
            item.gameObject.transform.DOScale(Vector3.one, 1f).SetEase(easeType);

        }
    }

    void OnTriggeredTarget()
    {
        currentProgress++;
        if (currentProgress >= questTargets.Length)
        {
            director.Play();
            
        }
        
    }

}
