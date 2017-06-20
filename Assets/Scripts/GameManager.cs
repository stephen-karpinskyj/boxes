using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : BehaviourSingleton<GameManager>
{
    private const float TickDuration = 1f;
    private const int TickCount = 10;

    public delegate void OnTickDelegate();
    public OnTickDelegate OnTick = delegate { };

    private void OnEnable()
    {
        SceneManager.sceneLoaded += this.HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= this.HandleSceneLoaded;
    }

    private void Start()
    {
        this.Reset();
    }

    private void Reset()
    {
        this.StopAllCoroutines();

        //this.StartCoroutine(this.RunCoroutine());
    }

    private IEnumerator RunCoroutine()
    {
        Debug.Log("[Game] Starting", this);

        yield return new WaitForSeconds(1f);

        for (var i = 0; i < TickCount; i++)
        {
            Debug.Log("[Game] Tick=" + i, this);
            this.OnTick();

            yield return new WaitForSeconds(TickDuration);
        }

        Debug.Log("[Game] Finished", this);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.Reset();
    }
}
