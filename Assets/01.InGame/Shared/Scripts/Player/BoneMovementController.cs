using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneMovementController : MonoBehaviour
{
    public Transform target;
    public float damping = 0.1f; //
    
    public Vector3 velocity = Vector3.zero;

    public float CurBoneDistance { get; private set; }
    public void Setup(Transform source)
    {
        target = source;
    }
    
    void Update()
    {
        var setting = PetalSettings.Instance;
        var player = PlayerController.localPlayer;
        float dampWeight =
            Mathf.Clamp(
                player.NormalizedSpeed,
                setting.boneDamperMin, setting.boneDamperMax);

        CurBoneDistance = Mathf.Lerp(
            PetalSettings.Instance.boneDistanceMin,
            PetalSettings.Instance.boneDistanceMax,
            player.NormalizedSpeed);
        
        var targetPosition = target.position - target.forward * CurBoneDistance;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dampWeight);
        if(velocity.magnitude < 0.01f)
            velocity = target.forward;
        transform.forward = velocity.normalized;        
    }
}
