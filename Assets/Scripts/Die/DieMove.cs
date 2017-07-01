using System;
using UnityEngine;

[Serializable]
public class DieMove
{
    public Vector3 Direction { get; private set; }
    public Vector2I TargetTile { get; private set; }
    public float RollSpeed = 1f;
    public float Progress;
    public bool IsFinished;

    public void Initialize(Vector3 direction, Vector2I targetTile)
    {
        this.Direction = direction;
        this.TargetTile = targetTile;
    }

    public bool CheckIsInitialized()
    {
        return this.Direction != Vector3.zero;
    }

    /// <remarks><paramref name="progress"/> should be less than 0.5</remarks>
    public bool IsNearerToFinishing(float progress)
    {
        return this.Progress < progress || this.Progress > (1f - progress);
    }
}
