using System;
using UnityEngine;

[Serializable]
public class DieMove
{
    public Vector3 Direction;
    public float RollSpeed = 1f;
    public float Progress;
    public bool IsFinished;

    /// <remarks><paramref name="progress"/> should be less than 0.5</remarks>
    public bool IsNearerToFinishing(float progress)
    {
        return this.Progress < progress || this.Progress > (1f - progress);
    }
}
