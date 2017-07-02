using UnityEngine;

public class DebugTile : MonoBehaviour
{
    [SerializeField]
    private TextMesh text;

    public void SetText(string text)
    {
        this.text.text = text;
    }
}
