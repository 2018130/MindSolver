using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

// [ExecuteAlways]를 추가하면 에디터 모드에서도 스크립트의 일부 라이프사이클(예: OnValidate)이 작동합니다.
[ExecuteAlways]
public class UniversalTilemapTool : MonoBehaviour
{
    // ==========================================
    // 1. 투명도 조절 (Alpha Control)
    // ==========================================
    [Header("1. 전체 타일맵 투명도 조절")]
    [Tooltip("슬라이더를 조절하면 하위의 모든 타일맵 투명도가 즉시 변경됩니다.")]
    [Range(0f, 1f)] public float globalAlpha = 1f;

    // ==========================================
    // 2. 전체 이동 (Grid Shift)
    // ==========================================
    [Header("2. 전체 타일 이동 설정")]
    [Tooltip("이동시킬 X, Y 칸 수를 입력하고 우클릭 메뉴에서 '전체 이동'을 실행하세요.")]
    public int moveX = 0;
    public int moveY = 0;

    // ==========================================
    // 3. 마우스 좌표 추적 (Mouse Tracking)
    // ==========================================
    [Header("3. 마우스 그리드 좌표 (읽기 전용)")]
    [Tooltip("우클릭 메뉴에서 '마우스 좌표 추적 시작'을 누르면 20초 동안 씬 뷰의 마우스 위치를 그리드 좌표로 보여줍니다.")]
    public Vector3Int currentMouseGridPos;

    // 마우스 추적용 내부 변수 (에디터 전용)
#if UNITY_EDITOR
    private bool isTrackingMouse = false;
    private double trackingEndTime = 0;
#endif

    // ==========================================
    // 기능 작동 로직
    // ==========================================

    /// <summary>
    /// 인스펙터에서 값이 변경될 때마다 자동으로 호출되는 유니티 기본 함수입니다.
    /// 슬라이더를 움직일 때 실시간으로 투명도를 적용하기 위해 사용합니다.
    /// </summary>
    private void OnValidate()
    {
        UpdateTilemapAlpha();
    }

    /// <summary>
    /// 자식 오브젝트로 있는 모든 타일맵을 찾아 투명도(Alpha)를 일괄 적용합니다.
    /// </summary>
    private void UpdateTilemapAlpha()
    {
        Tilemap[] maps = GetComponentsInChildren<Tilemap>();
        foreach (var map in maps)
        {
            Color color = map.color;
            color.a = globalAlpha;
            map.color = color;
        }
    }

#if UNITY_EDITOR

    // ------------------------------------------
    // 컨텍스트 메뉴 기능 (컴포넌트 우클릭 시 나타나는 메뉴)
    // ------------------------------------------

    [ContextMenu("지정한 오프셋으로 전체 이동")]
    public void MoveAllTilemaps()
    {
        Tilemap[] maps = GetComponentsInChildren<Tilemap>();
        Vector3Int moveOffset = new Vector3Int(moveX, moveY, 0);

        if (moveOffset == Vector3Int.zero)
        {
            Debug.LogWarning("이동 값이 0입니다. Inspector에서 moveX나 moveY 값을 설정해주세요.");
            return;
        }

        foreach (var map in maps)
        {
            // 실행 취소(Ctrl+Z)를 위해 상태를 기록합니다.
            Undo.RecordObject(map, "Move Tilemap Tiles");

            BoundsInt bounds = map.cellBounds;
            TileBase[] allTiles = map.GetTilesBlock(bounds);

            // 기존 타일 지우기
            map.SetTilesBlock(bounds, new TileBase[allTiles.Length]);

            // 새로운 위치로 이동하여 타일 배치
            BoundsInt newBounds = new BoundsInt(bounds.position + moveOffset, bounds.size);
            map.SetTilesBlock(newBounds, allTiles);

            // 타일맵 데이터 경계 최적화 (불필요한 빈 공간 제거)
            map.CompressBounds();
        }

        Debug.Log($"모든 타일맵이 X:{moveX}, Y:{moveY} 만큼 이동되었습니다.");
    }

    [ContextMenu("마우스 좌표 추적 시작 (20초)")]
    public void StartMouseTracking()
    {
        if (isTrackingMouse) return;

        isTrackingMouse = true;
        // 현재 시간으로부터 20초 뒤를 종료 시간으로 설정합니다.
        trackingEndTime = EditorApplication.timeSinceStartup + 20.0;

        // 씬 뷰(Scene View)의 GUI 업데이트 이벤트에 마우스 추적 함수를 연결합니다.
        SceneView.duringSceneGui += TrackMouseInSceneView;

        Debug.Log("마우스 좌표 추적을 시작합니다. 씬 뷰에서 마우스를 움직여보세요. (20초 후 자동 종료)");
    }

    /// <summary>
    /// 씬 뷰에서 마우스가 움직일 때마다 호출되어 좌표를 계산하는 함수입니다.
    /// </summary>
    private void TrackMouseInSceneView(SceneView sceneView)
    {
        if (!isTrackingMouse) return;

        // 20초가 지나면 추적을 종료하고 이벤트를 해제합니다.
        if (EditorApplication.timeSinceStartup > trackingEndTime)
        {
            isTrackingMouse = false;
            SceneView.duringSceneGui -= TrackMouseInSceneView;
            Debug.Log("20초가 경과하여 마우스 좌표 추적을 종료합니다.");
            return;
        }

        // 현재 마우스 위치를 가져와 씬 뷰의 월드 좌표로 변환합니다.
        Event currentEvent = Event.current;
        if (currentEvent != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            Vector3 worldPos = ray.origin;
            worldPos.z = 0; // 2D 환경이므로 Z축은 0으로 고정합니다.

            // 현재 스크립트가 부착된 Grid 컴포넌트를 가져와 월드 좌표를 그리드 좌표로 변환합니다.
            Grid grid = GetComponent<Grid>();
            if (grid != null)
            {
                currentMouseGridPos = grid.WorldToCell(worldPos);

                // 인스펙터 창이 실시간으로 갱신되도록 강제 업데이트합니다.
                RepaintInspector();
            }
        }
    }

    /// <summary>
    /// 에디터 화면을 강제로 다시 그리게 하여 인스펙터의 값이 실시간으로 변하는 것을 보여줍니다.
    /// </summary>
    private void RepaintInspector()
    {
        EditorUtility.SetDirty(this);
    }

#endif
}