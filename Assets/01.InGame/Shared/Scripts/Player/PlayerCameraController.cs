using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Examples;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class PlayerCameraController : MonoBehaviour
{
    //
    private CinemachineVirtualCamera virtualCamera;
    private Cinemachine3rdPersonFollow follow;
    [SerializeField] float FovMin = 60f;
    [SerializeField] float FovMax = 60f;
    [SerializeField] private float distanceMin = 24;
    [SerializeField] private float distanceMax = 12;
    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        follow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }
    void Update()
    {
        var t = PlayerController.LocalPlayer.NormalizedSpeed;
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(FovMin, FovMax, t);
        follow.CameraDistance = Mathf.Lerp(distanceMin, distanceMax, t);
        //transposer.m_FollowOffset.x = Mathf.Lerp(distanceMin, distanceMax, t);
        
    }
}
