using System;
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
            if (sender != null)
            {
                OnEndPuzzle?.Invoke(isClear, multiMissionType);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
