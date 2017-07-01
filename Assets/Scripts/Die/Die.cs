using UnityEngine;

public class Die : MonoBehaviour
{
    private const float DragDistPerRoll = 1.25f;
    private const float MinDragMag = 0.03f;
    private const float RollDirChangeProgressLimit = 0.35f;
    private const float RollDirChangeDiffLimit = 0.03f;
    private const float SwipeSpeedLimit = 0.1f;
    private const float FastRollSpeed = 7.5f;
    private const float SlowRollSpeed = 4.7f;

    private static IdGenerator IdGen;

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

    public int Id { get; private set; }

    private void Awake()
    {
        this.Id = IdGen.Next();
        
        this.size = this.transform.localScale.x;
        this.halfSize = this.size / 2f;

        this.state = new DieState();
        this.moves = new DieMoveList();

        if (!GameManager.Instance.HasStarted)
        {
            Object.Destroy(this.gameObject);
        }
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
                        GameManager.Instance.EndTick();
                    }

	                this.moves.RemoveOldest();
                    this.UpdateState();
	            }

                GameManager.Instance.UpdateTick(this.state.MoveProgress);
            }
        }
        while (this.CanStillMove() && deltaProgress > 0f);
    }

    public void ShowUpwardFace(int upwardFace)
    {
        this.visualParent.rotation = this.state.CalculateRandomRotation(upwardFace);
        this.UpdateState();
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

        this.state.CurrentTile.x = Mathf.RoundToInt(this.state.VisualPos.x);
        this.state.CurrentTile.y = Mathf.RoundToInt(this.state.VisualPos.z);
        Board.Instance.EnterTile(this, this.state.CurrentTile, this.state.CalculateUpwardFace());
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

    private DieMove CheckForRollDirectionChange(float xMag, float zMag, float xDir, float zDir, DieMove move)
    {
        var magDiff = Mathf.Abs(xMag - zMag);

        if (magDiff > RollDirChangeDiffLimit)
        {
            var currentTile = DieMoveList.CalculateAdjacentTile(this.moves.TargetTile, -move.Direction);

            if (xMag > zMag && !Mathf.Approximately(move.Direction.z, 0f)) 
            {
                var newDirection = Vector3.right * xDir;
                var targetTile = DieMoveList.CalculateAdjacentTile(currentTile, newDirection);
                var canMove = Board.Instance.IsTileAvailable(targetTile);

                if (canMove)
                {
	                this.moves.RoundLatestProgress(SlowRollSpeed, false);
	                move = this.moves.GetLatestOrNew(FastRollSpeed);
                    this.moves.InitializeMove(move, newDirection);
                }
            }
            else if (zMag > xMag && !Mathf.Approximately(move.Direction.x, 0f))
            {
                var newDirection = Vector3.forward * zDir;
                var targetTile = DieMoveList.CalculateAdjacentTile(currentTile, newDirection);
                var canMove = Board.Instance.IsTileAvailable(targetTile);

                if (canMove)
                {
	                this.moves.RoundLatestProgress(SlowRollSpeed, false);
	                move = this.moves.GetLatestOrNew(FastRollSpeed);
	                this.moves.InitializeMove(move, newDirection);
                }
            }
        }

        return move;
    }

    private void WorldInput_DragStart(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.coll.gameObject)
        {
            this.state.PrevDragPoint = this.GetPlanePoint(drag);

            this.moves.ClearAllExceptOldest(this.state.CurrentTile);
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

                if (Mathf.Max(xMag, zMag) > MinDragMag)
                {
                    var move = this.moves.GetLatestOrNew(FastRollSpeed);

                    if (!move.CheckIsInitialized())
	                {
                        var preferredDirection = xMag > zMag ? Vector3.right * xDir : Vector3.forward * zDir;
                        var fallbackDirection = xMag > zMag ? Vector3.forward * zDir : Vector3.right * xDir;
                        
                        this.moves.InitializeMove(move, preferredDirection, fallbackDirection);
	                }
                    else if (move.IsNearerToFinishing(RollDirChangeProgressLimit))
                    {
                        move = this.CheckForRollDirectionChange(xMag, zMag, xDir, zDir, move);
                    }

                    if (!move.CheckIsInitialized())
                    {
                        break;
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
                        move = this.moves.AddProgress(moveProgressDelta, FastRollSpeed);
	                    this.state.PrevDragPoint += dragDelta;
                    }
                }
            }
            while (Mathf.Abs(Mathf.Max(amountDragged.x, amountDragged.z)) > MinDragMag);
        }
    }

    private void WorldInput_DragEnd(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.coll.gameObject)
        {
            this.moves.ClearAllExceptOldest(this.state.CurrentTile);

            var dragWorldVelocity = drag.CurrentWorldPosition - drag.PrevWorldPosition;
            var didSwipe = dragWorldVelocity.magnitude > SwipeSpeedLimit;

            var alwaysRoundUp = !this.moves.IsEmpty && didSwipe;
            this.moves.RoundLatestProgress(SlowRollSpeed, alwaysRoundUp);
        }
    }
}
