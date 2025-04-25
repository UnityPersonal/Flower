using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;


public class FlowerBudAnimator : MonoBehaviour
{
    private static readonly int BLOOM = Animator.StringToHash("bloom");

    private TriggerItem animationTrigger;
    // Start is called before the first frame update
    [SerializeField] private Animator animator;
    [SerializeField] private float duration = 0.25f;
    
    void Awake()
    {
        animationTrigger = GetComponentInParent<TriggerItem>(true);
        if (animationTrigger == null)
        {
            Debug.LogError("FlowerBudAnimator: No TriggerItem component attached");
        }
        animationTrigger.callbacks.OnTriggerd += OpenBud;
    }

    void OpenBud()
    {
        animator.SetBool(BLOOM, true);
    }
}
