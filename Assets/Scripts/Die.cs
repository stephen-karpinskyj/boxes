using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Die : MonoBehaviour
{
    private const float MoveDuration = 0.35f;
    private const float MinDistBeforeMove = 0.65f;
    
    [SerializeField]
    private Transform visualParent;

    [SerializeField]
    private Collider coll;

    private float size;
    private float halfSize;

    private bool isMoving;

    private Vector3 lastDragPoint;
    private readonly Queue<Vector2> moves = new Queue<Vector2>();

    private void Awake()
    {
        this.size = this.transform.localScale.x;
        this.halfSize = this.size / 2f;
    }

    private void Start()
    {
        GameManager.Instance.OnTick += this.HandleTick;
    }

    private void Update()
    {
        if (!this.isMoving && this.moves.Count > 0)
        {
            this.Move(MoveDuration, this.moves.Dequeue());
        }
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
        Debug.Log("[Die] Moving dir=" + dir + ", duration=" + duration, this);

        Debug.Assert(!this.isMoving, this);

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

        this.isMoving = true;

        this.transform.DORotate(addedPivotRot, duration, RotateMode.WorldAxisAdd)
            .SetEase(Ease.InQuad)
            .OnComplete(() => this.isMoving = false);
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
            this.lastDragPoint = this.GetPlanePoint(drag);
        }
    }

    private void OnDragUpdateWorldObject(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.coll.gameObject)
        {
            var planePoint = this.GetPlanePoint(drag);
            var amountDragged = planePoint - this.lastDragPoint;

            if (Mathf.Abs(amountDragged.x) >= MinDistBeforeMove)
            {
                var move = new Vector2(Mathf.Sign(amountDragged.x), 0f);
                this.moves.Enqueue(move);

                this.lastDragPoint.x += move.x;
            }

            if (Mathf.Abs(amountDragged.z) >= MinDistBeforeMove)
            {
                var move = new Vector2(0f, Mathf.Sign(amountDragged.z));
                this.moves.Enqueue(move);

                this.lastDragPoint.z += move.y;
            }
        }
    }
}
