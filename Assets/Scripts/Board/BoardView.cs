using System;
using UnityEngine;

[Serializable]
public class BoardView
{
    [SerializeField]
    private DieView dieViewPrefab;

    [SerializeField]
    private int initialPoolSize = 80;

    private Pool<DieView> dieViewPool = new Pool<DieView>();

    private Transform root;

    public void Initialize(Transform root)
    {
        this.root = root;

        this.dieViewPool.UnuseAll(dieView => dieView.Hide());

        while (this.dieViewPool.AvailableCount < this.initialPoolSize)
        {
            var dieView = this.CreateNewDieView();
            this.dieViewPool.InsertAvailable(dieView);
        }
    }

    public bool Contains(int dieId)
    {
        return this.dieViewPool.FindUsed(v => v.Id == dieId) != null;
    }

    public void UpdateTick(TickUpdate update, TickState tickState, TickState nextTickState)
    {
        foreach (var dieState in tickState.DieStates)
        {
            var dieView = this.dieViewPool.FindUsed(v => v.Id == dieState.Id);

            if (dieView == null)
            {
                dieView = this.AddDieView(dieState.Id);
            }

            Debug.Assert(dieView != null);

            DieState nextDieState = null;

            if (nextTickState.Contains(dieState.Id))
            {
                nextDieState = nextTickState.GetDieState(dieState.Id);
            }

            dieView.UpdateTick(update, dieState, nextDieState);
        }

        var removedViews = this.dieViewPool.FindAllUsed(v => !tickState.Contains(v.Id));

        foreach (var dieView in removedViews)
        {
            this.RemoveDieView(dieView);
        }
    }

    private DieView AddDieView(int dieId)
    {
        Debug.Assert(!this.Contains(dieId));

        var dieView = this.dieViewPool.Use(() => { return this.CreateNewDieView(); });
        dieView.Show(dieId);

        Debug.Assert(this.Contains(dieId));

        return dieView;
    }

    private void RemoveDieView(DieView dieView)
    {
        Debug.Assert(this.Contains(dieView.Id));

        dieView.Hide();
        this.dieViewPool.Unuse(dieView);

        Debug.Assert(!this.Contains(dieView.Id));
    }

    private DieView CreateNewDieView()
    {
        var newDieView = GameObjectUtility.InstantiatePrefab(this.dieViewPrefab, this.root);
        newDieView.Hide();

        Debug.Assert(newDieView != null);

        return newDieView;
    }
}
