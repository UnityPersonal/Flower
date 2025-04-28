using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBone : MonoBehaviour
{
    public RotateBone rotateBone;
    public BoneMovementController movementController;

    public PlayerBone attached { get; set; }
    public bool isRoot = false;
}
