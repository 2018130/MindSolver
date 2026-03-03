using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHoverTransparency : MonoBehaviour
{
    [Header("설정")]
    private Tilemap targetTilemap;
    [Range(0f, 1f)]
    public float hoverAlpha = 0.5f; // 마우스 오버 시 투명도 (0: 완전 투명, 1: 불투명)

    private Vector3Int previousCellPos;
    private bool hasPreviousTile = false;

    private void Awake()
    {
        targetTilemap = GetComponent<Tilemap>();
    }
    void Update()
    {
        // 1. 마우스 화면 좌표를 월드 좌표로 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);
        mouseWorldPos.z = 0f; // 2D 환경이므로 z축 평면을 0으로 맞춤 (필요에 따라 조정)

        // 2. 월드 좌표를 타일맵의 그리드(Cell) 좌표로 변환
        Vector3Int currentCellPos = targetTilemap.WorldToCell(mouseWorldPos);

        // 3. 마우스가 새로운 타일 칸으로 이동했을 때만 실행
        if (currentCellPos != previousCellPos)
        {
            // 이전에 투명해진 타일이 있다면 원래 색(불투명)으로 복구
            if (hasPreviousTile)
            {
                ResetTileColor(previousCellPos);
            }

            // 현재 마우스 위치에 타일이 존재하는지 확인
            if (targetTilemap.HasTile(currentCellPos))
            {
                SetTileTransparent(currentCellPos);
                previousCellPos = currentCellPos;
                hasPreviousTile = true;
            }
            else
            {
                // 타일이 없는 빈 공간인 경우
                hasPreviousTile = false;
            }
        }
    }

    private void SetTileTransparent(Vector3Int pos)
    {
        // 핵심: 타일의 색상 잠금을 해제해야 SetColor가 작동합니다.
        targetTilemap.SetTileFlags(pos, TileFlags.None);

        // 타일의 알파값을 낮춤 (Color의 4번째 인자가 Alpha)
        targetTilemap.SetColor(pos, new Color(1f, 1f, 1f, hoverAlpha));
    }

    private void ResetTileColor(Vector3Int pos)
    {
        if (targetTilemap.HasTile(pos))
        {
            // 원래 색상(보통 Color.white)으로 복구
            targetTilemap.SetColor(pos, Color.white);
        }
    }
}