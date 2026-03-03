using System.Collections;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;

public class FallingEffect : MonoBehaviour
{
    private float fallSpeed = 10f;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        FallingTilemapEffect parent = GetComponentInParent<FallingTilemapEffect>();
        fallSpeed = parent.fallSpeed;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void StartFalling()
    {
        Debug.Log("Start falling obstacle");
        StartCoroutine(StartFalling_co());
    }

    private IEnumerator StartFalling_co()
    {
        float randDelay = Random.Range(0f, 3f);

        yield return new WaitForSeconds(randDelay);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos;
        endPos.y -= 5f;
        Color color = spriteRenderer.color;

        while (transform.position.y > endPos.y)
        {
            yield return null;

            Vector3 nextPos = Vector3.down * Time.deltaTime * fallSpeed;
            transform.position += nextPos;
            color.a = (transform.position.y - endPos.y) / (startPos.y - endPos.y);

            spriteRenderer.color = color;
        }
    }
}
