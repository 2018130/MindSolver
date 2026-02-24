using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Floating : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [Tooltip("움직임의 모양을 정의하는 그래프입니다.")]
    public AnimationCurve bounceCurve;

    [SerializeField]
    [Tooltip("전체적인 움직임 속도 배율")]
    private float speedMultiplier = 1f;

    [SerializeField]
    [Tooltip("그래프 값에 곱해질 높이 배율 (Y축 이동 거리)")]
    private float heightMultiplier = 2f;

    private Vector3 startPos;
    private float timer;
    private bool floating = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetFloating(false);
        if (bounceCurve == null || bounceCurve.length == 0)
        {
            SetupDefaultCurve();
        }
    }

    void Update()
    {
        if (floating)
        {
            timer += Time.deltaTime * speedMultiplier;

            float curveDuration = bounceCurve[bounceCurve.length - 1].time;

            float currentCurveTime = Mathf.Repeat(timer, curveDuration);

            float curveValue = bounceCurve.Evaluate(currentCurveTime);

            float newY = startPos.y + (curveValue * heightMultiplier);

            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    public void SetFloating(bool active)
    {
        spriteRenderer.enabled = active;
        floating = active;
        if (!active)
        {
            transform.position = startPos;
            timer = 0f;
        }
    }

    private void Reset()
    {
        SetupDefaultCurve();
    }

    private void SetupDefaultCurve()
    {
        bounceCurve = new AnimationCurve();
        bounceCurve.AddKey(new Keyframe(0f, 0f));
        bounceCurve.AddKey(new Keyframe(0.2f, -1f));
        bounceCurve.AddKey(new Keyframe(0.4f, -0.3f));
        bounceCurve.AddKey(new Keyframe(0.6f, -1f));
        bounceCurve.AddKey(new Keyframe(1f, 0f));

        for (int i = 0; i < bounceCurve.length; i++)
        {
            if (bounceCurve[i].value <= -0.9f)
            {
                AnimationUtility.SetKeyLeftTangentMode(bounceCurve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(bounceCurve, i, AnimationUtility.TangentMode.Linear);
            }
        }
    }
}
