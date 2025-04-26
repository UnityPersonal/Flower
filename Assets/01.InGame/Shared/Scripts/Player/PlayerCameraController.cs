using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class PlayerCameraController : MonoBehaviour
{
        private CinemachineVirtualCamera virtualCamera;
    [SerializeField] float FovMin = 60f;
    [SerializeField] float FovMax = 60f;
    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    void Update()
    {
        var t = PlayerController.localPlayer.NormalizedSpeed;
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(FovMin, FovMax, t);
    }
}
