using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(Collider))]
public class TriggerSensor : MonoBehaviour
{
    public class Callbacks
    {
        public Action OnTriggerd;
    }
    public Callbacks callbacks = new Callbacks();
    private Collider mainCollider;

    private void Awake()
    {
        mainCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Player.instance.MainCollider == other)
        {
            mainCollider.enabled = false;
            callbacks.OnTriggerd?.Invoke();
        }
    }
}
