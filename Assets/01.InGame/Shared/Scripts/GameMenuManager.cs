using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    public RawImage[] backgroundImages;
    private float dir = 1;
    private float holdTime = 0;
    public RectTransform[] images;

    public RawImage[] flowerImagse;
    
    private bool gotoInGame = false;

    IEnumerator LoadScene()
    {
        float duration = 2;

        while (duration > 0)
        {
            duration -= Time.deltaTime;
            foreach (var image in flowerImagse)
            {
                var c = image.color;
                c.r += dir * Time.deltaTime;
                c.g += dir * Time.deltaTime;
                c.b += dir * Time.deltaTime;
            
                c.r = Mathf.Clamp(c.r, 0, 1);
                c.g = Mathf.Clamp(c.g, 0, 1);
                c.b = Mathf.Clamp(c.b, 0, 1);
            
                image.color = c;
            }
            yield return null;
        }
        SceneManager.LoadScene("01.InGame/Scenes/InGameScene");

        
        
    }

    void Update()
    {
        if (gotoInGame)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            dir = -1;
            holdTime = 0;
        }

        if (Input.GetMouseButton(0))
        {
            holdTime += Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dir = 1;
        }
        
        
        foreach (var image in backgroundImages)
        {
            var c = image.color;
            c.r += dir * Time.deltaTime;
            c.g += dir * Time.deltaTime;
            c.b += dir * Time.deltaTime;
            
            c.r = Mathf.Clamp(c.r, 0, 1);
            c.g = Mathf.Clamp(c.g, 0, 1);
            c.b = Mathf.Clamp(c.b, 0, 1);
            
            image.color = c;
        }

        foreach (var img in images)
        {
            var scale = img.localScale;
            scale.x += -dir * Time.deltaTime;
            scale.y += -dir * Time.deltaTime;
            scale.z += -dir * Time.deltaTime;
            
            scale.x = Mathf.Clamp(scale.x, 1, 2);
            scale.y = Mathf.Clamp(scale.y, 1, 2);
            scale.z = Mathf.Clamp(scale.z, 1, 2);
            
            img.localScale = scale;
        }

        if (holdTime >= 2.0f)
        {
            gotoInGame = true;
            // load ingame
            StartCoroutine(LoadScene());
        }
    }
}
