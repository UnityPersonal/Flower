using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum TriggerType
{
    Unknown,
    Petal,
}

public class TriggerItem : MonoBehaviour
{

    public TriggerType type;
    private void OnTriggerEnter(Collider other)
    {
        if (PlayerController.localPlayer.mainCollider == other)
        {
            while (PetalGenerator.Instance.GeneratePetal() == false)
            {
                BoneGenerator.Instance.GenerateBone();
            }
            // generatePetal;
        }
    }
}
