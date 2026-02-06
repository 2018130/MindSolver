using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter : MonoBehaviour
{
    [SerializeField]
    private Texture2D brushTexture;
    [SerializeField]
    private float brushSize = 0.1f;
    [SerializeField]
    private RenderTexture maskRT;

    [Header("Progress Settings")]

    [SerializeField]
    private float targetPercentage = 0.9f;
    [SerializeField]
    private float checkInterval = 0.5f;

    // 픽셀 검사용 작은 텍스처
    private Texture2D checkTexture;
    private RenderTexture smallRT;

    private void Start()
    {
        ClearMask();

        smallRT = new RenderTexture(64, 64, 0, RenderTextureFormat.R8);
        checkTexture = new Texture2D(64, 64, TextureFormat.R8, false);

        // 주기적으로 검사하는 코루틴 시작
        StartCoroutine(CheckProgressRoutine());
    }

    private void Update()
    {
        if(InputManager.Singleton.LeftButtonClicked)
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Singleton.MousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.transform == transform)
                {
                    Vector3 localPos = hit.transform.InverseTransformPoint(hit.point);
                    //
                    float u = localPos.x + 0.5f;
                    float v = 0.5f - localPos.y;

                    DrawOnTexture(new Vector2(u, v));
                }
            }
        }
    }

    private void DrawOnTexture(Vector2 uv)
    {
        RenderTexture.active = maskRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, maskRT.width, maskRT.height, 0);

        float x = uv.x * maskRT.width;
        float y = uv.y * maskRT.height;
        float size = brushSize * maskRT.height;

        Graphics.DrawTexture(
            new Rect(x - size / 2, y - size / 2, size, size),
            brushTexture);

        GL.PopMatrix();
        RenderTexture.active = null;
    }

    private void ClearMask()
    {
        RenderTexture.active = maskRT;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = null;
    }

    IEnumerator CheckProgressRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            float progress = CalculateProgress();
            Debug.Log($"현재 진행도: {progress * 100:F1}%");

            if (progress >= targetPercentage)
            {
                Debug.Log("🎉 퍼즐 완성! 다음 스테이지로 이동!");

                // TODO : 여기에 다음 스테이지 이동 로직 추가
                // SceneManager.LoadScene("NextStage");

                yield break;
            }
        }
    }

    float CalculateProgress()
    {
        Graphics.Blit(maskRT, smallRT);

        RenderTexture.active = smallRT;
        checkTexture.ReadPixels(new Rect(0, 0, smallRT.width, smallRT.height), 0, 0);
        checkTexture.Apply();
        RenderTexture.active = null;

        Color[] pixels = checkTexture.GetPixels();
        int paintedCount = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].r > 0.1f)
            {
                paintedCount++;
            }
        }

        return (float)paintedCount / pixels.Length;
    }
}
