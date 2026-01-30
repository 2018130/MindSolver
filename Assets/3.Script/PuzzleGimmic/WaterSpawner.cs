using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject waterPrefab;

    [SerializeField]
    private Vector2 waterForceDir;

    [SerializeField]
    private bool canSpawn = false;
    [SerializeField]
    private float spawnDelay = 0.1f;

    [Space(10f)]

    [SerializeField]
    private int maxWaterSize = 100;
    private static GameObject[] waterPool;

    private void Start()
    {
        CreatePool();
        StartCoroutine(SpawnWater_co());
    }

    public void OpenFauset()
    {
        canSpawn = true;
    }
    private void CloseFauset()
    {
        canSpawn = false;
    }

    private IEnumerator SpawnWater_co()
    {
        while(true)
        {
            yield return null;

            if(canSpawn)
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

                break;
            }
        }
    }

    private void CreatePool()
    {
        waterPool = new GameObject[maxWaterSize];

        for(int i = 0; i < maxWaterSize; i++)
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
