using UnityEngine;
using System.Collections;

/// <summary>
/// 종이 펼침 애니메이션 및 퍼즐 완료 후 잉크 디졸브(사라짐) 연출을 관리하는 클래스
/// </summary>
public class PaperManager : MonoBehaviour
{
    [Header("Paper Settings")]
    [Tooltip("애니메이션 컴포넌트가 포함된 9단계 종이 오브젝트")]
    public GameObject animatedPaper;

    [Tooltip("종이 펼침 애니메이션을 제어할 Animator")]
    public Animator paperAnimator;

    [Tooltip("애니메이션 종료 후 표시될 완전히 펼쳐진 상태의 배경 종이 오브젝트")]
    public GameObject staticBackground;

    [Header("Dissolve Settings")]
    [Tooltip("디졸브 쉐이더(Material)가 적용된 배경 종이의 SpriteRenderer")]
    public SpriteRenderer backgroundRenderer;

    [Tooltip("잉크 디졸브 연출 속도 (값이 작을수록 천천히 사라짐)")]
    public float dissolveSpeed = 0.5f;

    // =========================================================
    // 1. 애니메이션 시작 및 연출 전환 메서드
    // =========================================================

    /// <summary>
    /// 종이 펼침 애니메이션을 시작할 때 외부에서 호출하는 메서드
    /// </summary>
    public void StartPaperUnfoldAnimation()
    {
        // 배경은 숨기고 애니메이션 오브젝트를 활성화
        if (staticBackground != null) staticBackground.SetActive(false);
        if (animatedPaper != null) animatedPaper.SetActive(true);

        // 애니메이터가 존재한다면 상태를 초기화하여 처음부터 재생되도록 처리
        if (paperAnimator != null)
        {
            paperAnimator.Rebind();
            paperAnimator.Update(0f);
        }
    }

    /// <summary>
    /// 애니메이션의 마지막 프레임에서 Animation Event를 통해 호출되는 메서드.
    /// 애니메이션 종이를 숨기고, 정지된 배경 종이로 자연스럽게 교체함.
    /// </summary>
    public void SwitchToBackground()
    {
        // 코루틴 실행을 위해 GameObject 자체를 끄지 않고 SpriteRenderer 컴포넌트만 비활성화
        if (animatedPaper != null)
        {
            SpriteRenderer sr = animatedPaper.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }

        // 고정된 배경 종이 활성화
        if (staticBackground != null)
        {
            staticBackground.SetActive(true);
        }

        // 디졸브 쉐이더의 진행도를 0(완전한 불투명 상태)으로 초기화
        if (backgroundRenderer != null)
        {
            backgroundRenderer.material.SetFloat("_DissolveAmount", 0f);
        }
    }

    // =========================================================
    // 2. 오브젝트를 사라지게 해주는 메서드 (디졸브 연출)
    // =========================================================

    /// <summary>
    /// 퍼즐 완료 시 외부에서 호출하여 종이를 잉크처럼 번지며 사라지게 하는 메서드
    /// </summary>
    public void StartDissolveEffect()
    {
        StartCoroutine(InkDissolveRoutine());
    }

    /// <summary>
    /// 실질적으로 Material의 _DissolveAmount 값을 조절하여 투명하게 만드는 코루틴
    /// </summary>
    private IEnumerator InkDissolveRoutine()
    {
        if (backgroundRenderer == null) yield break;

        float currentAmount = 0f;
        Material mat = backgroundRenderer.material;

        // _DissolveAmount 값이 1.0(완전 투명)이 될 때까지 프레임마다 수치 증가
        while (currentAmount < 1.0f)
        {
            currentAmount += Time.deltaTime * dissolveSpeed;
            mat.SetFloat("_DissolveAmount", currentAmount);
            yield return null;
        }

        // 연출이 완전히 끝난 후 메모리/성능을 위해 오브젝트 비활성화
        if (staticBackground != null)
        {
            staticBackground.SetActive(false);
        }
    }
}