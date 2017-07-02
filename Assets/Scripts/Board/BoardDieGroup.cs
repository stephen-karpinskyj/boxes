using System;
using System.Collections.Generic;

[Serializable]
public class BoardDieGroup
{
    public int DieFace;
    public List<DieState> DieStates = new List<DieState>();

    public bool IsDespawned { get; private set; }
    public int Score { get; private set; }

    public void AddIfNotAlready(DieState die)
    {
        if (this.DieStates.Find(d => d.Id == die.Id) == null)
        {
            this.DieStates.Add(die);
        }
    }

    public void Update(int tick)
    {
        var isDespawning = this.DieFace > 1 && this.DieStates.Count >= this.DieFace;

        if (isDespawning)
        {
            var areAllDiceDespawning = this.DieStates.Find(d => d.DespawnTick == -1) == null;

            if (!areAllDiceDespawning)
            {
                foreach (var dieState in this.DieStates)
                {
                    dieState.DespawnTick = tick;
                }
            }

            this.IsDespawned = this.DieStates.Find(d => d.CalculateIsDespawned(tick)) != null;
            this.Score = this.DieFace * this.DieStates.Count;
        }
    }

    public bool IsConnected(BoardDieGroup otherGroup)
    {
        foreach (var die in this.DieStates)
        {
            foreach (var otherDie in otherGroup.DieStates)
            {
                var isInOtherGroup = die.Id == otherDie.Id;

                if (isInOtherGroup)
                {
                    return true;
                }

                var isConnectedToOtherGroup = Vector2I.IsAdjacent(die.Tile, otherDie.Tile);

                if (isConnectedToOtherGroup)
                {
                    return true;
                }
            }
        }

        return false;
    }
}