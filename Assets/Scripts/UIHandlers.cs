using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIHandlers : MonoBehaviour
{
    [SerializeField]
    private Text turnText;

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

    private void HandleTick(int tick)
    {
        this.turnText.text = string.Format("Turn {0}", tick);
    }

    public void UGUI_OnResetButtonPress()
    {
        SceneManager.LoadScene(0);
    }
}
