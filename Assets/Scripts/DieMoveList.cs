using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DieMoveList
{
    private List<DieMove> moves = new List<DieMove>();

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

    public void RemoveOldest()
    {
        if (!this.IsEmpty)
        {
            this.moves.RemoveAt(0);
        }
    }

    public void ClearAllExceptOldest()
    {
        if (!this.IsEmpty)
        {
	        var oldest = this.GetOldest();
	        
            this.moves.Clear();

            this.moves.Add(oldest);
        }
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

            if (Mathf.Approximately(move.Progress, 0f))
            {
                move.IsFinished = true;
                var newDirection = -move.Direction;

                move = this.GetLatestOrNew(speed);
                move.Direction = newDirection;
                progressDelta *= -1;
            }
            else if (Mathf.Approximately(move.Progress, 1f))
            {
                move.IsFinished = true;
                var newDirection = move.Direction;

                move = this.GetLatestOrNew(speed);
                move.Direction = newDirection;
            }
        }

        return move;
    }

    public void RoundLatestProgress(float speed, bool alwaysRoundUp)
    {
        if (!this.IsEmpty)
        {
	        var move = this.GetLatest();
	        
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
        }
    }
}
