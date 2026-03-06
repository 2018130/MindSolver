using UnityEngine;

public class TutorialChecker: MonoBehaviour
{
    public void OnClickStartGame() // 게임시작버튼연결
    {
        //bool값으로 체크해서 튜토리얼 본지 안본지 확인
        bool hasPlayedTutorial = PlayerPrefs.GetInt("TutorialClear", 0) == 1;

        if (hasPlayedTutorial)
        {
            Debug.Log("튜토리얼 건너뛰기");
            SceneChangeManager.Singleton.ChangeScene(SceneType.NetworkRelayScene);
        }
        else
        {
            Debug.Log("튜토리얼로 이동");
            SceneChangeManager.Singleton.ChangeScene(SceneType.TutorialScene);
        }
    }

    public void OnTutorialClear()
    {
        // 튜토리얼 완료 표시
        PlayerPrefs.SetInt("TutorialClear", 1);
        PlayerPrefs.Save(); 

        // 게임 씬으로 이동
        SceneChangeManager.Singleton.ChangeScene(SceneType.NetworkRelayScene);
    }
}