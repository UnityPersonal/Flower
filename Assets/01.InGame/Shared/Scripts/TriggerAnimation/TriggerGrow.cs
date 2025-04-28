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
    [SerializeField] TriggerSensor[] questTargets;
    
    private TriggerSensor[] triggerItemsToGrow;
    private int currentProgress = 0;

    [SerializeField] private PlayableDirector director;
    [SerializeField] SignalReceiver signalReceiver;
    [SerializeField] private SignalAsset begin;
    [SerializeField] private SignalAsset end;
    [SerializeField] private SignalAsset grow;
    [SerializeField] private Ease easeType = Ease.OutElastic;

    public class Callbacks
    {
        public Action OnGrowth;
    }
    public Callbacks callbacks { get; private set; } = new Callbacks();
    
    void Start()
    {
        if (questTargets.Length == 0)
            return;
        
        triggerItemsToGrow = GetComponentsInChildren<TriggerSensor>();
        
        foreach (TriggerSensor item in questTargets)
        {
            item.callbacks.OnTriggerd += OnTriggeredTarget;
        }

        foreach (var item in triggerItemsToGrow)
        {
            item.gameObject.SetActive(false);
        }

        UnityEvent evt;
        evt = signalReceiver.GetReaction(begin);
        if (evt == null)
        {
            Debug.LogWarning("Trigger begin reaction was not found");
        }
        evt.AddListener(PlayerController.LocalPlayer.Lock);
        
        evt = signalReceiver.GetReaction(end);
        if (evt == null)
        {
            Debug.LogWarning("Trigger end reaction was not found");
        }
        evt.AddListener(PlayerController.LocalPlayer.UnLock);
        
        evt = signalReceiver.GetReaction(grow);
        if (evt == null)
        {
            Debug.LogWarning("Trigger grow reaction was not found");
        }
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
        callbacks.OnGrowth?.Invoke();
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
