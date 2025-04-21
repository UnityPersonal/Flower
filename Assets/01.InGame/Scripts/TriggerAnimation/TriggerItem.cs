using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(Collider))]
public class TriggerItem : MonoBehaviour
{
    [System.Serializable]
    public enum TriggerType
    {
        Unknown,
        PetalYellow,
        PetalOrange,
        PetalRed,
        PetalPurple
    }

    public class Callbacks
    {
        public Action OnTriggerd;
    }
    public Callbacks callbacks = new Callbacks();
    
    [FormerlySerializedAs("type")] public TriggerType triggerType;
    private Collider mainCollider;

    private void Awake()
    {
        mainCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Player.instance.MainCollider == other)
        {
            while (PetalGenerator.Instance.GeneratePetal(TypeMapper.MapPetalType(triggerType)) == false)
            {
                BoneGenerator.Instance.GenerateBone();
            }
            mainCollider.enabled = false;
            
            GameManager.instance.OnTriggeredItem();
            callbacks.OnTriggerd?.Invoke();
        }
    }
}
