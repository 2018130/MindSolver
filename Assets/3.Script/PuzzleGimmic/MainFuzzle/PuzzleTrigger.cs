using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEngine.InputSystem;

public class PuzzleTrigger : MonoBehaviour
{
    [Header("타일맵 설정")]
    public Tilemap obstacleTilemap; // 장애물 타일이 배치된 타일맵 (이 아래에 통로 타일이 깔려 있어야 함)

    [Header("색상 설정")]
    public Color activeColor = new Color(0.7f, 0.5f, 1f); // 변경될 목표 색상 (연보라색)
    public float fadeDuration = 0.8f; // 색상이 변하고 사라지는 데 걸리는 시간

    void Update()
    {
        // Pointer는 마우스와 터치 입력을 모두 포함합니다.
        // 현재 포인터 장치가 연결되어 있고, 이번 프레임에 눌렸는지 확인합니다.
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            // 1. 화면상의 입력 좌표(Screen Position)를 가져옵니다.
            Vector2 screenPos = Pointer.current.position.ReadValue();

            // 2. 화면 좌표를 게임 월드 좌표(World Position)로 변환합니다.
            // Z값 10f는 카메라와 타일맵 사이의 거리를 고려한 값입니다.
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));

            // 3. 2D 타일맵은 Z축이 0이므로, 정확한 좌표 계산을 위해 Z값을 0으로 고정합니다.
            worldPos.z = 0;

            // 4. 월드 좌표를 타일맵의 그리드 좌표(Cell Position)로 변환합니다.
            Vector3Int tilePos = obstacleTilemap.WorldToCell(worldPos);

            // 5. 해당 그리드 좌표에 장애물 타일이 존재하는지 확인합니다.
            if (obstacleTilemap.HasTile(tilePos))
            {
                // 타일이 있다면 색상 변경 및 삭제 코루틴을 실행합니다.
                StartCoroutine(FadeAndRemoveTile(tilePos));
            }
        }
    }

    // 타일의 색상을 서서히 변경한 후 제거하는 코루틴 함수입니다.
    IEnumerator FadeAndRemoveTile(Vector3Int pos)
    {
        // 타일의 색상을 코드로 변경할 수 있도록 Lock을 해제합니다.
        obstacleTilemap.SetTileFlags(pos, TileFlags.None);

        Color startColor = obstacleTilemap.GetColor(pos);
        float elapsedTime = 0;

        // 설정한 시간동안 색상을 서서히 변경합니다.
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            // 기존 색상에서 목표 색상으로 부드럽게 변경합니다.
            obstacleTilemap.SetColor(pos, Color.Lerp(startColor, activeColor, t));
            yield return null;
        }

        // 색상 변경 연출이 끝나면 장애물 타일을 제거하여 아래에 있던 통로 타일이 드러나게 합니다.
        obstacleTilemap.SetTile(pos, null);
    }
}