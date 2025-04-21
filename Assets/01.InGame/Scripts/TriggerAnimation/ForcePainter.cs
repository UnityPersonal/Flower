using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ForcePainter : MonoBehaviour
{
    public Ease easeType = Ease.Linear;
    public float endSize; 
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            InjectForce();
        }
    }

    void InjectForce()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one * endSize, 0.5f).SetEase(easeType);
    }
}
