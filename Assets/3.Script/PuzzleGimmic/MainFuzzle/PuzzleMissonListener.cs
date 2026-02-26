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

public enum SingleMissionType
{
    Puzzle,
    ColorPainting,
    SpreadInk,
    ObjectFinding,
    SequenceRemember,
    ConnectLine,
    Pipe,
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
    private SingleMissionType singleMissionType = SingleMissionType.Puzzle;
    public SingleMissionType SingleMissionType => singleMissionType;

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
        GameManager.Singleton.ChangeState(GameState.Puzzle);
        if (missionType == MissionType.Multi)
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
        StartCoroutine(EndPuzzle_co(isClear));
    }

    private IEnumerator EndPuzzle_co(bool isClear)
    {
        bool isNetwork = missionType == MissionType.Multi;

        if (isClear)
        {
            GameUIManager.Singleton.SetclearText("미션 성공!!!!", isNetwork);
        }
        else
        {
            GameUIManager.Singleton.SetclearText("미션 실패ㅠㅜ", isNetwork);
        }

        yield return new WaitForSeconds(endDelayTime);

        GameUIManager.Singleton.SetclearText("", isNetwork);

        if (sender != null)
        {
            if (missionType == MissionType.Multi)
            {
                OnEndPuzzle?.Invoke(isClear, multiMissionType);
            }
            else
            {
                gameObject.SetActive(false);
            }

            sender.EndPuzzle(isClear);
        }
    }
}
