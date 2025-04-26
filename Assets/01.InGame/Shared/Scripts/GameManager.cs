using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public class Events
    {
        public Action OnCompletedGame;
    }
    public Events events = new Events();

    private void Awake()
    {
        instance = this;
    }


    [SerializeField, Range(0, 1)] private float completeRate = 1;
    private int completedItemCount;
    private void Start()
    {
         var items = GameObject.FindObjectsOfType<TriggerItem>(true);
         completedItemCount = (int)(items.Length* completeRate);
    }
    
    public void OnTriggeredItem()
    {
        completedItemCount--;
        if (completedItemCount <= 0)
        {
            // complete game;
            Debug.Log("Game Complete");
            events.OnCompletedGame?.Invoke();
        }
    }
    
}
