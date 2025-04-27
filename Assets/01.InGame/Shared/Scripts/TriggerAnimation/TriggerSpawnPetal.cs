using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawnPetal : MonoBehaviour
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
    public TriggerType triggerType;
    TriggerItem trigger;
    
    private void Awake()
    {
        trigger = GetComponentInParent<TriggerItem>(true);
        if (trigger == null)
        {
            Debug.LogError("TriggerSpawnPetal: No TriggerItem component attached");
        }
        trigger.callbacks.OnTriggerd += SpawnPetal;
    }

    private void SpawnPetal()
    {
        PetalGenerator.Instance.GeneratePetal(TypeMapper.MapPetalType(triggerType), transform.position);
    }
}
