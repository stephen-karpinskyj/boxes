using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : BehaviourSingleton<GameManager>
{
    private int prevTick;
    private int tick;

    public delegate void OnGameStartDelegate();
    public OnGameStartDelegate OnGameStart = delegate { };

    public delegate void OnTickChangeDelegate(int prevTick, int tick);
    public OnTickChangeDelegate OnTickChange = delegate { };

    public delegate void OnTickUpdateDelegate(int prevTick, int tick, float t);
    public OnTickUpdateDelegate OnTickUpdate = delegate { };

    public bool HasStarted { get; private set; }

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
        this.prevTick = this.tick;
        this.tick++;

        this.OnTickChange(this.prevTick, this.tick);
    }

    public void UpdateTick(float t)
    {
        this.OnTickUpdate(this.prevTick, this.tick, t);
    }

    private void Reset()
    {
        this.prevTick = -1;
        this.tick = 0;
        
        this.OnTickChange(this.prevTick, this.tick);
        this.OnTickUpdate(this.prevTick, this.tick, 0f);

        Debug.Log("[Game] Reset", this);

        this.HasStarted = true;
        this.OnGameStart();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.Reset();
    }
}
