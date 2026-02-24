using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WaterSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject waterPrefab;

    [SerializeField]
    private Vector2 waterForceDir;

    private NetworkVariable<bool> canSpawn = new NetworkVariable<bool>(false);

    [SerializeField]
    private float spawnDelay = 0.1f;

    [Space(10f)]

    [SerializeField]
    private int maxWaterSize = 100;
    private GameObject[] waterPool;

    [Space(10f)]
    private int waterSpawnableCount = 0;
    private int maxWaterSpawnableCount = 10;

    [SerializeField]
    private ColorType colorType;

    private void Start()
    {
        CreatePool();
        StartCoroutine(SpawnWater_co());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsServer)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == "Water")
            {
                waterSpawnableCount++;

                if (waterSpawnableCount > maxWaterSpawnableCount)
                {
                    CloseFauset_ServerRpc();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsServer)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == "Water")
            {
                waterSpawnableCount--;

                if (waterSpawnableCount < maxWaterSpawnableCount)
                {
                    OpenFauset_ServerRpc();
                }
            }
        }
    }

    [ServerRpc]
    public void OpenFauset_ServerRpc()
    {
        canSpawn.Value = true;
    }

    [ServerRpc]
    public void CloseFauset_ServerRpc()
    {
        canSpawn.Value = false;
    }

    private IEnumerator SpawnWater_co()
    {
        while (true)
        {
            yield return null;

                if(canSpawn.Value)
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
