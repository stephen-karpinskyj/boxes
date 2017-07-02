using UnityEngine;

[SelectionBase]
public class DieView : MonoBehaviour
{
    private const float MinSpawnHeight = -0.99f;
    private const float SelectedEmissionAmount = 0.07f;
    private const float GlowingEmissionAmount = 0.25f;
    private const string EmissionColorPropertyName = "_EmissionColor";

    [SerializeField]
    private Transform hingeParent;

    [SerializeField]
    private Transform visualParent;

    [SerializeField]
    private Renderer rend;

    [SerializeField]
    private AnimationCurve rollCurve;

    private float size;
    private float halfSize;

    private bool isDragging;
    private bool isDespawning;

    public int Id { get; private set; }

    private void Awake()
    {
        this.size = this.transform.localScale.x;
        this.halfSize = this.size / 2f;
    }

    public void Show(int dieId)
    {
        this.Id = dieId;

        this.gameObject.name = string.Format("Die [{0}]", dieId);
        this.gameObject.SetActive(true);

        this.SetHeight(0f);
    }

    public void Hide()
    {
        this.gameObject.name = string.Format("Die [?]");
        this.gameObject.SetActive(false);
    }

    public void UpdateTick(TickUpdate update, DieState dieState, DieState nextDieState)
    {
        Debug.Assert(this.Id == dieState.Id, this);

        var direction = dieState.CalculateDirection(nextDieState);

        this.UpdateRoll(dieState, update.Progress, direction);

        var currentHeight = this.CalculateSpawnHeight(update.Current, dieState);
        var nextHeight = this.CalculateSpawnHeight(update.Current + 1, nextDieState == null ? dieState : nextDieState);

        this.SetHeight(Mathf.Lerp(currentHeight, nextHeight, update.Progress));

        this.isDespawning = dieState.CalculateIsDespawning(update.Current + update.Progress);
        this.UpdateGlowing();
    }

    public void SetDragging(bool dragging)
    {
        this.isDragging = dragging;
        this.UpdateGlowing();
    }

    private void UpdateGlowing()
    {
        var isGlowing = this.isDragging || this.isDespawning;
        this.SetEmission(isGlowing ? GlowingEmissionAmount : 0f);
    }

    private void SetHeight(float yPos)
    {
        var pos = this.transform.position;
        pos.y = 0.5f + yPos;
        this.transform.position = pos;
    }

    private float CalculateSpawnHeight(float tick, DieState dieState)
    {
        var percentageSpawned = 1f;

        if (dieState.CalculateIsSpawning(tick))
        {
            percentageSpawned = 1f - (dieState.SpawnTick - tick) / BoardDieSpawner.SpawnDuration;
        }
        else if (dieState.CalculateIsDespawning(tick))
        {
            percentageSpawned = 1f - (tick - dieState.DespawnTick) / BoardDieSpawner.DespawnDuration;
        }

        return Mathf.Lerp(MinSpawnHeight, 0f, percentageSpawned);
    }

    private void SetEmission(float emission)
    {
        Debug.Assert(emission >= 0f && emission <= 1f, this);

        var emissionColor = new Color(emission, emission, emission);
        this.rend.material.SetColor(EmissionColorPropertyName, emissionColor);
    }

    private void UpdateRoll(DieState dieState, float progress, Vector3 direction = default(Vector3))
    {
        this.hingeParent.rotation = dieState.Rotation;

        var position = new Vector3(dieState.Tile.x, this.transform.position.y, dieState.Tile.y);

        // Hinge position
        {
            var hingePos = position + direction * this.halfSize;
            hingePos.y = 0f;

            this.hingeParent.position = hingePos;
        }

        // Position
        {
            this.visualParent.position = position;
        }

        // Hinge rotation
        {
            var curvedProgress = this.rollCurve.Evaluate(progress);
            var targetRotation = DieState.CalculateAdjacentRotation(dieState.Rotation, direction);

            this.hingeParent.rotation = Quaternion.Slerp(dieState.Rotation, targetRotation, curvedProgress);
        }
    }
}
