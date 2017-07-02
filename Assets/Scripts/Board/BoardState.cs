using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardState
{
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

    public bool ContainsDieState(Vector2I tile, int tick)
    {
        var tickState = this.tickStates.Find(ts => ts.Tick == tick);
        return tickState != null && tickState.Contains(tile);
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

    private void RemoveLaterTicks(int tick)
    {
        foreach (var tickState in this.tickStates.FindAll(t => t.Tick > tick))
        {
            tickState.Clear();
            this.tickStates.Remove(tickState);
        }
    }

    public TickState ChangeTick(TickUpdate update)
    {
        var currentTickState = this.GetTickStateOrAdd(update.Current);

        if (update.Current == update.Previous + 1)
        {
            // Copy previous tick to new tick
            this.GetTickStateOrAdd(update.Previous).Fill(currentTickState);
        }

        currentTickState.Update();

        if (currentTickState.Score > 0)
        {
            GameManager.Instance.AddScore(currentTickState.Score);
        }

        this.RemoveLaterTicks(update.Current);

        // TODO: Remove any states for ticks before 3 ticks ago
        // TODO: Return removed dieState's to pool

        return currentTickState;
    }

    public void UpdateTick(TickUpdate update)
    {
        var currentTickState = this.GetTickStateOrAdd(update.Current);

        currentTickState.Update();
    }
}
