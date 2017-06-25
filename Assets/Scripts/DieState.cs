using System;
using UnityEngine;

[Serializable]
public class DieState
{
    public Vector3 PrevDragPoint;

    public Vector3 HingePos;
    public Quaternion HingeRot;
    public Vector3 VisualPos;

    public float MoveProgress;
}
