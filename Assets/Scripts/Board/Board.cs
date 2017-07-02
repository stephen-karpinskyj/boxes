using UnityEngine;

public class Board : BehaviourSingleton<Board>
{
    [SerializeField]
    private BoardState state;

    [SerializeField]
    private BoardView view;

    [SerializeField]
    private BoardInvalidTiles invalidTiles;

    [SerializeField]
    private BoardReservedTiles reservedTiles;

    [SerializeField]
    private BoardDieSpawner spawner;

    [SerializeField]
    private BoardMoveQueue moves;

    private Transform invalidTilesRoot;
    private Transform reservedTilesRoot;
    private Transform diceRoot;

    public BoardMoveQueue Moves { get { return this.moves; } }

    private void Awake()
    {
        this.invalidTilesRoot = GameObjectUtility.InstantiateGameObject("Invalid", this.transform).transform;
        this.reservedTilesRoot = GameObjectUtility.InstantiateGameObject("Reserved", this.transform).transform;
        this.diceRoot = GameObjectUtility.InstantiateGameObject("Dice", this.transform).transform;
    }

    private void Update()
    {
        this.moves.Update(Time.smoothDeltaTime);
    }

    private void OnEnable()
    {
        if (Board.Instance != this)
        {
            Object.Destroy(this.gameObject);
            return;
        }

        GameManager.Instance.OnGameStart += this.HandleGameStart;
        GameManager.Instance.OnTickChange += this.HandleTickChange;
        GameManager.Instance.OnTickUpdate += this.HandleTickUpdate;
    }

    private void OnDisable()
    {
        if (GameManager.Exists)
        {
            GameManager.Instance.OnGameStart -= this.HandleGameStart;
            GameManager.Instance.OnTickChange -= this.HandleTickChange;
            GameManager.Instance.OnTickUpdate -= this.HandleTickUpdate;
        }
    }

    public bool ReserveTile(Vector2I tile)
    {
        var isAvailable = this.IsTileAvailable(tile);

        if (isAvailable)
        {
            this.reservedTiles.Add(tile);
        }

        return isAvailable;
    }

    public void UnreserveTile(Vector2I tile)
    {
        this.reservedTiles.Remove(tile);
    }

    public bool IsTileAvailable(Vector2I tile)
    {
        // TODO: Query availability at particular tick

        if (this.state.ContainsDieState(tile, GameManager.Instance.Tick))
        {
            return false;
        }

        if (this.invalidTiles.Contains(tile))
        {
            return false;
        }

        if (this.spawner.Contains(tile))
        {
            return false;
        }

        if (this.reservedTiles.Contains(tile))
        {
            return false;
        }

        return true;
    }

    public bool ContainsDieState(int dieId, int tick)
    {
        return this.state.ContainsDieState(dieId, tick);
    }

    public DieState AddDieState(int dieId, int tick)
    {
        return this.state.AddDieState(dieId, tick);
    }

    public DieState GetDieState(int dieId, int tick)
    {
        return this.state.GetDieState(dieId, tick);
    }

    public void ClearTickState(int tick)
    {
        this.state.GetTickStateOrAdd(tick).Clear();
    }

    private void HandleGameStart()
    {
        this.state.Initialize();

        this.view.Initialize(this.diceRoot);

        this.invalidTiles.Initialize(this.invalidTilesRoot);

        this.reservedTiles.Initialize(this.reservedTilesRoot);

        this.spawner.Initialize();

        this.moves.Initialize();
    }

    private void HandleTickChange(TickUpdate update)
    {
        var tickState = this.state.ChangeTick(update);

        Debug.Assert(tickState.Tick == update.Current, this);

        this.spawner.ChangeTick(update.Current);
    }

    private void HandleTickUpdate(TickUpdate update)
    {
        this.state.UpdateTick(update);

        var tickState = this.state.GetTickState(update.Current);
        var nextTickState = this.state.GetTickStateOrAdd(update.Current + 1);

        this.view.UpdateTick(update, tickState, nextTickState);
    }
}
