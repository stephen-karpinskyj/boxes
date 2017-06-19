using UnityEngine;
using UnityEngine.SceneManagement;

public class UIHandlers : MonoBehaviour
{
    public void UGUI_OnResetButtonPress()
    {
        SceneManager.LoadScene(0);
    }
}
