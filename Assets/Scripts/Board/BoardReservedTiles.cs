using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

[Serializable]
public class BoardReservedTiles
{
    [SerializeField]
    private bool enableReservedTileDebug;

    [SerializeField]
    private DebugTile debugTilePrefab;

    private readonly Dictionary<Vector2I, DebugTile> debugViews = new Dictionary<Vector2I, DebugTile>();

    private List<Vector2I> tiles = new List<Vector2I>();

    private Transform root;

    public void Initialize(Transform root)
    {
        this.root = root;

        this.tiles.Clear();

        foreach (var kv in this.debugViews)
        {
            Object.Destroy(kv.Value.gameObject);
        }

        this.debugViews.Clear();
    }

    public bool Contains(Vector2I tile)
    {
        return this.tiles.Contains(tile);
    }

    public void Add(Vector2I tile)
    {
        Debug.Assert(!this.Contains(tile));

        //Debug.Log("[Board] Added reserved tile=" + tile);

        this.tiles.Add(tile);

        if (this.enableReservedTileDebug)
        {
            var debug = GameObjectUtility.InstantiatePrefab(this.debugTilePrefab, this.root);
            debug.SetText(tile.ToString());
            debug.gameObject.name = tile.ToString();
            debug.transform.localPosition = new Vector3(tile.x, 0f, tile.y);
            this.debugViews[tile] = debug;
        }
    }

    public void Remove(Vector2I tile)
    {
        Debug.Assert(this.tiles.Contains(tile));

        //Debug.Log("[Board] Removed reserved tile=" + tile);

        this.tiles.Remove(tile);

        if (this.enableReservedTileDebug && this.debugViews.ContainsKey(tile))
        {
            Object.Destroy(this.debugViews[tile].gameObject);
            this.debugViews.Remove(tile);
        }
    }
}
