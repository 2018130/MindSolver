using UnityEngine;

public class EndingSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject developMsg;
    private int count = 0;

    public void ToggleEndingMsg()
    {
        count++;

        if(count > 10)
        {
            developMsg.SetActive(!developMsg.activeSelf);
        }
    }

    public void GoToLobby()
    {
        SceneChangeManager.Singleton.ChangeScene(SceneType.LobbyScene);
    }
}
