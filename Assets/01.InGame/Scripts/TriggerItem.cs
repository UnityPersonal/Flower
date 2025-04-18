using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

[System.Serializable]
public enum TriggerType
{
    Unknown,
    PetalYellow,
    PetalOrange,
    PetalRed,
    PetalPurple
}

[RequireComponent(typeof(Collider))]
public class TriggerItem : MonoBehaviour
{
    public TriggerType type;
    private Collider mainCollider;

    private void Awake()
    {
        mainCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerController.localPlayer.mainCollider == other)
        {
            while (PetalGenerator.Instance.GeneratePetal(TypeMapper.MapPetalType(type)) == false)
            {
                BoneGenerator.Instance.GenerateBone();
            }
            GameManager.instance.OnTriggeredItem();
            mainCollider.enabled = false;
        }
    }
}
