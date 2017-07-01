using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIHandlers : MonoBehaviour
{
    [SerializeField]
    private Text oddTurnText;

    [SerializeField]
    private Text evenTurnText;

    [SerializeField]
    private float turnTextTweenDist = 70f;

    private void OnEnable()
    {
        GameManager.Instance.OnTickUpdate += this.HandleTickUpdate;
    }

    private void OnDisable()
    {
        if (GameManager.Exists)
        {
            GameManager.Instance.OnTickUpdate -= this.HandleTickUpdate;
        }
    }

    private void HandleTickUpdate(int prevTick, int tick, float t)
    {
        Vector2 pos;
        Color col;

        if (prevTick > tick)
        {
            // TODO
        }
        else
        {
            var text1 = tick % 2 == 0 ? this.evenTurnText : this.oddTurnText;
            var text2 = tick % 2 != 0 ? this.evenTurnText : this.oddTurnText;

            text1.text = (tick + 1).ToString();
            pos = text1.rectTransform.anchoredPosition;
            pos.x = Mathf.Lerp(-this.turnTextTweenDist, 0f, t);
            text1.rectTransform.anchoredPosition = pos;
            col = text1.color;
            col.a = t;
            text1.color = col;

            text2.text = (tick + 0).ToString();
            pos = text2.rectTransform.anchoredPosition;
            pos.x = Mathf.Lerp(0f, this.turnTextTweenDist, t);
            text2.rectTransform.anchoredPosition = pos;
            col = text2.color;
            col.a = 1f - t;
            text2.color = col;
        }
    }

    public void UGUI_OnResetButtonPress()
    {
        SceneManager.LoadScene(0);
    }
}
