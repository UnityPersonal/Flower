using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InGameFadeUI : MonoBehaviour , ILoadable
{
    [SerializeField] Image backgroundImage;
    
    [SerializeField] float fadeDuration = 1f;
    
    public class Callbacks
    {
        public System.Action OnFadeComplete;
    }
    public Callbacks callbacks { get; private set; } = new Callbacks();
    
    private void Awake()
    {
        if (backgroundImage == null)
        {
            Debug.LogError("Background Image is not assigned.");
            return;
        }
        OnLoadComplete();
    }

    public void OnLoadComplete()
    {
        GameManager.Instance.events.OnBeginGame += OnBeginGame;
        GameManager.Instance.OnLoadComplete(this);
    }

    private void OnBeginGame()
    {
        backgroundImage.DOFade(0, fadeDuration).onComplete += () =>
        {
            callbacks?.OnFadeComplete?.Invoke();
        };
    }
}
