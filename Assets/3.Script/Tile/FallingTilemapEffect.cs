using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

// -----------------------------------------------------------------------------
// [읽기 전용 커스텀 속성]
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
    [ReadOnly][SerializeField] private bool isAnimating = false;
    [ReadOnly][SerializeField] private bool hasCapturedState = false;

    // 초기 상태 캐싱 데이터
    private Tilemap[] childTilemaps;
    private Vector3[] initialLayerPositions;
    private float[] initialLayerAlphas;
    private List<IndividualTileData> individualTiles = new List<IndividualTileData>();

    // 애니메이션 시간 제어용
    private float animationStartTime;
    private Coroutine activeCoroutine; // 런타임 애니메이션 실행을 위한 코루틴 캐싱


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
    public void StartLayerFall()
    {
        if (isAnimating) return;

        CaptureInitialState();
        if (childTilemaps == null || childTilemaps.Length == 0) return;

        animationStartTime = Time.time;
        isAnimating = true;

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(LayerAnimationRoutine());
    }

    // 기존 주석 처리되어 있던 코드를 런타임 코루틴으로 실행하도록 연결
    private IEnumerator LayerAnimationRoutine()
    {
        while (isAnimating)
        {
            UpdateLayerAnimation();
            yield return null;
        }
    }

    private void UpdateLayerAnimation()
    {
        float elapsed = Time.time - animationStartTime; // EditorApplication.timeSinceStartup 대체
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

        if (distance >= disappearDistance) StopAnimation();
    }

    // =================================================================
    // 2. 모든 타일 개별 낙하 연출
    // =================================================================
    public void StartIndividualFall()
    {
        if (isAnimating) return;

        CaptureInitialState();
        if (childTilemaps == null || childTilemaps.Length == 0) return;

        AssignStaggeredDelays();

        animationStartTime = Time.time; // 빌드 호환을 위해 Time.time 사용
        isAnimating = true;

        // 리플렉션 오류 방지 (빌드 시 FallingEffect를 안전하게 호출)
        foreach (var fallingEffect in GetComponentsInChildren<MonoBehaviour>())
        {
            if (fallingEffect.GetType().Name == "FallingEffect")
            {
                fallingEffect.SendMessage("StartFalling", SendMessageOptions.DontRequireReceiver);
            }
        }

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(IndividualAnimationRoutine());
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

    // 에디터 밖에서도 실행되도록 Update 로직을 코루틴에 연결
    private IEnumerator IndividualAnimationRoutine()
    {
        while (isAnimating)
        {
            UpdateIndividualAnimation();
            yield return null;
        }
    }

    // #if UNITY_EDITOR 삭제: 빌드 환경에서도 정상 호출 가능하도록 변경
    private void UpdateIndividualAnimation()
    {
        float currentTime = Time.time - animationStartTime; // EditorApplication.timeSinceStartup 대체
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

        if (!isAnyTileStillFalling) StopAnimation();
    }

    // =================================================================
    // 3. 복구 로직
    // =================================================================
    public void ResetEffect()
    {
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
        StopAnimation();
        Debug.Log("원상복구 완료.");
    }

    public void HardReset()
    {
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
        StopAnimation();
        Debug.Log("비상 복구 완료.");
    }

    private void StopAnimation()
    {
        isAnimating = false;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
    }

    // =================================================================
    // [개발자를 위한 가이드] 
    // 나중에 실제 게임의 스테이지 클리어 연출로 전환하는 방법
    // =================================================================
    public void PlayStageClearTransition()
    {
        CaptureInitialState();
        AssignStaggeredDelays();

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(StageClearCoroutine());
    }

    private IEnumerator StageClearCoroutine()
    {
        float elapsedTime = 0f;
        bool isAnyTileStillFalling = true;
        isAnimating = true;

        while (isAnyTileStillFalling)
        {
            elapsedTime += Time.deltaTime; // 인게임 프레임 시간에 맞춰 증가
            isAnyTileStillFalling = false;

            foreach (var tile in individualTiles)
            {
                if (elapsedTime < tile.delay)
                {
                    isAnyTileStillFalling = true;
                    continue;
                }

                float fallTime = elapsedTime - tile.delay;
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

            yield return null; // 다음 프레임까지 대기
        }

        StopAnimation();
        // 연출 종료 후 잠시 여운을 줌
        yield return new WaitForSeconds(0.5f);
    }
}