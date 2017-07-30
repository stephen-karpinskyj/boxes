using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardDieSpawner
{
    public const int SpawnDuration = 3;
    public const int DespawnDuration = 3;

    private static IdGenerator DieIdGen;

    [SerializeField]
    private int initialDiceCount = 20;

    private readonly List<Vector2I> allTiles = new List<Vector2I>
    {
        new Vector2I(-4, 5),
        new Vector2I(-3, 5),
        new Vector2I(-2, 5),

        new Vector2I(-5, 4),
        new Vector2I(-4, 4),
        new Vector2I(-3, 4),
        new Vector2I(-2, 4),
        new Vector2I(-1, 4),

        new Vector2I(-5, 3),
        new Vector2I(-4, 3),
        new Vector2I(-3, 3),
        new Vector2I(-2, 3),
        new Vector2I(-1, 3),
        new Vector2I(0, 3),

        new Vector2I(-5, 2),
        new Vector2I(-4, 2),
        new Vector2I(-3, 2),
        new Vector2I(-2, 2),
        new Vector2I(-1, 2),
        new Vector2I(0, 2),
        new Vector2I(1, 2),

        new Vector2I(-4, 1),
        new Vector2I(-3, 1),
        new Vector2I(-2, 1),
        new Vector2I(-1, 1),
        new Vector2I(0, 1),
        new Vector2I(1, 1),
        new Vector2I(2, 1),

        new Vector2I(-3, 0),
        new Vector2I(-2, 0),
        new Vector2I(-1, 0),
        new Vector2I(0, 0),
        new Vector2I(1, 0),
        new Vector2I(2, 0),
        new Vector2I(3, 0),

        new Vector2I(-2, -1),
        new Vector2I(-1, -1),
        new Vector2I(0, -1),
        new Vector2I(1, -1),
        new Vector2I(2, -1),
        new Vector2I(3, -1),
        new Vector2I(4, -1),

        new Vector2I(-1, -2),
        new Vector2I(0, -2),
        new Vector2I(1, -2),
        new Vector2I(2, -2),
        new Vector2I(3, -2),
        new Vector2I(4, -2),
        new Vector2I(5, -2),

        new Vector2I(0, -3),
        new Vector2I(1, -3),
        new Vector2I(2, -3),
        new Vector2I(3, -3),
        new Vector2I(4, -3),
        new Vector2I(5, -3),
        new Vector2I(6, -3),

        new Vector2I(1, -4),
        new Vector2I(2, -4),
        new Vector2I(3, -4),
        new Vector2I(4, -4),
        new Vector2I(5, -4),

        new Vector2I(2, -5),
        new Vector2I(3, -5),
        new Vector2I(4, -5),

        new Vector2I(3, -6),
    };

    private Vector2I spawningTile;

    public void Initialize()
    {
        this.SpawnRandomDice(this.initialDiceCount);
    }

    public bool Contains(Vector2I tile)
    {
        return this.spawningTile == tile;
    }

    public void ChangeTick(int tick)
    {
        if ((tick - 1) % SpawnDuration == 0)
        {
            this.StartSpawningNextDie(this.FindRandomSpawnTile(), tick);
        }
    }

    private Vector2I FindRandomSpawnTile()
    {
        Vector2I tile;

        var foundTile = this.GetAvailableTiles().TryUseRandom(out tile);

        Debug.Assert(foundTile);

        return tile;
    }

    private void SpawnRandomDice(int spawnCount)
    {
        var tilePool = this.GetAvailableTiles();

        Debug.Assert(spawnCount <= tilePool.AvailableCount);

        Vector2I tile;
        for (var i = 0; i < spawnCount; i++)
        {
            if (tilePool.TryUseRandom(out tile))
            {
                var dieState = this.SpawnDie(tile);

                dieState.SpawnTick = 0;

                Debug.Log("[Board] Spawned die=" + dieState.Id + " in tile=" + dieState.Tile + " showing face=" + dieState.CalculateCurrentFace());
            }
        }
    }

    private DieState SpawnDie(Vector2I tile)
    {
        var dieState = Board.Instance.AddDieState(DieIdGen.Next(), GameManager.Instance.Tick);
        dieState.SetFace(DieState.GetRandomFace());
        dieState.Tile = tile;

        return dieState;
    }

    private void StartSpawningNextDie(Vector2I tile, int tick)
    {
        var dieState = this.SpawnDie(tile);

        var spawnTick = tick + SpawnDuration;

        Debug.Log("[Board] Spawning die=" + dieState.Id + " at tick=" + spawnTick + " in tile=" + tile + " showing face=" + dieState.CalculateCurrentFace());

        dieState.SpawnTick = spawnTick;

        this.spawningTile = tile;
    }

    private Pool<Vector2I> GetAvailableTiles()
    {
        var pool = new Pool<Vector2I>();

        foreach (var t in this.allTiles)
        {
            if (Board.Instance.IsTileAvailable(t))
            {
                pool.InsertAvailable(t);
            }
        }

        return pool;
    }
}
