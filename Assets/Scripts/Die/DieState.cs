using System;
using UnityEngine;

using Random = UnityEngine.Random;

[Serializable]
public class DieState
{
    private const int MaxFace = 6;

    public int Id { get; private set; }
    public Vector2I Tile;
    public Quaternion Rotation;
    public int SpawnTick;
    public int DespawnTick;

    public void Reset(int id)
    {
        this.Id = id;
        this.Tile.Set(0, 0);
        this.Rotation = Quaternion.identity;
        this.SpawnTick = -1;
        this.DespawnTick = -1;
    }

    public void CopyTo(DieState other)
    {
        Debug.Assert(other.Id == this.Id);

        other.Tile = this.Tile;
        other.Rotation = this.Rotation;
        other.SpawnTick = this.SpawnTick;
        other.DespawnTick = this.DespawnTick;
    }

    public int CalculateCurrentFace()
    {
        return CalculateFace(this.Rotation);
    }

    /// <summary>
    /// Whether this die has fully spawned onto the board at <paramref name="tick"/>.
    /// </summary>
    public bool CalculateIsSpawned(float tick)
    {
        return !this.CalculateIsSpawning(tick) && !this.CalculateIsDespawning(tick);
    }

    /// <summary>
    /// Whether this die is spawning onto the board at <paramref name="tick"/>.
    /// </summary>
    public bool CalculateIsSpawning(float tick)
    {
        return !Mathf.Approximately(this.SpawnTick, -1f) && tick < this.SpawnTick;
    }

    /// <summary>
    /// Whether this die has fully despawned off the board at <paramref name="tick"/>.
    /// </summary>
    public bool CalculateIsDespawned(float tick)
    {
        return !Mathf.Approximately(this.DespawnTick, -1f) && tick >= this.DespawnTick + BoardDieSpawner.DespawnDuration;
    }

    /// <summary>
    /// Whether this die is despawning off the board at <paramref name="tick"/>.
    /// </summary>
    public bool CalculateIsDespawning(float tick)
    {
        return !Mathf.Approximately(this.DespawnTick, -1f) && tick >= this.DespawnTick;
    }

    public void SetFace(int face)
    {
        Debug.Assert(face > 0 && face <= MaxFace);

        this.Rotation = CalculateRandomRotation(face);
    }

    public Vector3 CalculateDirection(DieState other)
    {
        var direction = Vector3.zero;

        if (other != null)
        {
            if (other.Tile.x != this.Tile.x)
            {
                direction.x = Mathf.Sign(other.Tile.x - this.Tile.x);
            }

            if (other.Tile.y != this.Tile.y)
            {
                direction.z = Mathf.Sign(other.Tile.y - this.Tile.y);
            }
        }

        return direction;
    }

    public static int CalculateFace(Quaternion rotation)
    {
        var rot = rotation.eulerAngles;

        var x = MathUtility.ClampAngle(Mathf.Round(rot.x / 90f) * 90f, -180f, 180f);
        var z = MathUtility.ClampAngle(Mathf.Round(rot.z / 90f) * 90f, -180f, 180f);

        if (Mathf.Approximately(x, 0f))
        {
            if (Mathf.Approximately(z, 0f))
            {
                return 1;
            }

            if (Mathf.Approximately(z, 90f))
            {
                return 2;
            }

            if (Mathf.Approximately(z, -90f))
            {
                return 5;
            }

            if (Mathf.Approximately(z, 180f))
            {
                return 6;
            }
        }

        if (Mathf.Approximately(z, 0f))
        {
            if (Mathf.Approximately(x, -90f))
            {
                return 3;
            }

            if (Mathf.Approximately(x, 90f))
            {
                return 4;
            }
        }

        throw new NotSupportedException();
    }

    public static Quaternion CalculateAdjacentRotation(Quaternion rotation, Vector3 direction = default(Vector3))
    {
        if (!Mathf.Approximately(direction.x, 0f))
        {
            return Quaternion.Euler(0f, 0f, Mathf.Sign(direction.x) * -90f) * rotation;
        }

        if (!Mathf.Approximately(direction.z, 0f))
        {
            return Quaternion.Euler(Mathf.Sign(direction.z) * 90f, 0f, 0f) * rotation;
        }

        return rotation;
    }

    public static Quaternion CalculateRandomRotation(int face)
    {
        Debug.Assert(face > 0 || face <= MaxFace);

        var rot = Vector3.zero;

        switch (face)
        {
            case 1:
            case 2:
            case 5:
            case 6:
                {
                    rot.x = 0f;

                    switch (face)
                    {
                        case 1: rot.z = 0f; break;
                        case 2: rot.z = 90f; break;
                        case 5: rot.z = -90f; break;
                        case 6: rot.z = 180f; break;
                        default: throw new NotSupportedException();
                    }
                }
                break;

            case 3:
            case 4:
                {
                    rot.z = 0f;

                    switch (face)
                    {
                        case 3: rot.x = -90f; break;
                        case 4: rot.x = 90f; break;
                        default: throw new NotSupportedException();
                    }
                }
                break;

            default: throw new NotSupportedException();
        }

        rot.y = Mathf.Round(Random.Range(-180f, 180f) / 90f) * 90f;

        return Quaternion.Euler(rot);
    }

    public static int GetRandomFace()
    {
        return Random.Range(1, MaxFace + 1);
    }
}
