using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DieMoveList
{
    private List<DieMove> moves = new List<DieMove>();

    public Vector2I TargetTile { get; private set; }

    public bool IsEmpty
    {
        get { return this.moves.Count <= 0; }
    }

    public DieMove GetLatest()
    {
        return this.IsEmpty ? null : this.moves[this.moves.Count - 1];
    }

    public DieMove GetLatestOrNew(float speed)
    {
        DieMove move = null;

        if (this.IsEmpty)
        {
            move = new DieMove { RollSpeed = speed };
            this.moves.Add(move);
        }
        else
        {
            move = this.GetLatest();

            if (move.IsFinished)
            {
                move = new DieMove { RollSpeed = speed };
                this.moves.Add(move);
            }
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

        this.TargetTile = oldest == null ? currentTile : oldest.TargetTile;

        return oldest;
    }

    public DieMove AddProgress(float progressDelta, float speed)
    {
        var move = this.GetLatestOrNew(speed);
        
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
                this.TargetTile = CalculateAdjacentTile(this.TargetTile, newDirection);
            }
            else if (Mathf.Approximately(move.Progress, 1f))
            {
                move.IsFinished = true;
            }

            if (move.IsFinished)
            {
                move = this.GetLatestOrNew(speed);
                this.InitializeMove(move, newDirection);
            }
        }

        return move;
    }

    public DieMove RoundLatestProgress(float speed, bool alwaysRoundUp)
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
            if (Mathf.Approximately(0f, move.Progress))
            {
                this.TargetTile = CalculateAdjacentTile(this.TargetTile, -move.Direction);
            }
        }

        return move;
    }

    public bool InitializeMove(DieMove move, params Vector3[] directions)
    {
        var didReserve = false;
        
        foreach (var dir in directions)
        {
            var targetTile = CalculateAdjacentTile(this.TargetTile, dir);
			
            didReserve = Board.Instance.ReserveTile(targetTile);

	        if (didReserve)
	        {
				move.Initialize(dir, targetTile);
                this.TargetTile = targetTile;
                break;
	        }
        }

        return didReserve;
    }

    public static Vector2I CalculateAdjacentTile(Vector2I tile, Vector3 direction)
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

    private void RemoveMove(DieMove move)
    {
        Debug.Assert(this.moves.Contains(move), this);
        
        Board.Instance.UnreserveTile(move.TargetTile);
        this.moves.Remove(move);
    }
}
