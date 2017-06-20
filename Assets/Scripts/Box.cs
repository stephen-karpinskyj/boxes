﻿using UnityEngine;
using DG.Tweening;

public class Box : MonoBehaviour
{
    private const float MoveDuration = 0.35f;
    private const float MinDistBeforeMove = 0.5f;
    
    [SerializeField]
    private Transform visualParent;

    private float size;
    private float halfSize;

    private bool isRotating;
    private bool hasMovedSinceDrag;
    private Vector3 originalPlanePoint;

    private void Awake()
    {
        this.size = this.transform.localScale.x;
        this.halfSize = this.size / 2f;
    }

    private void Start()
    {
        GameManager.Instance.OnTick += this.HandleTick;
    }

    private void OnDestroy()
    {
        if (GameManager.Exists)
        {
            GameManager.Instance.OnTick -= this.HandleTick;
        }
    }

    private void Move(float duration, Vector2 dir)
    {
        Debug.Log("[Box] Moving dir=" + dir + ", duration=" + duration, this);

        Debug.Assert(!this.isRotating, this);

        var visualPos = this.visualParent.position;
        var pivotPos = visualPos;
        var addedPivotRot = Vector3.zero;

        if (!Mathf.Approximately(dir.x, 0f))
        {
            pivotPos.x += Mathf.Sign(dir.x) * this.halfSize;
            addedPivotRot.z -= Mathf.Sign(dir.x) * 90f;
        }
        else if (!Mathf.Approximately(dir.y, 0f))
        {
            pivotPos.z += Mathf.Sign(dir.y) * this.halfSize;
            addedPivotRot.x += Mathf.Sign(dir.y) * 90f;
        }

        pivotPos.y = 0f;

        this.transform.position = pivotPos;
        this.visualParent.position = visualPos;

        this.isRotating = true;

        this.transform.DORotate(addedPivotRot, duration, RotateMode.WorldAxisAdd)
            .SetEase(Ease.InQuad)
            .OnComplete(() => this.isRotating = false);
    }

    private void MoveRandom(float duration)
    {
        var dir = Vector2.zero;

        switch (Random.Range(0, 4))
        {
            case 0: dir.x += 1f; break;
            case 1: dir.x -= 1f; break;
            case 2: dir.y += 1f; break;
            case 3: dir.y -= 1f; break;
        }

        this.Move(duration, dir);
    }

    private void HandleTick()
    {
        //this.MoveRandom(0.5f);
    }

    private Vector3 GetPlanePoint(WorldInput.DragData drag)
    {
        var plane = new Plane(Vector3.up, this.visualParent.position);
        var dist = 0f;

        plane.Raycast(drag.CurrentRay, out dist);
        return drag.CurrentRay.GetPoint(dist);
    }

    private void OnDragStartWorldObject(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.visualParent.gameObject)
        {
            this.hasMovedSinceDrag = false;
            this.originalPlanePoint = this.GetPlanePoint(drag);
        }
    }

    private void OnDragUpdateWorldObject(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.visualParent.gameObject)
        {
            var canStillMove = !this.hasMovedSinceDrag && !this.isRotating;
            
            if (canStillMove)
            {
                var planePoint = this.GetPlanePoint(drag);
                var amountDragged = planePoint - this.originalPlanePoint;
                var amountDraggedV2 = new Vector2(amountDragged.x, amountDragged.z);

                if (amountDraggedV2.magnitude >= MinDistBeforeMove)
                {
                    if (Mathf.Abs(amountDragged.x) > Mathf.Abs(amountDraggedV2.y))
                    {
                        this.Move(MoveDuration, Vector2.right * Mathf.Sign(amountDraggedV2.x));
                    }
                    else
                    {
                        this.Move(MoveDuration, Vector2.up * Mathf.Sign(amountDraggedV2.y));
                    }

                    this.hasMovedSinceDrag = true;
                }
            }
        }
    }
}
