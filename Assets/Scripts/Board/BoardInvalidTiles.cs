using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardInvalidTiles
{
    [SerializeField]
    private Transform invalidTilePrefab;

    private readonly List<Vector2I> tiles = new List<Vector2I>
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

    private readonly Dictionary<Vector2I, Transform> views = new Dictionary<Vector2I, Transform>();

    public void Initialize(Transform root)
    {
        if (this.views.Count <= 0)
        {
            foreach (var tile in this.tiles)
            {
                var invalid = GameObjectUtility.InstantiatePrefab(this.invalidTilePrefab, root);
                invalid.gameObject.name = string.Format("Invalid {0}", tile);
                invalid.localPosition = new Vector3(tile.x, 0f, tile.y);
                this.views[tile] = invalid;
            }
        }
    }

    public bool Contains(Vector2I tile)
    {
        return this.tiles.Contains(tile);
    }
}
