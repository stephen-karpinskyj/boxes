using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : BehaviourSingleton<GameManager>
{
    private int score;
    private int prevTick;
    public int Tick { get; private set; }

    public delegate void OnGameStartDelegate();
    public OnGameStartDelegate OnGameStart = delegate { };

    public delegate void OnTickChangeDelegate(TickUpdate update);
    public OnTickChangeDelegate OnTickChange = delegate { };

    public delegate void OnTickUpdateDelegate(TickUpdate update);
    public OnTickUpdateDelegate OnTickUpdate = delegate { };

    public delegate void OnScoreUpdateDelegate(int score);
    public OnScoreUpdateDelegate OnScoreUpdate = delegate { };

    public bool HasStarted { get; private set; }

    private TickUpdate tickUpdate;

    private void Awake()
    {
        Input.multiTouchEnabled = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += this.HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= this.HandleSceneLoaded;
    }

    public void EndTick()
    {
        this.prevTick = this.Tick;
        this.Tick++;

        Debug.Log("[Game] Ending tick=" + this.prevTick + ", now tick=" + this.Tick);

        this.tickUpdate.Previous = this.prevTick;
        this.tickUpdate.Current = this.Tick;
        this.tickUpdate.Progress = 0f;

        this.OnTickChange(this.tickUpdate);
    }

    public void UpdateTick(float t)
    {
        this.tickUpdate.Previous = this.prevTick;
        this.tickUpdate.Current = this.Tick;
        this.tickUpdate.Progress = t;

        this.OnTickUpdate(this.tickUpdate);
    }

    public void AddScore(int score)
    {
        this.score += score;

        Debug.Log("[Game] Added score=" + score, this);

        this.OnScoreUpdate(this.score);
    }

    private void Reset()
    {
        this.score = 0;
        this.Tick = 0;

        Debug.Log("[Game] Reset", this);

        this.HasStarted = true;
        this.OnGameStart();

        this.EndTick();
        this.UpdateTick(0f);

        this.OnScoreUpdate(0);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.Reset();
    }
}
