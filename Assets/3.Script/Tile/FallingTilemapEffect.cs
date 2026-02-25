using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// -----------------------------------------------------------------------------
// [읽기 전용 커스텀 속성]
// 인스펙터 창에서 변수의 상태를 눈으로 확인할 수만 있게 만듭니다.
// -----------------------------------------------------------------------------
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
#endif

// -----------------------------------------------------------------------------
// [데이터 구조 및 열거형]
// -----------------------------------------------------------------------------
public enum TileDropPattern
{
    RandomGroup,    // 무작위 그룹이 일정한 간격으로 연속 낙하
    LeftToRight     // 좌측에서 우측으로 물결치듯 연속 낙하
}

[System.Serializable]
public class IndividualTileData
{
    public Tilemap tilemap;
    public Vector3Int position;
    public float delay;
    public Matrix4x4 initialMatrix;
    public Color initialColor;
}

// -----------------------------------------------------------------------------
// [메인 연출 스크립트]
// -----------------------------------------------------------------------------
[ExecuteAlways]
public class FallingTilemapEffect : MonoBehaviour
{
    [Header("공통 연출 설정")]
    public float fallSpeed = 10.0f;
    public float disappearDistance = 5.0f;

    [Header("개별 낙하 템포 설정")]
    public TileDropPattern dropPattern = TileDropPattern.RandomGroup;
    [Tooltip("다음 타일 그룹이 떨어지기 시작할 때까지의 간격(초)입니다. 값이 작을수록 더 촘촘하게 떨어집니다.")]
    public float dropInterval = 0.05f;

    [Header("상태 확인 (수정 불필요)")]
    [ReadOnly] [SerializeField] private bool isAnimating = false;
    [ReadOnly] [SerializeField] private bool hasCapturedState = false;

    // 초기 상태 캐싱 데이터
    private Tilemap[] childTilemaps;
    private Vector3[] initialLayerPositions;
    private float[] initialLayerAlphas;
    private List<IndividualTileData> individualTiles = new List<IndividualTileData>();

    // 애니메이션 시간 제어용
    private float animationStartTime;

#if UNITY_EDITOR

    // =================================================================
    // 상태 수집 로직
    // =================================================================
    private void CaptureInitialState()
    {
        if (hasCapturedState) return;

        childTilemaps = GetComponentsInChildren<Tilemap>();
        if (childTilemaps == null || childTilemaps.Length == 0) return;

        initialLayerPositions = new Vector3[childTilemaps.Length];
        initialLayerAlphas = new float[childTilemaps.Length];
        individualTiles.Clear();

        for (int i = 0; i < childTilemaps.Length; i++)
        {
            Tilemap map = childTilemaps[i];
            initialLayerPositions[i] = map.transform.localPosition;
            initialLayerAlphas[i] = map.color.a;

            BoundsInt bounds = map.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (map.HasTile(pos))
                {
                    IndividualTileData data = new IndividualTileData();
                    data.tilemap = map;
                    data.position = pos;
                    data.initialMatrix = map.GetTransformMatrix(pos);
                    data.initialColor = map.GetColor(pos);

                    map.SetTileFlags(pos, TileFlags.None);
                    individualTiles.Add(data);
                }
            }
        }
        hasCapturedState = true;
    }

    // =================================================================
    // 1. 레이어 전체 하강 연출
    // =================================================================
    [ContextMenu("1. 모든 레이어 전체 하강 시작")]
    public void StartLayerFall()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("이 기능은 에디터 테스트용입니다. 인게임 적용법은 스크립트 하단 주석을 참고하세요.");
            return;
        }
        if (isAnimating) return;

        CaptureInitialState();
        if (childTilemaps == null || childTilemaps.Length == 0) return;

        animationStartTime = (float)EditorApplication.timeSinceStartup;
        isAnimating = true;
        EditorApplication.update += UpdateLayerAnimation;
    }

    private void UpdateLayerAnimation()
    {
        float elapsed = (float)EditorApplication.timeSinceStartup - animationStartTime;
        float distance = elapsed * fallSpeed;
        float alphaRatio = Mathf.Clamp01(1.0f - (distance / disappearDistance));

        for (int i = 0; i < childTilemaps.Length; i++)
        {
            if (childTilemaps[i] == null) continue;
            childTilemaps[i].transform.localPosition = initialLayerPositions[i] + new Vector3(0, -distance, 0);

            Color c = childTilemaps[i].color;
            c.a = initialLayerAlphas[i] * alphaRatio;
            childTilemaps[i].color = c;
        }

        if (distance >= disappearDistance) StopAnimation(UpdateLayerAnimation);
    }

    // =================================================================
    // 2. 모든 타일 개별 낙하 연출
    // =================================================================
    [ContextMenu("2. 모든 타일 개별 낙하 시작")]
    public void StartIndividualFall()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("이 기능은 에디터 테스트용입니다. 인게임 적용법은 스크립트 하단 주석을 참고하세요.");
            return;
        }
        if (isAnimating) return;

        CaptureInitialState();
        if (childTilemaps == null || childTilemaps.Length == 0) return;

        AssignStaggeredDelays();

        animationStartTime = (float)EditorApplication.timeSinceStartup;
        isAnimating = true;
        EditorApplication.update += UpdateIndividualAnimation;
    }

    private void AssignStaggeredDelays()
    {
        if (individualTiles.Count == 0) return;

        if (dropPattern == TileDropPattern.RandomGroup)
        {
            for (int i = 0; i < individualTiles.Count; i++)
            {
                IndividualTileData temp = individualTiles[i];
                int randomIndex = Random.Range(i, individualTiles.Count);
                individualTiles[i] = individualTiles[randomIndex];
                individualTiles[randomIndex] = temp;
            }

            int currentIndex = 0;
            int groupCount = 0;

            while (currentIndex < individualTiles.Count)
            {
                int groupSize = Random.Range(1, 11);
                float currentGroupDelay = groupCount * dropInterval;

                for (int j = 0; j < groupSize && currentIndex < individualTiles.Count; j++)
                {
                    individualTiles[currentIndex].delay = currentGroupDelay;
                    currentIndex++;
                }
                groupCount++;
            }
        }
        else if (dropPattern == TileDropPattern.LeftToRight)
        {
            int minX = int.MaxValue;
            foreach (var tile in individualTiles) if (tile.position.x < minX) minX = tile.position.x;
            foreach (var tile in individualTiles) tile.delay = (tile.position.x - minX) * dropInterval;
        }
    }

    private void UpdateIndividualAnimation()
    {
        float currentTime = (float)EditorApplication.timeSinceStartup - animationStartTime;
        bool isAnyTileStillFalling = false;

        foreach (var tile in individualTiles)
        {
            if (currentTime < tile.delay)
            {
                isAnyTileStillFalling = true;
                continue;
            }

            float fallTime = currentTime - tile.delay;
            float distance = fallTime * fallSpeed;

            if (distance < disappearDistance)
            {
                isAnyTileStillFalling = true;

                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(0, -distance, 0), Quaternion.identity, Vector3.one);
                tile.tilemap.SetTransformMatrix(tile.position, matrix);

                float alphaRatio = Mathf.Clamp01(1.0f - (distance / disappearDistance));
                Color newColor = tile.initialColor;
                newColor.a = tile.initialColor.a * alphaRatio;
                tile.tilemap.SetColor(tile.position, newColor);
            }
            else
            {
                Color hiddenColor = tile.initialColor;
                hiddenColor.a = 0f;
                tile.tilemap.SetColor(tile.position, hiddenColor);
            }
        }

        if (!isAnyTileStillFalling) StopAnimation(UpdateIndividualAnimation);
    }

    // =================================================================
    // 3. 복구 로직
    // =================================================================
    [ContextMenu("3. 원상복구 (모두 리셋)")]
    public void ResetEffect()
    {
        StopAllAnimations();

        if (childTilemaps == null || initialLayerPositions == null || childTilemaps.Length != initialLayerPositions.Length)
        {
            Debug.LogWarning("타일맵 구조가 변경되어 강제 초기화를 진행합니다.");
            HardReset();
            return;
        }

        for (int i = 0; i < childTilemaps.Length; i++)
        {
            if (childTilemaps[i] == null) continue;
            childTilemaps[i].transform.localPosition = initialLayerPositions[i];

            Color c = childTilemaps[i].color;
            c.a = initialLayerAlphas[i];
            childTilemaps[i].color = c;
        }

        foreach (var tile in individualTiles)
        {
            if (tile.tilemap == null) continue;
            tile.tilemap.SetTransformMatrix(tile.position, tile.initialMatrix);
            tile.tilemap.SetColor(tile.position, tile.initialColor);
        }

        hasCapturedState = false;
        Debug.Log("원상복구 완료.");
    }

    [ContextMenu("비상 복구 (모든 타일 강제 초기화)")]
    public void HardReset()
    {
        StopAllAnimations();

        Tilemap[] allMaps = GetComponentsInChildren<Tilemap>();
        foreach (Tilemap map in allMaps)
        {
            map.transform.localPosition = Vector3.zero;

            Color mapColor = map.color;
            mapColor.a = 1f;
            map.color = mapColor;

            BoundsInt bounds = map.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (map.HasTile(pos))
                {
                    map.SetTransformMatrix(pos, Matrix4x4.identity);
                    map.SetColor(pos, Color.white);
                }
            }
        }
        hasCapturedState = false;
        individualTiles.Clear();
        Debug.Log("비상 복구 완료.");
    }

    private void StopAnimation(EditorApplication.CallbackFunction method)
    {
        isAnimating = false;
        EditorApplication.update -= method;
    }

    private void StopAllAnimations()
    {
        isAnimating = false;
        EditorApplication.update -= UpdateLayerAnimation;
        EditorApplication.update -= UpdateIndividualAnimation;
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) StopAllAnimations();
    }

    // =================================================================
    // [개발자를 위한 가이드] 
    // 나중에 실제 게임의 스테이지 클리어 연출로 전환하는 방법
    // =================================================================
    /*
    이 스크립트는 에디터 테스트(EditorApplication.update) 기반으로 작성되었습니다.
    실제 게임(런타임)에서 스테이지 클리어 시 작동하게 하려면 다음 과정을 거쳐야 합니다.

    1. 네임스페이스 추가: 스크립트 맨 위에 아래 두 줄을 추가합니다.
       using System.Collections;
       using UnityEngine.SceneManagement;

    2. 런타임용 코루틴 작성:
       에디터용 업데이트 함수 대신, 코루틴(IEnumerator)을 만들어 Time.deltaTime을 활용합니다.

       public void PlayStageClearTransition(string nextSceneName)
       {
           CaptureInitialState();
           AssignStaggeredDelays();
           StartCoroutine(StageClearCoroutine(nextSceneName));
       }

       private IEnumerator StageClearCoroutine(string nextSceneName)
       {
           float elapsedTime = 0f;
           bool isAnyTileStillFalling = true;

           while (isAnyTileStillFalling)
           {
               elapsedTime += Time.deltaTime; // 인게임 프레임 시간에 맞춰 증가
               isAnyTileStillFalling = false;

               // ... (여기에 UpdateIndividualAnimation의 거리 및 투명도 계산 로직을 그대로 붙여넣습니다) ...
               
               yield return null; // 다음 프레임까지 대기
           }

           // 연출 종료 후 잠시 여운을 줌
           yield return new WaitForSeconds(0.5f); 
           
           // 다음 씬 로드
           SceneManager.LoadScene(nextSceneName); 
       }

    3. 실행: 
       게임오버 매니저나 스테이지 클리어 매니저 스크립트에서 보스를 잡았을 때 
       이 스크립트의 PlayStageClearTransition("다음 씬 이름") 함수를 호출하면 작동합니다.
    */
#endif
}