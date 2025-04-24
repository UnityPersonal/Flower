using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;


public class FlowerBudAnimator : MonoBehaviour
{
    private TriggerItem animationTrigger;
    // Start is called before the first frame update
    [SerializeField] private AlembicStreamPlayer streamPlayer;
    [SerializeField] private float duration = 0.25f;

    
    void Awake()
    {
        animationTrigger = GetComponentInParent<TriggerItem>(true);
        if (animationTrigger == null)
        {
            Debug.LogError("FlowerBudAnimator: No TriggerItem component attached");
        }
        animationTrigger.callbacks.OnTriggerd += OpenBud;

        streamPlayer = GetComponent<AlembicStreamPlayer>();

    }

    void OpenBud()
    {
        StartCoroutine(DoBloom());
    }


    IEnumerator DoBloom()
    {
        float time = 0;
        while (time < duration)
        {
            streamPlayer.CurrentTime = (time / duration) * streamPlayer.EndTime;
            time += Time.deltaTime;
            yield return null;
        }
        streamPlayer.CurrentTime = (time / duration) * streamPlayer.EndTime;
    }
    
}
