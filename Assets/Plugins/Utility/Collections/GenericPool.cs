using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class GenericPool<T>
{
    private List<T> available;
    private List<T> used;

    public delegate T OnCreateCallback();
    public delegate void OnUnuseCallback(T obj);

    public int AvailableCount
    {
        get { return this.available.Count; }
    }

    public IEnumerable<T> Available
    {
        get { return this.available; }
    }

    public int UsedCount
    {
        get { return this.used.Count; }
    }

    public IEnumerable<T> Used
    {
        get { return this.used; }
    }

    public GenericPool()
    {
        this.available = new List<T>();
        this.used = new List<T>();
    }

    public GenericPool(IEnumerable<T> elements)
    {
        Debug.Assert(this.available == null);

        this.available = new List<T>(elements);
        this.used = new List<T>();
    }

    public T FindAvailable(Predicate<T> match)
    {
        return this.available.Find(match);
    }

    public List<T> FindAllAvailable(Predicate<T> match)
    {
        return this.available.FindAll(match);
    }

    public T FindUsed(Predicate<T> match)
    {
        return this.used.Find(match);
    }

    public List<T> FindAllUsed(Predicate<T> match)
    {
        return this.used.FindAll(match);
    }

    public void InsertAvailable(T obj)
    {
        Debug.Assert(!this.available.Contains(obj));
        Debug.Assert(!this.used.Contains(obj));

        this.available.Add(obj);
    }

    public void Use(T obj)
    {
        Debug.Assert(!this.used.Contains(obj));

        this.available.Remove(obj);
        this.used.Add(obj);
    }

    public T Use(OnCreateCallback onCreate = null)
    {
        if (this.AvailableCount <= 0)
        {
            Debug.Assert(onCreate != null);

            this.available.Add(onCreate());
        }

        var toUse = this.available[0];

        this.Use(toUse);

        return toUse;
    }

    public bool TryUseRandom(out T obj)
    {
        obj = default(T);

        if (this.AvailableCount <= 0)
        {
            return false;
        }

        obj = this.available[Random.Range(0, this.AvailableCount)];

        this.Use(obj);

        return true;
    }

    public void Unuse(T obj)
    {
        Debug.Assert(this.used.Contains(obj));
        Debug.Assert(!this.available.Contains(obj));

        this.used.Remove(obj);
        this.available.Add(obj);
    }

    public void UnuseAll(OnUnuseCallback onUnuse = null)
    {
        foreach (var obj in this.used)
        {
            Debug.Assert(!this.available.Contains(obj));

            this.available.Add(obj);

            if (onUnuse != null)
            {
                onUnuse(obj);
            }
        }

        this.used.Clear();
    }
}
