using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;
using Random = UnityEngine.Random;


public class FlowerBudAnimator : MonoBehaviour
{
    private static readonly int BLOOM = Animator.StringToHash("bloom");

    private TriggerItem animationTrigger;
    // Start is called before the first frame update
    [SerializeField] private Animator animator;
    
    void Awake()
    {
        animationTrigger = GetComponentInParent<TriggerItem>(true);
        if (animationTrigger == null)
        {
            Debug.LogError("FlowerBudAnimator: No TriggerItem component attached");
        }
        animationTrigger.callbacks.OnTriggerd += OpenBud;
        
        transform.localRotation = Quaternion.Euler(0, Random.Range(0,360), 0);
    }

    void OpenBud()
    {
        animator.SetBool(BLOOM, true);
    }
}
