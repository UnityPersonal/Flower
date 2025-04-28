using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ForcePainter : MonoBehaviour
{
    private static readonly int FORCE_STRENGTH = Shader.PropertyToID("_ForceStrength");
    public Ease easeType = Ease.Linear;
    public float endSize;
    [SerializeField] private float duration = 0.5f;
    
    private Material material;
    private TriggerSensor triggerSensor;

    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        triggerSensor = GetComponentInParent<TriggerSensor>(true);
        if(triggerSensor != null)
            triggerSensor.callbacks.OnTriggerd += InjectForce;
    }

    void InjectForce()
    {
        material.SetFloat(FORCE_STRENGTH,1);
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one * endSize, duration).SetEase(easeType);
        material.DOFloat(0,FORCE_STRENGTH, duration).SetEase(Ease.InQuint);
    }
}
