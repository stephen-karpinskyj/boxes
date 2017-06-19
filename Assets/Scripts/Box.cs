using UnityEngine;
using DG.Tweening;

public class Box : MonoBehaviour
{
    [SerializeField]
    private Transform visualParent;

    private float size;
    private float halfSize;

    private void Awake()
    {
        this.size = this.transform.localScale.x;
        this.halfSize = this.size / 2f;
    }

    private void Start()
    {
        GameManager.Instance.OnTick += this.HandleTick;
    }

    private void OnDestroy()
    {
        if (GameManager.Exists)
        {
            GameManager.Instance.OnTick -= this.HandleTick;
        }
    }

    private void Move(float duration, Vector2 dir)
    {
        Debug.Log("[Box] Moving dir=" + dir + ", duration=" + duration, this);

        var visualPos = this.visualParent.position;
        var pivotPos = visualPos;
        var addedPivotRot = Vector3.zero;

        if (!Mathf.Approximately(dir.x, 0f))
        {
            pivotPos.x += Mathf.Sign(dir.x) * this.halfSize;
            addedPivotRot.z -= Mathf.Sign(dir.x) * 90f;
        }
        else if (!Mathf.Approximately(dir.y, 0f))
        {
            pivotPos.z += Mathf.Sign(dir.y) * this.halfSize;
            addedPivotRot.x += Mathf.Sign(dir.y) * 90f;
        }

        pivotPos.y = 0f;

        this.transform.position = pivotPos;
        this.visualParent.position = visualPos;

        this.transform.DORotate(addedPivotRot, duration, RotateMode.WorldAxisAdd)
            .SetEase(Ease.InQuad);
    }

    private void MoveRandom(float duration)
    {
        var dir = Vector2.zero;

        switch (Random.Range(0, 4))
        {
            case 0: dir.x += 1f; break;
            case 1: dir.x -= 1f; break;
            case 2: dir.y += 1f; break;
            case 3: dir.y -= 1f; break;
        }

        this.Move(duration, dir);
    }

    private void HandleTick()
    {
        this.MoveRandom(0.5f);
    }
}
