using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : BehaviourSingleton<Board>
{
    [SerializeField]
    private int initialDiceCount = 20;
    
    [SerializeField]
    private Die diePrefab;

    [SerializeField]
    private Transform invalidTilePrefab;

    private readonly Dictionary<Vector2I, Transform> invalidTileViews = new Dictionary<Vector2I, Transform>();


    #region Debug


    [Header("Debug")]

    [SerializeField]
    private bool enableReservedTileDebug;

    [SerializeField]
    private Transform debugTilePrefab;

    private readonly Dictionary<Vector2I, Transform> debugReservedTileViews = new Dictionary<Vector2I, Transform>();


    #endregion


    private readonly Dictionary<Die, Vector2I> dice = new Dictionary<Die, Vector2I>();
    private readonly List<Vector2I> reservedTiles = new List<Vector2I>();

    private readonly List<Vector2I> invalidTiles = new List<Vector2I>
    {
        // Right/left edge
        new Vector2I(-2, 6),
        new Vector2I(-1, 5),
        new Vector2I(0, 4),
        new Vector2I(1, 3),
        new Vector2I(2, 2),
        new Vector2I(3, 1),
        new Vector2I(4, 0),
        new Vector2I(5, -1),
        new Vector2I(6, -2),
        new Vector2I(7, -3),

        // Bottom edge
        new Vector2I(6, -4),
        new Vector2I(5, -5),
        new Vector2I(4, -6),
        new Vector2I(3, -7),

        // left edge
        new Vector2I(2, -6),
        new Vector2I(1, -5),
        new Vector2I(0, -4),
        new Vector2I(-1, -3),
        new Vector2I(-2, -2),
        new Vector2I(-3, -1),
        new Vector2I(-4, 0),
        new Vector2I(-5, 1),
        new Vector2I(-6, 2),

        // Top edge
        new Vector2I(-7, 4),
        new Vector2I(-7, 3),
        new Vector2I(-6, 3),
        new Vector2I(-6, 4),
        new Vector2I(-6, 5),
		new Vector2I(-5, 5),
        new Vector2I(-5, 6),
        new Vector2I(-4, 6),
        new Vector2I(-3, 6),
        new Vector2I(-3, 7),
        new Vector2I(-4, 7),
    };

    private void OnEnable()
    {
        GameManager.Instance.OnGameStart += this.HandleGameStart;
    }

    private void OnDisable()
    {
        if (GameManager.Exists)
        {
            GameManager.Instance.OnGameStart -= this.HandleGameStart;
        }
    }

    public bool ReserveTile(Vector2I tile)
    {
        if (this.IsTileAvailable(tile))
        {
	        //Debug.Log("[Board] Reserved tile=" + tile, this);

	        this.reservedTiles.Add(tile);

	        if (this.enableReservedTileDebug)
	        {
                var debug = GameObjectUtility.InstantiatePrefab(this.debugTilePrefab, this.transform);
	            debug.gameObject.name = tile.ToString();
	            debug.localPosition = new Vector3(tile.x, 0f, tile.y);
                this.debugReservedTileViews[tile] = debug;
	        }

            return true;
        }

        return false;
    }

    public void UnreserveTile(Vector2I tile)
    {
        var index = this.reservedTiles.IndexOf(tile);

        if (index >= 0)
        {
	        //Debug.Log("[Board] Unreserved tile=" + tile, this);

	        this.reservedTiles.RemoveAt(index);
        }

        if (this.enableReservedTileDebug && this.debugReservedTileViews.ContainsKey(tile))
        {
            Object.Destroy(this.debugReservedTileViews[tile].gameObject);
            this.debugReservedTileViews.Remove(tile);
        }
    }

    public void EnterTile(Die die, Vector2I tile)
    {
        Debug.Assert(!this.dice.Any(kv => kv.Key != die && kv.Value == tile), this);
        Debug.Assert(!this.reservedTiles.Contains(tile), this);

		this.dice[die] = tile;

        Debug.Log("[Board] Die=" + die.Id + " entered tile=" + tile, this);
    }

    public bool IsTileAvailable(Vector2I tile)
    {
        if (this.invalidTiles.Contains(tile))
        {
            return false;
        }
        
        if (this.dice.Any(kv => kv.Value == tile))
        {
            return false;
        }

        if (this.reservedTiles.Contains(tile))
        {
            return false;
        }

        return true;
    }

    private void HandleGameStart()
    {
        if (Board.Instance != this)
        {
            Object.Destroy(this.gameObject);
            return;
        }

        // Debug tiles
        {
	        foreach (var kv in this.debugReservedTileViews)
	        {
	            Object.Destroy(kv.Value.gameObject);
	        }
	        this.debugReservedTileViews.Clear();
        }

        // Invalid tiles
        {
	        foreach (var kv in this.invalidTileViews)
	        {
	            Object.Destroy(kv.Value.gameObject);
	        }
	        this.invalidTileViews.Clear();

	        foreach (var tile in this.invalidTiles)
	        {
	            var invalid = GameObjectUtility.InstantiatePrefab(this.invalidTilePrefab, this.transform);
	            invalid.gameObject.name = string.Format("Invalid {0}", tile);
	            invalid.localPosition = new Vector3(tile.x, 0f, tile.y);
	            this.invalidTileViews[tile] = invalid;
	        }
        }

        // Reserved tiles
        {
            this.reservedTiles.Clear();
        }

        // Dice
        {
            foreach (var kv in this.dice)
            {
                Object.Destroy(kv.Key.gameObject);
            }
            this.dice.Clear();

            var tilePool = new GenericPool<Vector2I>(new List<Vector2I>
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
            });

            Debug.Assert(this.initialDiceCount <= tilePool.AvailableCount, this);

            Vector2I tile;
            for (var i = 0; i < this.initialDiceCount; i++)
            {
                if (tilePool.TryUseRandom(out tile))
                {
                    var die = GameObjectUtility.InstantiatePrefab(this.diePrefab, this.transform);
                    die.gameObject.name = string.Format("Die {0}", tile);
                    die.transform.localPosition = new Vector3(tile.x, 0.5f, tile.y);
                }
            }
        }
    }
}
