using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TutorialSceneManager : MonoBehaviour
{
    public static TutorialSceneManager singleton;

    [Header("Tile")]

    [SerializeField]
    private List<FallingTilemapEffect> tiles = new List<FallingTilemapEffect>();

    [Header("Pathfinding")]

    [SerializeField]
    private PathFinding redPathFinding;
    [SerializeField]
    private PathFinding bluePathFinding;
    [ReadOnly][SerializeField]
    private int pathFidingCount = 0;
    [SerializeField]
    private List<Pair<Transform>> redPathfindingPoint = new List<Pair<Transform>>();
    [SerializeField]
    private List<Pair<Transform>> bluePathfindingPoint = new List<Pair<Transform>>();
    //
    [Header("Mission")]

    public bool isPlayMission { get; set; }

    [Header("MissionTrigger")]

    [SerializeField]
    private List<PuzzleMissionTrigger> missionTriggers = new List<PuzzleMissionTrigger>();
    [SerializeField]
    private List<PuzzleMissionTrigger> missionTriggers_step03 = new List<PuzzleMissionTrigger>();
    [SerializeField]
    private List<PuzzleMissionTrigger> missionTriggers_step04 = new List<PuzzleMissionTrigger>();

    [Header("ETC")]
    [ReadOnly] public int PathCount;
    [SerializeField]
    private List<GameObject> footholds = new List<GameObject>();
    [SerializeField]
    private PlayerController redPlayer;
    [SerializeField]
    private PlayerController bluePlayer;
    [SerializeField]
    private DialogueManager dialogueManager;
    [SerializeField]
    private Light2D light2d;
    [SerializeField]
    private float bloomSpeed = 3f;
    [SerializeField]
    private float maxStep05Scale = 0.5f;

    private Vector3 step05DefaultPos;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
        step05DefaultPos = tiles[4].transform.position;
    }

    private void Start()
    {
        StartCoroutine(Step01_co());
    }

    private IEnumerator Step01_co()
    {
        Debug.Log($"step 01 ½ÃÀÛ");
        dialogueManager.PrintDialogue();

        yield return new WaitUntil(() => dialogueManager.IsDialogueEnded);

        pathFidingCount = 0;
        GameUIManager.Singleton.SetPathFindingBtn(true);

        yield return new WaitWhile(() => pathFidingCount != 2);

        // Ä³¸¯ÅÍ Èå·ÁÁü
        float  timer = 0;
        float bloomTime = 3;
        float t = 1 / 4;
        Vector3 destPos = t * Vector3.zero + (1 - t) * step05DefaultPos;
        Vector3 startPos = tiles[4].transform.position;
        Vector3 destScale = new Vector3(maxStep05Scale / 4, maxStep05Scale / 4, 1);
        Vector3 startScale = new Vector3(tiles[4].transform.localScale.x, tiles[4].transform.localScale.y, 1);

        while (timer <= bloomTime)
        {
            timer += Time.deltaTime;

            yield return null;

            redPlayer.SetAlpha(Mathf.Lerp(1, 0, timer / bloomTime));
            bluePlayer.SetAlpha(Mathf.Lerp(1, 0, timer / bloomTime));

            tiles[4].transform.position = Vector3.Lerp(startPos, destPos, timer / bloomTime);
            tiles[4].transform.localScale = Vector3.Lerp(startScale, destScale, timer / bloomTime);
        }
        footholds[0].SetActive(false);
        tiles[0].StartIndividualFall();

        yield return new WaitForSeconds(2f);

        tiles[0].gameObject.SetActive(false);

        StartCoroutine(Step02_co());
    }

    private IEnumerator Step02_co()
    {
        Debug.Log($"step 02 ½ÃÀÛ");
        tiles[1].gameObject.SetActive(true);

        redPlayer.transform.position = redPathfindingPoint[0].first.position;
        bluePlayer.transform.position = bluePathfindingPoint[0].first.position;

        // Ä³¸¯ÅÍ ¹à¾ÆÁü, step 05°¡±î¿öÁü
        float timer = 0;
        float bloomTime = 3;

        while (timer <= bloomTime)
        {
            timer += Time.deltaTime;

            yield return null;

            redPlayer.SetAlpha(Mathf.Lerp(0, 1, timer / bloomTime));
            bluePlayer.SetAlpha(Mathf.Lerp(0, 1, timer / bloomTime));
        }

        dialogueManager.PrintDialogue();


        redPathFinding.Origin = redPathfindingPoint[0].first;
        redPathFinding.Destination = redPathfindingPoint[0].second;
        bluePathFinding.Origin = bluePathfindingPoint[0].first;
        bluePathFinding.Destination = bluePathfindingPoint[0].second;

        yield return new WaitUntil(() => dialogueManager.IsDialogueEnded);

        // ¹Ì¼Ç½ÃÀÛ
        isPlayMission = true;
        missionTriggers[0].SetTouchable(true);

        yield return new WaitWhile(() => isPlayMission);

        missionTriggers[1].gameObject.SetActive(false);

        // ±æ Ã£±â
        pathFidingCount = 0;
        GameUIManager.Singleton.SetPathFindingBtn(true);

        yield return new WaitWhile(() => pathFidingCount != 2);

        // Ä³¸¯ÅÍ Èå·ÁÁü
        timer = 0;
        bloomTime = 3;
        float t = 1 / 2;
        Vector3 destPos = t * Vector3.zero + (1 - t) * step05DefaultPos;
        Vector3 startPos = tiles[4].transform.position;
        Vector3 destScale = new Vector3(maxStep05Scale / 2, maxStep05Scale / 2, 1);
        Vector3 startScale = new Vector3(tiles[4].transform.localScale.x, tiles[4].transform.localScale.y, 1);

        while (timer <= bloomTime)
        {
            timer += Time.deltaTime;

            yield return null;

            redPlayer.SetAlpha(Mathf.Lerp(1, 0, timer / bloomTime));
            bluePlayer.SetAlpha(Mathf.Lerp(1, 0, timer / bloomTime));

            tiles[4].transform.position = Vector3.Lerp(startPos, destPos, timer / bloomTime);
            tiles[4].transform.localScale = Vector3.Lerp(startScale, destScale, timer / bloomTime);
        }
        // ¸Ê Á¦°Å
        footholds[1].SetActive(false);
        tiles[1].StartIndividualFall();

        yield return new WaitForSeconds(5f);

        tiles[1].gameObject.SetActive(false);

        StartCoroutine(Step03_co());
    }
    private IEnumerator Step03_co()
    {

        Debug.Log($"step 03 ½ÃÀÛ");
        tiles[2].gameObject.SetActive(true);

        redPlayer.transform.position = redPathfindingPoint[1].first.position;
        bluePlayer.transform.position = bluePathfindingPoint[1].first.position;

        // Ä³¸¯ÅÍ ¹à¾ÆÁü
        float timer = 0;
        float bloomTime = 3;

        while (timer <= bloomTime)
        {
            timer += Time.deltaTime;

            yield return null;

            redPlayer.SetAlpha(Mathf.Lerp(0, 1, timer / bloomTime));
            bluePlayer.SetAlpha(Mathf.Lerp(0, 1, timer / bloomTime));
        }

        redPathFinding.Origin = redPathfindingPoint[1].first;
        redPathFinding.Destination = redPathfindingPoint[1].second;
        bluePathFinding.Origin = bluePathfindingPoint[1].first;
        bluePathFinding.Destination = bluePathfindingPoint[1].second;

        yield return null;
        PathCount = 0;
        while (PathCount < 2)
        {
            redPlayer.transform.position = redPathfindingPoint[1].first.position;
            bluePlayer.transform.position = bluePathfindingPoint[1].first.position;
            PathCount = 0;

            GameUIManager.Singleton.SetCanMoveText(1);

            // ¹Ì¼Ç½ÃÀÛ
            isPlayMission = true;
            for (int i = 0; i < missionTriggers_step03.Count; i++)
            {
                missionTriggers_step03[i].gameObject.SetActive(true);
                missionTriggers_step03[i].SetTouchable(true);
            }

            yield return new WaitWhile(() => isPlayMission);

            // ±æ Ã£±â
            pathFidingCount = 0;
            GameUIManager.Singleton.SetPathFindingBtn(true);

            yield return new WaitWhile(() => pathFidingCount != 2);
        }

        // Ä³¸¯ÅÍ Èå·ÁÁü
        timer = 0;
        bloomTime = 3;

        float t = 3 / 4;
        Vector3 destPos = t * Vector3.zero + (1 - t) * step05DefaultPos;
        Vector3 startPos = tiles[4].transform.position;
        Vector3 destScale = new Vector3(maxStep05Scale * 3 / 4, maxStep05Scale * 3/ 4, 1);
        Vector3 startScale = new Vector3(tiles[4].transform.localScale.x, tiles[4].transform.localScale.y, 1);

        while (timer <= bloomTime)
        {
            timer += Time.deltaTime;

            yield return null;

            redPlayer.SetAlpha(Mathf.Lerp(1, 0, timer / bloomTime));
            bluePlayer.SetAlpha(Mathf.Lerp(1, 0, timer / bloomTime));

            tiles[4].transform.position = Vector3.Lerp(startPos, destPos, timer / bloomTime);
            tiles[4].transform.localScale = Vector3.Lerp(startScale, destScale, timer / bloomTime);
        }

        foreach (var missonBox in missionTriggers_step03)
        {
            if(missonBox.gameObject.activeSelf)
                missonBox.GetComponent<FallingEffect>().StartFalling();
        }
        footholds[2].SetActive(false);
        tiles[2].StartIndividualFall();

        yield return new WaitForSeconds(5f);

        tiles[2].gameObject.SetActive(false);

        StartCoroutine(Step04_co());
    }

    private IEnumerator Step04_co()
    {

        Debug.Log($"step 04 ½ÃÀÛ");
        tiles[3].gameObject.SetActive(true);

        redPlayer.transform.position = redPathfindingPoint[2].first.position;
        bluePlayer.transform.position = bluePathfindingPoint[2].first.position;

        // Ä³¸¯ÅÍ ¹à¾ÆÁü
        float timer = 0;
        float bloomTime = 3;

        while (timer <= bloomTime)
        {
            timer += Time.deltaTime;

            yield return null;

            redPlayer.SetAlpha(Mathf.Lerp(0, 1, timer / bloomTime));
            bluePlayer.SetAlpha(Mathf.Lerp(0, 1, timer / bloomTime));
        }

        yield return null;

        //dialogueManager.PrintDialogue();


        redPathFinding.Origin = redPathfindingPoint[2].first;
        redPathFinding.Destination = redPathfindingPoint[2].second;
        bluePathFinding.Origin = bluePathfindingPoint[2].first;
        bluePathFinding.Destination = bluePathfindingPoint[2].second;

        //yield return new WaitUntil(() => dialogueManager.IsDialogueEnded);
        PathCount = 0;
        while (PathCount < 2)
        {
            PathCount = 0;
            redPlayer.transform.position = redPathfindingPoint[2].first.position;
            bluePlayer.transform.position = bluePathfindingPoint[2].first.position;

            GameUIManager.Singleton.SetCanMoveText(1);

            // ¹Ì¼Ç½ÃÀÛ
            isPlayMission = true;
            for (int i = 0; i < missionTriggers_step04.Count; i++)
            {
                missionTriggers_step04[i].SetTouchable(true);
            }

            yield return new WaitWhile(() => isPlayMission);
            GameUIManager.Singleton.SetCanMoveText(0);

            // ±æ Ã£±â
            pathFidingCount = 0;
            GameUIManager.Singleton.SetPathFindingBtn(true);

            yield return new WaitWhile(() => pathFidingCount != 2);
        }

        foreach (var missonBox in missionTriggers_step04)
        {
            if (missonBox.gameObject.activeSelf)
                missonBox.GetComponent<FallingEffect>().StartFalling();
        }

        // Ä³¸¯ÅÍ Èå·ÁÁü
        timer = 0;
        bloomTime = 3;
        float t = 3 / 4;
        Vector3 destPos = t * Vector3.zero + (1 - t) * step05DefaultPos;
        Vector3 startPos = tiles[4].transform.position;
        Vector3 destScale = new Vector3(maxStep05Scale, maxStep05Scale, 1);
        Vector3 startScale = new Vector3(tiles[4].transform.localScale.x, tiles[4].transform.localScale.y, 1);

        while (timer <= bloomTime)
        {
            timer += Time.deltaTime;

            yield return null;

            redPlayer.SetAlpha(Mathf.Lerp(1, 0, timer / bloomTime));
            bluePlayer.SetAlpha(Mathf.Lerp(1, 0, timer / bloomTime));

            tiles[4].transform.position = Vector3.Lerp(startPos, destPos, timer / bloomTime);
            tiles[4].transform.localScale = Vector3.Lerp(startScale, destScale, timer / bloomTime);
        }

        footholds[3].SetActive(false);
        tiles[3].StartIndividualFall();

        yield return new WaitForSeconds(5f);

        tiles[3].gameObject.SetActive(false);

        StartCoroutine(Step05_co());
    }

    private IEnumerator Step05_co()
    {

        GameUIManager.Singleton.SetCanMoveText(-1);
        tiles[4].gameObject.SetActive(true);

        // Ä³¸¯ÅÍ ¹à¾ÆÁü
        float timer = 0;
        float bloomTime = 3;

        Vector3 destPos = Vector3.zero;
        Vector3 startPos = tiles[4].transform.position;
        Vector3 startScale = new Vector3(tiles[4].transform.localScale.x, tiles[4].transform.localScale.y, 1);
        Vector3 destScale = Vector3.one;

        while (timer <= bloomTime)
        {
            timer += Time.deltaTime;

            yield return null;

            redPlayer.SetAlpha(Mathf.Lerp(0, 1, timer / bloomTime));
            bluePlayer.SetAlpha(Mathf.Lerp(0, 1, timer / bloomTime));

            tiles[4].transform.position = Vector3.Lerp(startPos, destPos, timer / bloomTime);
            tiles[4].transform.localScale = Vector3.Lerp(startScale, destScale, timer / bloomTime);

            redPlayer.transform.position = redPathfindingPoint[3].first.position;
            bluePlayer.transform.position = bluePathfindingPoint[3].first.position;
        }

        dialogueManager.PrintDialogue();


        redPathFinding.Origin = redPathfindingPoint[3].first;
        redPathFinding.Destination = redPathfindingPoint[3].second;
        bluePathFinding.Origin = bluePathfindingPoint[3].first;
        bluePathFinding.Destination = bluePathfindingPoint[3].second;

        yield return new WaitUntil(() => dialogueManager.IsDialogueEnded);


        // ±æ Ã£±â
        pathFidingCount = 0;
        StartPathfinding();

        yield return new WaitWhile(() => pathFidingCount != 2);


        yield return new WaitForSeconds(1f);

        timer = 0;
        bloomTime = 5;

        while(timer <= bloomTime)
        {
            timer += Time.deltaTime;

            yield return null;

            light2d.intensity += Time.deltaTime * bloomSpeed;
        }

        SceneChangeManager.Singleton.ChangeScene(SceneType.NetworkRelayScene);
    }

    public void StartPathfinding()
    {
        redPathFinding.StartPathFinding();
        bluePathFinding.StartPathFinding();
    }
    public void EndOfPathfinding(bool isClear)
    {
        pathFidingCount++;
        PathCount = isClear ? PathCount + 1 : PathCount;
    }//

    public void EndOfMission()
    {
        isPlayMission = false;
    }
}
