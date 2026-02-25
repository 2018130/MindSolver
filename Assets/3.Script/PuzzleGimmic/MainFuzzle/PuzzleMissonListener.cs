using System;
using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Events;

public enum MissionType
{
    Single,
    Multi,
}

public enum MultiMissionType
{
    ThreadDrawer,
    DrawLine,
    Sandwich,
    Pipe,
}

public class PuzzleMissonListener : MonoBehaviour
{
    [SerializeField]
    private MissionType missionType = MissionType.Single;
    public MissionType MissionType => missionType;

    [SerializeField]
    private MultiMissionType multiMissionType = MultiMissionType.ThreadDrawer;
    public MultiMissionType MultiMissionType => multiMissionType;

    private PuzzleMissionTrigger sender;

    public event Action<MultiMissionType> OnStartPuzzle;
    public event Action<bool, MultiMissionType> OnEndPuzzle;

    [SerializeField]
    private float endDelayTime = 3f;

    public void StartPuzzle(PuzzleMissionTrigger sender)
    {
        this.sender = sender;

        if(missionType == MissionType.Multi)
        {
            OnStartPuzzle?.Invoke(multiMissionType);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void EndPuzzle(bool isClear)
    {
        if (missionType == MissionType.Multi)
        {
            StartCoroutine(EndPuzzle_co(isClear));

        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator EndPuzzle_co(bool isClear)
    {
        if (isClear)
        {
            GameUIManager.Singleton.SetText("미션 성공!!!!");
        }
        else
        {
            GameUIManager.Singleton.SetText("미션 실패ㅠㅜ");
        }

        yield return new WaitForSeconds(endDelayTime);

        GameUIManager.Singleton.SetText("");
        if (sender != null)
        {
            OnEndPuzzle?.Invoke(isClear, multiMissionType);
        }
    }
}
