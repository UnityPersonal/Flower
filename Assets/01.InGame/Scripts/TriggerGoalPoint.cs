using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerGoalPoint : MonoBehaviour
{
    private Collider mainCollider;

    private void Awake()
    {
        mainCollider = GetComponent<Collider>();
    }

    void Update()
    {
        
    }
}
