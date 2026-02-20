using UnityEngine;

/// <summary>
/// [층 이동 시스템 - Floor Changer]
/// 
/// [기능 설명]
/// 플레이어가 계단이나 경사로 등 특정 구역(Trigger)에 진입했을 때,
/// 캐릭터의 '시각적 높이(Sorting Order)'와 '물리적 충돌 레이어(Layer)'를 변경하여
/// 마치 다른 층으로 이동한 것과 같은 효과를 구현하는 스크립트입니다.
/// 
/// [사용 방법]
/// 1. 빈 오브젝트를 생성하고 BoxCollider2D(IsTrigger 체크)를 추가하여 계단 위치에 배치합니다.
/// 2. 이 스크립트를 해당 오브젝트에 컴포넌트로 추가합니다.
/// 3. Inspector 창에서 이동할 목표 층의 Sorting Order와 Layer 이름을 설정합니다.
/// </summary>
public class FloorChanger : MonoBehaviour
{
    [Header("층 이동 설정 (Target Settings)")]
    [Tooltip("플레이어가 이동하게 될 층의 시각적 렌더링 순서(Sorting Order)입니다.\n(예: 1층=0, 2층=10, 숫자가 클수록 화면의 앞쪽에 그려집니다.)")]
    public int targetSortingOrder = 10;

    [Tooltip("플레이어가 이동하게 될 층의 물리적 레이어(Layer) 이름입니다.\n(Unity Inspector 상단 'Layers'에 등록된 정확한 이름을 입력해주세요. 예: 'Player_1F', 'Player_2F')")]
    public string targetLayerName = "Default";

    [Header("감지 필터 설정 (Detection Filter)")]
    [Tooltip("층 이동 효과를 적용할 대상의 태그입니다.\n(주로 'Player' 태그를 사용하며, 다른 태그의 물체는 무시합니다.)")]
    public string playerTag = "Player";

    /// <summary>
    /// [이벤트 핸들러] 트리거 영역에 물체가 진입했을 때 자동으로 호출됩니다.
    /// </summary>
    /// <param name="collision">진입한 물체의 콜라이더 정보</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 유효성 검사: 진입한 물체가 설정된 '플레이어 태그'를 가지고 있는지 확인합니다.
        if (collision.CompareTag(playerTag))
        {
            // 조건이 맞다면 층 변경 로직을 실행합니다.
            ChangeFloor(collision.gameObject);
        }
    }

    /// <summary>
    /// [핵심 로직] 실제 플레이어의 속성을 변경하여 층을 이동시킵니다.
    /// </summary>
    /// <param name="player">속성을 변경할 플레이어 게임 오브젝트</param>
    private void ChangeFloor(GameObject player)
    {
        // 1. 시각적 처리 (Rendering Order)
        // SpriteRenderer 컴포넌트를 가져와서 그리기 순서를 변경합니다.
        // 이를 통해 캐릭터가 2층 타일 위에 그려지거나, 1층 타일 뒤에 숨는 등의 입체감을 표현합니다.
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = targetSortingOrder;

            // (디버그용) 개발 편의를 위해 로그를 남깁니다. 필요 없다면 주석 처리 가능합니다.
            Debug.Log($"[FloorChanger] '{player.name}'의 Sorting Order가 {targetSortingOrder}로 변경되었습니다.");
        }
        else
        {
            Debug.LogWarning($"[FloorChanger] 주의: '{player.name}'에게 SpriteRenderer 컴포넌트가 없습니다. 시각적 층 이동이 적용되지 않았습니다.");
        }

        // 2. 물리적 처리 (Physics Layer)
        // 캐릭터의 레이어를 변경하여, 해당 층의 벽(Wall)이나 바닥(Ground)과만 상호작용하도록 만듭니다.
        // 예를 들어 2층에 있을 때는 1층의 벽을 통과하거나 무시할 수 있게 됩니다.
        int targetLayerID = LayerMask.NameToLayer(targetLayerName);

        // 레이어 이름이 유효한지(존재하는지) 확인 후 적용합니다.
        if (targetLayerID != -1)
        {
            player.layer = targetLayerID;
            Debug.Log($"[FloorChanger] '{player.name}'의 Layer가 '{targetLayerName}'(ID: {targetLayerID})로 변경되었습니다.");
        }
        else
        {
            // 오타 등으로 인해 레이어를 찾을 수 없는 경우 개발자에게 경고를 띄웁니다.
            Debug.LogError($"[FloorChanger] 오류: Inspector에 입력된 '{targetLayerName}'라는 이름의 레이어를 찾을 수 없습니다.\nProject Settings > Tags and Layers에서 레이어 이름을 확인해주세요.");
        }
    }
}