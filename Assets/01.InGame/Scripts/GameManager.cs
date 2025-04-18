using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }


    [SerializeField, Range(0, 1)] private float completeRate = 1;
    private int completedItemCount;
    private void Start()
    {
         var items = GameObject.FindObjectsByType<TriggerItem>(FindObjectsSortMode.None);
         completedItemCount = (int)(items.Length* completeRate);
    }

    public void OnTriggeredItem()
    {
        completedItemCount--;
        if (completedItemCount <= 0)
        {
            // complete game;
            Debug.Log("Game Complete");
        }
    }
    
}
