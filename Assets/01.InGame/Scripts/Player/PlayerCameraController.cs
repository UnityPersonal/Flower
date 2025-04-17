using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class PlayerCameraController : MonoBehaviour
{
    private CinemachineVirtualCamera camera;
    [SerializeField] float FovMin = 60f;
    [SerializeField] float FovMax = 60f;
    void Start()
    {
        camera = GetComponent<CinemachineVirtualCamera>();
    }
    void Update()
    {
        var t = PlayerController.localPlayer.NormalizedSpeed;
        camera.m_Lens.FieldOfView = Mathf.Lerp(FovMin, FovMax, t);
    }
}
