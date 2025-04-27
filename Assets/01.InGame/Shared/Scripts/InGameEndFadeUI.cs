using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InGameEndFadeUI : MonoBehaviour
{
    [SerializeField] float fadeDuration = 1f;
    [SerializeField] Image fadeImage;
    
    private void Awake()
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image is not assigned.");
            return;
        }
        
        GameManager.Instance.events.OnEndGame += OnEndGame;
    }

    private void OnEndGame()
    {
        fadeImage.DOFade(1, fadeDuration).OnComplete(() =>
        {
            GameManager.Instance.GoToMainMenu();
        });
    }
}
