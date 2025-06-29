using UnityEngine;

public class StartGame : MonoBehaviour
{
    public void LoadGameScene(string scene)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.ProcessToNextScene(scene);
    }
}
