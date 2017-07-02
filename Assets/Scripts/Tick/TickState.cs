using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TickState
{
    private List<DieState> dieStates = new List<DieState>();

    private List<BoardDieGroup> dieGroups = new List<BoardDieGroup>();

    private static GenericPool<DieState> dieStatePool = new GenericPool<DieState>();

    public int Tick { get; private set; }
    public int Score { get; private set; }

    public IEnumerable<DieState> DieStates
    {
        get { return this.dieStates; }
    }

    public TickState(int tick)
    {
        this.Tick = tick;
        this.Score = 0;
    }

    public void Clear()
    {
        foreach (var die in this.dieStates)
        {
            dieStatePool.Unuse(die);
        }

        this.dieStates.Clear();

        this.Update();
    }

    public bool Contains(int dieId)
    {
        return this.dieStates.Find(d => d.Id == dieId) != null;
    }

    public bool Contains(Vector2I tile)
    {
        return this.dieStates.Find(d => d.Tile == tile) != null;
    }

    public DieState GetDieState(int dieId)
    {
        var dieState = this.dieStates.Find(d => d.Id == dieId);

        Debug.Assert(dieState != null, "[Board] Cannot find state for die=" + dieId + " at tick=" + this.Tick);

        return dieState;
    }

    public DieState AddDieState(int dieId)
    {
        Debug.Assert(!this.Contains(dieId));

        var dieState = dieStatePool.Use(() => { return new DieState(); });

        dieState.Reset(dieId);
        this.dieStates.Add(dieState);

        return dieState;
    }

    /// <summary>
    /// Fills <paramref name="ticktoFill"/> with anything missing from this tick.
    /// </summary>
    public void Fill(TickState ticktoFill)
    {
        foreach (var dieState in this.DieStates)
        {
            if (!ticktoFill.Contains(dieState.Id))
            {
                var copiedDieState = ticktoFill.AddDieState(dieState.Id);
                dieState.CopyTo(copiedDieState);
            }
        }
    }

    public void Update()
    {
        this.UpdateGroups();
        this.UpdateScore();

        var despawnedCount = this.dieStates.RemoveAll(d => d.CalculateIsDespawned(this.Tick));

        if (despawnedCount > 0)
        {
            Debug.Log("[Board] Despawned dice count=" + despawnedCount + " at tick=" + this.Tick);
        }
    }

    private void UpdateGroups(TickState backupTick = null)
    {
        this.dieGroups.Clear();

        // Create groups
        foreach (var dieStateA in this.dieStates)
        {
            if (dieStateA.CalculateIsSpawning(this.Tick))
            {
                continue;
            }

            var faceA = dieStateA.CalculateCurrentFace();

            if (faceA <= 1)
            {
                continue;
            }

            foreach (var dieStateB in this.dieStates)
            {
                if (dieStateA.Id == dieStateB.Id)
                {
                    continue;
                }

                if (dieStateB.CalculateIsSpawning(this.Tick))
                {
                    continue;
                }

                var faceB = dieStateB.CalculateCurrentFace();

                if (faceB <= 1)
                {
                    continue;
                }

                var isPartOfSameGroup = faceA == faceB && Vector2I.IsAdjacent(dieStateA.Tile, dieStateB.Tile);

                if (!isPartOfSameGroup)
                {
                    continue;
                }

                var groupA = this.dieGroups.Find(g => g.DieStates.Find(d => d.Id == dieStateA.Id) != null);
                var groupB = this.dieGroups.Find(g => g.DieStates.Find(d => d.Id == dieStateB.Id) != null);

                if (groupA == null && groupB == null)
                {
                    this.dieGroups.Add(groupA = new BoardDieGroup { DieFace = faceA });
                }
                else if (groupA == null)
                {
                    groupA = groupB;
                }

                Debug.Assert(groupA != null);

                groupA.AddIfNotAlready(dieStateA);
                groupA.AddIfNotAlready(dieStateB);

                Debug.Assert(groupA.DieStates.Contains(dieStateA));
                Debug.Assert(groupA.DieStates.Contains(dieStateB));
            }
        }

        // Merge groups with common dice
        foreach (var groupA in this.dieGroups)
        {
            if (groupA.DieFace == -1)
            {
                continue;
            }

            foreach (var groupB in this.dieGroups)
            {
                if (groupA == groupB)
                {
                    continue;
                }

                if (groupA.DieFace != groupB.DieFace)
                {
                    continue;
                }

                if (groupB.DieFace == -1)
                {
                    continue;
                }

                if (groupA.IsConnected(groupB))
                {
                    foreach (var dieB in groupB.DieStates)
                    {
                        groupA.AddIfNotAlready(dieB);
                    }

                    groupB.DieStates.Clear();
                    groupB.DieFace = -1;
                }
            }
        }

        this.dieGroups.RemoveAll(g => g.DieFace == -1);

        // Update groups
        foreach (var g in this.dieGroups)
        {
            Debug.Assert(g.DieStates.Count > 0);

            g.Update(this.Tick);
        }
    }

    private void UpdateScore()
    {
        this.Score = 0;

        foreach (var g in this.dieGroups)
        {
            if (g.IsDespawned)
            {
                this.Score += g.Score;
            }
        }
    }
}
