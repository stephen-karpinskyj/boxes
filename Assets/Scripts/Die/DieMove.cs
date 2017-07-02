using System;
using UnityEngine;

[Serializable]
public class DieMove
{
    public Vector3 Direction;
    public Vector2I TargetTile;
    public float Progress;
    public float RollSpeed;
    public bool IsUser;
    public bool IsFinished;

    public void Reset(float rollSpeed, bool isUser)
    {
        this.Direction.Set(0f, 0f, 0f); // Set won't work for properties...
        this.TargetTile.Set(0, 0);
        this.Progress = 0f;
        this.RollSpeed = rollSpeed;
        this.IsUser = isUser;
        this.IsFinished = false;
    }

    public void Initialize(Vector3 direction, Vector2I targetTile)
    {
        this.Direction = direction;
        this.TargetTile = targetTile;
    }

    public bool CheckIsInitialized()
    {
        return this.Direction != Vector3.zero;
    }

    public bool IsNearerToFinishing(float progress)
    {
        Debug.Assert(progress >= 0f && progress < 0.5f);

        return this.Progress < progress || this.Progress > (1f - progress);
    }
}
