using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private readonly List<IEnumerator> running = new List<IEnumerator>();

    public bool IsRunning
    {
        get { return this.running.Count > 0; }
    }

    public void Reset()
    {
        this.running.Clear();
    }

    public void Run(IEnumerator coroutine)
    {
        this.StartCoroutine(this.RunCoroutine(coroutine));
    }

    private IEnumerator RunCoroutine(IEnumerator coroutine)
    {
        Debug.Assert(!this.running.Contains(coroutine), this);

        this.running.Add(coroutine);

        yield return this.StartCoroutine(coroutine);

        this.running.Remove(coroutine);
    }
}
