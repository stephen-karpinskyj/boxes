using System;
using UnityEngine;

using Random = UnityEngine.Random;

[Serializable]
public class DieState
{
    public Vector3 PrevDragPoint;

    public Vector3 HingePos;
    public Quaternion HingeRot;
    public Vector3 VisualPos;

    public float MoveProgress;

    public Vector2I CurrentTile;

    public int CalculateUpwardFace()
    {
        var rot = this.HingeRot.eulerAngles;
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

    public Quaternion CalculateRandomRotation(int upwardFace)
    {
        Debug.Assert(upwardFace >= 0 || upwardFace <= 6, this);
        
        var rot = Vector3.zero;

        switch (upwardFace)
        {
            case 1:
            case 2:
            case 5:
            case 6:
            {
                rot.x = 0f;

                switch (upwardFace)
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

                switch (upwardFace)
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
}
