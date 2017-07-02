using UnityEngine;

[RequireComponent(typeof(DieView))]
public class DieInput : MonoBehaviour
{
    private const float DragDistPerRoll = 1.1f;
    private const float DragPlaneHeight = 0.5f;
    private const float MinDragMag = 0.03f;
    private const float RollDirChangeProgressLimit = 0.35f;
    private const float RollDirChangeDiffLimit = 0.03f;
    private const float SwipeSpeedLimit = 0.1f;
    private const float FastRollSpeed = 8f;
    private const float SlowRollSpeed = 4.7f;

    [SerializeField]
    private Collider coll;

    private DieView view;

    private Plane dragPlane;
    private Vector3 prevDragPoint;

    private DieMoveQueue Moves
    {
        get { return Board.Instance.Moves.GetDie(this.view.Id); }
    }

    private DieState State
    {
        get { return Board.Instance.GetDieState(this.view.Id, GameManager.Instance.Tick); }
    }

    private void Awake()
    {
        this.view = this.GetComponent<DieView>();

        this.dragPlane = new Plane(Vector3.up, Vector3.up * DragPlaneHeight);
    }

    private bool CheckInteractable()
    {
        return this.State.CalculateIsSpawned(GameManager.Instance.Tick);
    }

    private DieMove CheckForRollDirectionChange(float xMag, float zMag, float xDir, float zDir, DieMove move)
    {
        var magDiff = Mathf.Abs(xMag - zMag);

        if (magDiff > RollDirChangeDiffLimit)
        {
            if (xMag > zMag && !Mathf.Approximately(move.Direction.z, 0f))
            {
                var newDirection = Vector3.right * xDir;
                var targetTile = this.Moves.CalculateAdjacentTile(newDirection);
                var canMove = Board.Instance.IsTileAvailable(targetTile);

                if (canMove)
                {
                    this.Moves.RoundLatestProgress(SlowRollSpeed, true, false);
                    move = this.Moves.GetLatestOrNew(FastRollSpeed, true);
                    this.Moves.InitializeMove(move, newDirection);
                }
            }
            else if (zMag > xMag && !Mathf.Approximately(move.Direction.x, 0f))
            {
                var newDirection = Vector3.forward * zDir;
                var targetTile = this.Moves.CalculateAdjacentTile(newDirection);
                var canMove = Board.Instance.IsTileAvailable(targetTile);

                if (canMove)
                {
                    this.Moves.RoundLatestProgress(SlowRollSpeed, true, false);
                    move = this.Moves.GetLatestOrNew(FastRollSpeed, true);
                    this.Moves.InitializeMove(move, newDirection);
                }
            }
        }

        return move;
    }

    private Vector3 CalculateDragPoint(WorldInput.DragData drag)
    {
        var dist = 0f;

        this.dragPlane.Raycast(drag.CurrentRay, out dist);
        return drag.CurrentRay.GetPoint(dist);
    }

    private void StartDrag()
    {
        this.view.SetDragging(true);
    }

    private void EndDrag()
    {
        this.view.SetDragging(false);
    }

    private void WorldInput_DragStart(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.coll.gameObject)
        {
            var isInteractable = this.CheckInteractable();

            if (isInteractable)
            {
                this.prevDragPoint = this.CalculateDragPoint(drag);

                this.Moves.ClearAllExceptOldest(this.State.Tile);

                this.StartDrag();
            }
        }
    }

    private void WorldInput_DragUpdate(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.coll.gameObject)
        {
            var isInteractable = this.CheckInteractable();

            if (isInteractable)
            {
                var planePoint = this.CalculateDragPoint(drag);
                Vector3 amountDragged;

                do
                {
                    amountDragged = planePoint - this.prevDragPoint;

                    var xMag = Mathf.Min(DragDistPerRoll, Mathf.Abs(amountDragged.x));
                    var xDir = Mathf.Sign(amountDragged.x);
                    var zMag = Mathf.Min(DragDistPerRoll, Mathf.Abs(amountDragged.z));
                    var zDir = Mathf.Sign(amountDragged.z);

                    if (Mathf.Max(xMag, zMag) > MinDragMag)
                    {
                        var move = this.Moves.GetLatestOrNew(FastRollSpeed, true);

                        if (!move.CheckIsInitialized())
                        {
                            var preferredDirection = xMag > zMag ? Vector3.right * xDir : Vector3.forward * zDir;
                            var fallbackDirection = xMag > zMag ? Vector3.forward * zDir : Vector3.right * xDir;

                            this.Moves.InitializeMove(move, preferredDirection, fallbackDirection);
                        }
                        else if (move.IsNearerToFinishing(RollDirChangeProgressLimit))
                        {
                            move = this.CheckForRollDirectionChange(xMag, zMag, xDir, zDir, move);
                        }

                        if (move.CheckIsInitialized())
                        {
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
                                move = this.Moves.AddProgress(moveProgressDelta, FastRollSpeed, true);
                                this.prevDragPoint += dragDelta;
                            }
                        }
                        else
                        {
                            // Can't move
                            this.prevDragPoint = planePoint;
                            this.Moves.RemoveMove(move);
                            break;
                        }
                    }
                }
                while (Mathf.Abs(Mathf.Max(amountDragged.x, amountDragged.z)) > MinDragMag);
            }
            else
            {
                this.Moves.ClearAll();

                this.EndDrag();
            }
        }
    }

    private void WorldInput_DragEnd(WorldInput.DragData drag)
    {
        if (drag.DraggedGO == this.coll.gameObject)
        {
            var isInteractable = this.CheckInteractable();

            if (isInteractable)
            {
                this.Moves.ClearAllExceptOldest(this.State.Tile);

                var dragWorldVelocity = drag.CurrentWorldPosition - drag.PrevWorldPosition;
                var didSwipe = dragWorldVelocity.magnitude > SwipeSpeedLimit;

                var alwaysRoundUp = !this.Moves.IsEmpty && didSwipe;
                this.Moves.RoundLatestProgress(SlowRollSpeed, true, alwaysRoundUp);

                this.EndDrag();
            }
        }
    }
}
