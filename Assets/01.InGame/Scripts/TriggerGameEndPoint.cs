using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerGameEndPoint : MonoBehaviour
{
    [SerializeField] Collider mainCollider;
    [SerializeField] MeshRenderer meshRenderer;

    private void Awake()
    {
        mainCollider.enabled = false;
        meshRenderer.enabled = false;
    }

    private void Start()
    {
        GameManager.instance.events.OnCompletedGame += OnCompletedGame;
    }

    void OnCompletedGame()
    {
        mainCollider.enabled = true;
        meshRenderer.enabled = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("End Game");
    }

}
