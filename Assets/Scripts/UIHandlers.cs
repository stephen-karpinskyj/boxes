using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UIHandlers : MonoBehaviour
{
    [SerializeField]
    private Text oddTurnText;

    [SerializeField]
    private Text evenTurnText;

    [SerializeField]
    private float turnTextTweenDist = 70f;

    [SerializeField]
    private Text scoreText;

    private void OnEnable()
    {
        GameManager.Instance.OnTickUpdate += this.HandleTickUpdate;
        GameManager.Instance.OnScoreUpdate += this.HandleScoreUpdate;
    }

    private void OnDisable()
    {
        if (GameManager.Exists)
        {
            GameManager.Instance.OnTickUpdate -= this.HandleTickUpdate;
            GameManager.Instance.OnScoreUpdate -= this.HandleScoreUpdate;
        }
    }

    private void HandleTickUpdate(TickUpdate update)
    {
        Vector2 pos;
        Color col;

        if (update.Previous > update.Current)
        {
            // TODO: Visualise tick reversal
        }
        else
        {
            var text1 = update.Current % 2 == 0 ? this.evenTurnText : this.oddTurnText;
            var text2 = update.Current % 2 != 0 ? this.evenTurnText : this.oddTurnText;

            text1.text = (update.Current + 1).ToString();
            pos = text1.rectTransform.anchoredPosition;
            pos.x = Mathf.Lerp(-this.turnTextTweenDist, 0f, update.Progress);
            text1.rectTransform.anchoredPosition = pos;
            col = text1.color;
            col.a = update.Progress;
            text1.color = col;

            text2.text = (update.Current + 0).ToString();
            pos = text2.rectTransform.anchoredPosition;
            pos.x = Mathf.Lerp(0f, this.turnTextTweenDist, update.Progress);
            text2.rectTransform.anchoredPosition = pos;
            col = text2.color;
            col.a = 1f - update.Progress;
            text2.color = col;
        }
    }

    private void HandleScoreUpdate(int score)
    {
        this.scoreText.text = score.ToString("D3");

        this.scoreText.transform.DOPunchScale(Vector3.one, 0.1f);
    }

    public void UGUI_OnResetButtonPress()
    {
        SceneManager.LoadScene(0);
    }
}
