using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public interface ILoadable
{
    void OnLoadComplete();
}

public class GameManager : Singleton<GameManager>
{
    int loadCompleteCount;
    bool isCollectedLoadables;
    
    List<ILoadable> loadables = new List<ILoadable>();
    List<ILoadable> loadedList = new List<ILoadable>();

    public void OnLoadComplete(ILoadable loadable)
    {
        // init loadables
        if (isCollectedLoadables == false)
        {
            loadables = FindInterfaces.Find<ILoadable>();
            isCollectedLoadables = true;
        }
        
        if (loadedList.Contains(loadable))
        {
            Debug.LogError("Loadable already loaded");
        }
        
        loadedList.Add(loadable);
        loadCompleteCount++;
        
        if (loadCompleteCount == loadables.Count)
        {
            Debug.Log("Load Complete");
            events.OnBeginGame?.Invoke();
        }
    }
    

    public class Events
    {
        public Action OnBeginGame;
        public Action OnEndGame;
        public Action OnCompletedGame;
    }
    public Events events = new Events();

    [SerializeField, Range(0, 1)] private float completeRate = 1;
    private int completedItemCount;
    private void Start()
    {
         var items = GameObject.FindObjectsOfType<TriggerItem>(true);
         completedItemCount = (int)(items.Length* completeRate);
    }
    
    public void OnEndGame()
    {
        events.OnEndGame?.Invoke();
    }
    
    public void GoToMainMenu()
    {
        // Load Main Menu Scene
        Debug.Log("Go to Main Menu");
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
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
