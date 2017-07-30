using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DieMoveQueue
{
    private static readonly Pool<DieMove> MovePool = new Pool<DieMove>();

    public int Id { get; private set; }

    private List<DieMove> moves = new List<DieMove>();

    private Vector2I currentTile;

    public bool IsEmpty
    {
        get { return this.moves.Count <= 0; }
    }

    public void Initialize(int id)
    {
        Debug.Assert(this.IsEmpty);

        this.Id = id;
    }

    public void RemoveMove(DieMove move)
    {
        Debug.Assert(this.moves.Contains(move));

        if (move.CheckIsInitialized())
        {
            Board.Instance.UnreserveTile(move.TargetTile);
        }

        var removed = this.moves.Remove(move);

        Debug.Assert(removed);

        MovePool.Unuse(move);
    }

    public DieMove GetLatest()
    {
        return this.IsEmpty ? null : this.moves[this.moves.Count - 1];
    }

    public DieMove GetLatestOrNew(float speed, bool isUser)
    {
        var move = this.IsEmpty ? null : this.GetLatest();

        if (move == null || move.IsFinished)
        {
            move = MovePool.Use(() => { return new DieMove(); });
            move.Reset(speed, isUser);
            this.moves.Add(move);
        }

        return move;
    }

    public DieMove GetOldest()
    {
        return this.IsEmpty ? null : this.moves[0];
    }

    public DieMove RemoveOldest()
    {
        var oldest = this.GetOldest();

        if (oldest != null)
        {
            this.RemoveMove(oldest);
        }

        return oldest;
    }

    public DieMove ClearAllExceptOldest(Vector2I currentTile)
    {
        var oldest = this.GetOldest();

        DieMove latest;

        while (!this.IsEmpty && (latest = this.GetLatest()) != oldest)
        {
            this.RemoveMove(latest);
        }

        this.currentTile = currentTile;

        return oldest;
    }

    public void ClearAll()
    {
        while (!this.IsEmpty)
        {
            this.RemoveOldest();
        }
    }

    public DieMove AddProgress(float progressDelta, float speed, bool isUser)
    {
        var move = this.GetLatestOrNew(speed, isUser);

        while (!Mathf.Approximately(progressDelta, 0f))
        {
            var moveProgressDelta = progressDelta > 0f ?
                Mathf.Min(1f - move.Progress, progressDelta) :
                     Mathf.Max(0f - move.Progress, progressDelta);

            progressDelta -= moveProgressDelta;
            move.Progress += moveProgressDelta;
            Debug.Assert(move.Progress >= 0f && move.Progress <= 1f);

            var newDirection = move.Direction;

            if (Mathf.Approximately(move.Progress, 0f))
            {
                newDirection *= -1;
                progressDelta *= -1;
                move.IsFinished = true;
            }
            else if (Mathf.Approximately(move.Progress, 1f))
            {
                move.IsFinished = true;
                this.currentTile = move.TargetTile;
            }

            if (move.IsFinished)
            {
                move = this.GetLatestOrNew(speed, isUser);
                this.InitializeMove(move, newDirection);
            }
        }

        return move;
    }

    public DieMove RoundLatestProgress(float speed, bool isUser, bool alwaysRoundUp)
    {
        var move = this.GetLatest();

        if (move != null)
        {
            move.RollSpeed = speed;

            if (alwaysRoundUp)
            {
                move.Progress = 1f;
            }
            else
            {
                move.Progress = Mathf.Round(move.Progress);
            }

            move.IsFinished = true;
            move.IsUser = isUser;
            if (Mathf.Approximately(1f, move.Progress)) // Only if moved
            {
                this.currentTile = move.TargetTile;
            }
        }

        return move;
    }

    public bool InitializeMove(DieMove move, params Vector3[] directions)
    {
        var didReserve = false;

        foreach (var dir in directions)
        {
            var target = this.CalculateAdjacentTile(dir);

            didReserve = Board.Instance.ReserveTile(target);

            if (didReserve)
            {
                move.Initialize(dir, target);
                break;
            }
        }

        return didReserve;
    }

    public Vector2I CalculateAdjacentTile(Vector3 direction)
    {
        return CalculateAdjacentTile(this.currentTile, direction);
    }

    private static Vector2I CalculateAdjacentTile(Vector2I tile, Vector3 direction)
    {
        if (!Mathf.Approximately(0f, direction.x))
        {
            tile.x += Mathf.RoundToInt(direction.x);
        }
        else if (!Mathf.Approximately(0f, direction.z))
        {
            tile.y += Mathf.RoundToInt(direction.z);
        }

        return tile;
    }
}
