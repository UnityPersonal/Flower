using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapPainter : MonoBehaviour
{
    private static readonly int PAINT_COLOR = Shader.PropertyToID("_PaintColor");
    [SerializeField] private TriggerItem painterTrigger;
    
    [SerializeField] private Color paintColor;
    [SerializeField] private float paintRadius;
    [SerializeField] private float paintDuration;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetColor(PAINT_COLOR, paintColor);
        transform.localScale = Vector3.zero; // disappear by scale

        painterTrigger.callbacks.OnTriggerd += DoPaint;
    }
    

    public void DoPaint()
    {
        transform.DOScale(Vector3.one * paintRadius , paintDuration);
        
    }

}
