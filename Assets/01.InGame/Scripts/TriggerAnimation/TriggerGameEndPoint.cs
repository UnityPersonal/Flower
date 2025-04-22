using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(Collider))]
public class TriggerGameEndPoint : MonoBehaviour
{
    [SerializeField] Collider mainCollider;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] PlayableDirector director;

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
        Debug.Log("OnCompletedGame");
        mainCollider.enabled = true;
        meshRenderer.enabled = true;

        director.Play();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("End Game");
    }

}
