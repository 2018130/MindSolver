using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpawner_Single : MonoBehaviour
{
    [SerializeField]
    private GameObject waterPrefab;

    [SerializeField]
    private Vector2 waterForceDir;

    // NetworkVariable 대신 일반 bool 변수를 사용합니다.
    private bool canSpawn = false;

    [SerializeField]
    private float spawnDelay = 0.1f;

    [Space(10f)]

    [SerializeField]
    private int maxWaterSize = 100;
    private GameObject[] waterPool;

    [Space(10f)]
    private int waterSpawnableCount = 0;
    [SerializeField]
    private int maxWaterSpawnableCount = 10;

    [SerializeField]
    private ColorType colorType;

    private void Start()
    {
        CreatePool();
        StartCoroutine(SpawnWater_co());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // IsServer 조건문을 제거했습니다.
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Water")
        {
            waterSpawnableCount++;

            if (waterSpawnableCount > maxWaterSpawnableCount)
            {
                CloseFaucet();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Water")
        {
            waterSpawnableCount--;

            if (waterSpawnableCount < maxWaterSpawnableCount)
            {
                OpenFaucet();
            }
        }
    }

    public void OpenFaucet()
    {
        canSpawn = true;
    }

    public void CloseFaucet()
    {
        canSpawn = false;
    }

    private IEnumerator SpawnWater_co()
    {
        while (true)
        {
            yield return null;

            if (canSpawn)
            {
                SpawnWaterFromPool();
                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }

    private void SpawnWaterFromPool()
    {
        for (int i = 0; i < waterPool.Length; i++)
        {
            if (!waterPool[i].activeSelf)
            {
                GameObject water = waterPool[i];
                water.transform.localPosition = Vector3.zero;
                waterPool[i].SetActive(true);
                water.GetComponent<Rigidbody2D>().AddForce(waterForceDir);
                water.GetComponent<PipeWater>().SetColor(colorType);

                break;
            }
        }
    }

    private void CreatePool()
    {
        waterPool = new GameObject[maxWaterSize];

        for (int i = 0; i < maxWaterSize; i++)
        {
            waterPool[i] = Instantiate(waterPrefab);
            waterPool[i].transform.SetParent(transform);
            waterPool[i].transform.localPosition = Vector3.zero;
            waterPool[i].SetActive(false);
        }
    }

    public static void ReturnToPool(GameObject returnObject)
    {
        returnObject.SetActive(false);
    }
}
