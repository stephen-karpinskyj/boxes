using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardMoveQueueState
{
    public List<DieMoveQueue> Dice = new List<DieMoveQueue>();

    public bool HasMoves
    {
        get { return this.Dice.Count > 0; }
    }

    public bool CanMove
    {
        get { return this.HasMoves && !Mathf.Approximately(this.Progress, this.MoveProgress); }
    }

    /// <summary>
    /// Progress of this board move queue.
    /// </summary>
    public float Progress;

    /// <summary>
    /// Progress of current moves.
    /// </summary>
    public float MoveProgress;

    /// <summary>
    /// Roll speed of current moves.
    /// </summary>
    public float MoveRollSpeed;
}
