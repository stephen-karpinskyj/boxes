using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : BehaviourSingleton<GameManager>
{
    private int tick;
    
    public delegate void OnTickDelegate(int tick);
    public OnTickDelegate OnTick = delegate { };

    private void OnEnable()
    {
        SceneManager.sceneLoaded += this.HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= this.HandleSceneLoaded;
    }

    public void EndMove(Die die)
    {
        this.tick++;

        this.OnTick(this.tick);
    }

    private void Start()
    {
        this.Reset();
    }

    private void Reset()
    {
        this.tick = 0;
        
        this.OnTick(this.tick);

        Debug.Log("[Game] Reset", this);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.StartCoroutine(this.HandleSceneLoadedCoroutine());
    }

    private IEnumerator HandleSceneLoadedCoroutine()
    {
        yield return null;
        
        this.Reset();
    }
}
