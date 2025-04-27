using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    
    [Header("Interaction Setting")]
    [SerializeField] private SphereCollider triggerCollider;
    public Collider MainCollider => triggerCollider;
    [SerializeField] private float triggerGrowthStep;
    [SerializeField] private float triggerRadiusMax;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        var petalgenerator = PetalGenerator.Instance as PetalGenerator;
        if (petalgenerator != null)
            petalgenerator.events.OnGeneratedPetal += OnGeneratedPetal;
    }

    void OnGeneratedPetal()
    {
        triggerCollider.radius += triggerGrowthStep;
        triggerCollider.radius = Mathf.Min(triggerCollider.radius, triggerRadiusMax);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(triggerCollider.transform.position, triggerCollider.radius );
    }
}
