using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardMoveQueue
{
    private GenericPool<DieMoveQueue> dicePool = new GenericPool<DieMoveQueue>();

    private BoardMoveQueueState state = new BoardMoveQueueState();

    public void Initialize()
    {
        foreach (var d in new List<DieMoveQueue>(dicePool.Used))
        {
            this.RemoveDie(d.Id);
        }

        Debug.Assert(this.dicePool.UsedCount <= 0);
    }

    public DieMoveQueue GetDie(int id)
    {
        var dieQueue = this.dicePool.FindUsed(q => q.Id == id);

        if (dieQueue == null)
        {
            dieQueue = this.dicePool.Use(() => { return new DieMoveQueue(); });
            dieQueue.Initialize(id);
        }

        Debug.Assert(dieQueue != null);

        return dieQueue;
    }

    public void RemoveDie(int id)
    {
        var dieQueue = this.dicePool.FindUsed(q => q.Id == id);

        if (dieQueue != null)
        {
            dieQueue.ClearAll();
            this.dicePool.Unuse(dieQueue);
        }
    }

    public void Update(float deltaTime)
    {
        this.UpdateState();

        do
        {
            if (this.state.HasMoves)
            {
                var maxDeltaProgress = deltaTime * this.state.MoveRollSpeed;
                var deltaMoveProgress = Mathf.Clamp(this.state.MoveProgress - this.state.Progress, -maxDeltaProgress, maxDeltaProgress);

                this.state.Progress += deltaMoveProgress;
                Debug.Assert(this.state.Progress >= 0f && this.state.Progress <= 1f, this);

                deltaTime -= Mathf.Abs(deltaMoveProgress / this.state.MoveRollSpeed);

                var isMovingToNextTick = Mathf.Approximately(this.state.Progress, 1f);
                var isMovingToStartOfTick = Mathf.Approximately(this.state.Progress, 0f);
                var isEndingTick = isMovingToNextTick || isMovingToStartOfTick;

                if (isEndingTick)
                {
                    if (isMovingToNextTick)
                    {
                        GameManager.Instance.EndTick();
                    }

                    if (isMovingToStartOfTick)
                    {
                        Board.Instance.ClearTickState(GameManager.Instance.Tick + 1); // Erase future tick
                    }

                    this.state.Progress = 0f;
                }

                GameManager.Instance.UpdateTick(this.state.Progress);

                foreach (var dieQueue in this.state.Dice)
                {
                    if (isEndingTick)
                    {
                        dieQueue.RemoveOldest();
                    }
                    else
                    {
                        this.UpdateNextTickDieState(dieQueue, GameManager.Instance.Tick);
                    }
                }

                this.UpdateState();
            }
        }
        while (this.state.CanMove && deltaTime > 0f);
    }

    private void UpdateState()
    {
        this.state.Dice.Clear();
        this.state.MoveProgress = 0f;
        this.state.MoveRollSpeed = 0f;

        var hasUserMove = false;

        foreach (var dieQueue in this.dicePool.Used)
        {
            if (!dieQueue.IsEmpty)
            {
                var oldest = dieQueue.GetOldest();

                hasUserMove |= oldest.IsUser;

                if (oldest.IsUser || !hasUserMove)
                {
                    this.state.MoveProgress = oldest.Progress;

                    if (this.state.MoveRollSpeed < oldest.RollSpeed)
                    {
                        this.state.MoveRollSpeed = oldest.RollSpeed;
                    }
                }

                this.state.Dice.Add(dieQueue);
            }
        }
    }

    private void UpdateNextTickDieState(DieMoveQueue dieQueue, int tick)
    {
        var oldest = dieQueue.GetOldest();

        if (oldest.CheckIsInitialized())
        {
            var dieState = Board.Instance.GetDieState(dieQueue.Id, tick);

            if (dieState.CalculateIsSpawned(tick))
            {
                DieState nextDieState;
                var nextTick = tick + 1;

                if (Board.Instance.ContainsDieState(dieQueue.Id, nextTick))
                {
                    nextDieState = Board.Instance.GetDieState(dieQueue.Id, nextTick);
                }
                else
                {
                    nextDieState = Board.Instance.AddDieState(dieQueue.Id, nextTick);
                }

                Debug.Assert(nextDieState != null, this);

                nextDieState.Tile = oldest.TargetTile;
                nextDieState.Rotation = DieState.CalculateAdjacentRotation(dieState.Rotation, oldest.Direction);
            }
        }
    }
}