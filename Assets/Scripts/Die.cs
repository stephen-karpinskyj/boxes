using UnityEngine;

public class Die : MonoBehaviour
{
    private const float DragDistPerRoll = 1.25f;
    private const float RollDirChangeProgressLimit = 0.35f;
    private const float RollDirChangeDiffLimit = 0.03f;
    private const float SwipeSpeedLimit = 0.1f;
    private const float FastRollSpeed = 7f;
    private const float SlowRollSpeed = 4.4f;
    
    [SerializeField]
    private Transform visualParent;

    [SerializeField]
    private Collider coll;

    [SerializeField]
    private AnimationCurve rollCurve;

    private float size;
    private float halfSize;

    private DieState state = new DieState();
    private DieMoveList moves = new DieMoveList();

    private float progressDampVel;

    private void Awake()
    {
        this.size = this.transform.localScale.x;
        this.halfSize = this.size / 2f;

        this.state = new DieState();
        this.moves = new DieMoveList();

        this.UpdateState();
    }

    private void Update()
    {
        var deltaProgress = Time.smoothDeltaTime;

        do
        {
            if (!this.moves.IsEmpty)
            {
                var move = this.moves.GetOldest();
                var maxDeltaProgress = deltaProgress * move.RollSpeed;
                var deltaMoveProgress = Mathf.Clamp(move.Progress - this.state.MoveProgress, -maxDeltaProgress, maxDeltaProgress);

                this.state.MoveProgress += deltaMoveProgress;
	            Debug.Assert(this.state.MoveProgress >= 0f && this.state.MoveProgress <= 1f, this);

                deltaProgress -= Mathf.Abs(deltaMoveProgress / move.RollSpeed);

	            this.UpdateRoll(move.Direction);

	            if (this.state.MoveProgress <= 0f || this.state.MoveProgress >= 1f)
	            {
                    if (this.state.MoveProgress >= 1f)
                    {
                        GameManager.Instance.EndMove(this);
                    }

	                this.moves.RemoveOldest();
	                this.UpdateState();
	            }
            }
        }
        while (this.CanStillMove() && deltaProgress > 0f);
    }

    private bool CanStillMove()
    {
        if (!this.moves.IsEmpty)
        {
            var move = this.moves.GetOldest();

            if (!Mathf.Approximately(this.state.MoveProgress, move.Progress))
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateState()
    {
        this.state.HingePos = this.transform.position;
        this.state.HingeRot = this.transform.rotation;
		this.state.VisualPos = this.visualParent.position;
        this.state.MoveProgress = 0f;
    }

    private void ResetToState()
    {
        this.transform.rotation = this.state.HingeRot;
        this.transform.position = this.state.HingePos;
		this.visualParent.position = this.state.VisualPos;
    }

    private void UpdateRoll(Vector3 direction)
    {
        this.ResetToState();

        var newVisualPos = this.visualParent.position;
        var newHingePos = this.visualParent.position;
        var newHingeRot = this.state.HingeRot;

        newHingePos += direction * halfSize;
        newHingePos.y = 0f;

        if (!Mathf.Approximately(direction.x, 0f))
        {
            newHingeRot = Quaternion.Euler(0f, 0f, direction.x * -90f) * newHingeRot;
        }
        else if (!Mathf.Approximately(direction.z, 0f))
        {
            newHingeRot = Quaternion.Euler(direction.z * 90f, 0f, 0f) * newHingeRot;
        }

        var progress = this.rollCurve.Evaluate(this.state.MoveProgress);

        this.transform.position = newHingePos;
        this.visualParent.position = newVisualPos;
        this.transform.rotation = Quaternion.Slerp(this.state.HingeRot, newHingeRot, progress);
    }

    private Vector3 GetPlanePoint(WorldInput.DragData drag)
    {
        var plane = new Plane(Vector3.up, this.visualParent.position);
        var dist = 0f;

        plane.Raycast(drag.CurrentRay, out dist);
        return drag.CurrentRay.GetPoint(dist);
    }

    private void WorldInput_DragStart(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.coll.gameObject)
        {
            this.moves.ClearAllExceptOldest();

            this.state.PrevDragPoint = this.GetPlanePoint(drag);
        }
    }

    private void WorldInput_DragUpdate(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.coll.gameObject)
        {
            var planePoint = this.GetPlanePoint(drag);
            Vector3 amountDragged;

            do
            {
				amountDragged = planePoint - this.state.PrevDragPoint;

                var xMag = Mathf.Min(DragDistPerRoll, Mathf.Abs(amountDragged.x));
                var xDir = Mathf.Sign(amountDragged.x);
                var zMag = Mathf.Min(DragDistPerRoll, Mathf.Abs(amountDragged.z));
                var zDir = Mathf.Sign(amountDragged.z);

                if (Mathf.Max(xMag, zMag) > 0f)
                {
                    var move = this.moves.GetLatestOrNew(FastRollSpeed);

	                if (move.Direction == Vector3.zero)
	                {
                        move.Direction = xMag > zMag ? Vector3.right * xDir : Vector3.forward * zDir;
	                }
                    else if (move.IsNearerToFinishing(RollDirChangeProgressLimit))
                    {
                        move = this.CheckForRollDirectionChange(xMag, zMag, xDir, zDir, move);
                    }

                    var dragDelta = Vector3.zero;
                    var moveProgressDelta = 0f;

	                if (!Mathf.Approximately(move.Direction.x, 0f))
                    {
                        dragDelta.x = xMag * xDir;
                        dragDelta.z = amountDragged.z; // Ignore non-rolling direction
                        moveProgressDelta = move.Direction.x * dragDelta.x / DragDistPerRoll;
	                }
                    else if (!Mathf.Approximately(move.Direction.z, 0f))
                    {
                        dragDelta.x = amountDragged.x; // Ignore non-rolling direction
						dragDelta.z = zMag * zDir;
                        moveProgressDelta = move.Direction.z * dragDelta.z / DragDistPerRoll;
                    }

                    if (dragDelta != Vector3.zero)
                    {
                        Debug.Assert(!Mathf.Approximately(moveProgressDelta, 0f), this);
                        
                        move = this.moves.AddProgress(moveProgressDelta, FastRollSpeed);
	                    this.state.PrevDragPoint += dragDelta;
                    }
                }
            }
            while (Mathf.Abs(Mathf.Max(amountDragged.x, amountDragged.z)) > 9f);
        }
    }

    private DieMove CheckForRollDirectionChange(float xMag, float zMag, float xDir, float zDir, DieMove move)
    {
        var magDiff = Mathf.Abs(xMag - zMag);

        if (magDiff > RollDirChangeDiffLimit)
        {
            if (xMag > zMag && !Mathf.Approximately(move.Direction.z, 0f)) 
            {
                this.moves.RoundLatestProgress(SlowRollSpeed, false);
                move = this.moves.GetLatestOrNew(FastRollSpeed);
                move.Direction = Vector3.right * xDir;
            }
            else if (zMag > xMag && !Mathf.Approximately(move.Direction.x, 0f))
            {
                this.moves.RoundLatestProgress(SlowRollSpeed, false);
                move = this.moves.GetLatestOrNew(FastRollSpeed);
                move.Direction = Vector3.forward * zDir;
            }
        }

        return move;
    }

    private void WorldInput_DragEnd(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.coll.gameObject)
        {
            this.moves.ClearAllExceptOldest();

            var dragWorldVelocity = drag.CurrentWorldPosition - drag.PrevWorldPosition;
            var didSwipe = dragWorldVelocity.magnitude > SwipeSpeedLimit;

            var alwaysRoundUp = !this.moves.IsEmpty && didSwipe;
            this.moves.RoundLatestProgress(SlowRollSpeed, alwaysRoundUp);
        }
    }
}
