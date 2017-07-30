using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardState
{
    private const int NumUndoTicks = 3;

    private List<TickState> tickStates = new List<TickState>();

    public void Initialize()
    {
        foreach (var tickState in this.tickStates)
        {
            tickState.Clear();
        }

        this.tickStates.Clear();
    }

    public bool ContainsDieState(int dieId, int tick)
    {
        var tickState = this.tickStates.Find(ts => ts.Tick == tick);
        return tickState != null && tickState.Contains(dieId);
    }

    public bool IsOccupied(Vector2I tile, int tick)
    {
        var tickState = this.tickStates.Find(ts => ts.Tick == tick);
        return tickState != null && tickState.IsOccupied(tile);
    }

    public DieState GetDieState(int dieId, int tick)
    {
        return this.GetTickStateOrAdd(tick).GetDieState(dieId);
    }

    public DieState AddDieState(int dieId, int tick)
    {
        return this.GetTickStateOrAdd(tick).AddDieState(dieId);
    }

    public TickState GetTickState(int tick)
    {
        var tickState = this.tickStates.Find(ts => ts.Tick == tick);

        Debug.Assert(tickState != null);

        return tickState;
    }

    public TickState GetTickStateOrAdd(int tick)
    {
        var tickState = this.tickStates.Find(ts => ts.Tick == tick);

        if (tickState == null)
        {
            tickState = new TickState(tick);
            this.tickStates.Add(tickState);
        }

        Debug.Assert(tickState != null);

        return tickState;
    }

    public void ResetTickState(int tick, int originalTick)
    {
        var tickState = this.GetTickStateOrAdd(tick);

        tickState.Clear();

        this.GetTickStateOrAdd(originalTick).Fill(tickState);
    }

    private void RemoveTicks(Predicate<TickState> match)
    {
        foreach (var tickState in this.tickStates.FindAll(match))
        {
            tickState.Clear();
            this.tickStates.Remove(tickState);
        }
    }

    public TickState ChangeTick(TickUpdate update)
    {
        var currentTickState = this.GetTickStateOrAdd(update.Current);
        var isMovingForward = update.Current == update.Previous + 1;

        if (isMovingForward)
        {
            this.GetTickStateOrAdd(update.Previous).Fill(currentTickState);
        }

        currentTickState.Update();

        if (currentTickState.Score > 0)
        {
            GameManager.Instance.AddScore(currentTickState.Score);
        }

        if (isMovingForward)
        {
            currentTickState.Fill(this.GetTickStateOrAdd(update.Current + 1));
        }

        this.RemoveTicks(t => t.Tick > update.Current + 1);
        this.RemoveTicks(t => t.Tick < update.Current - NumUndoTicks);

        return currentTickState;
    }

    public void UpdateTick(TickUpdate update)
    {
        var currentTickState = this.GetTickStateOrAdd(update.Current);
        var nextTickState = this.GetTickStateOrAdd(update.Current + 1);

        currentTickState.Update();
        nextTickState.Update();
    }
}
