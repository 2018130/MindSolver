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

#if UNITY_EDITOR
    [SerializeField]
    private bool canSpawn = false;
#else
    [SerializeField]
    private NetworkVariable<bool> canSpawn = new NetworkVariable<bool>(false);
#endif
    [SerializeField]
    private float spawnDelay = 0.1f;

    [Space(10f)]

    [SerializeField]
    private int maxWaterSize = 100;
    private static GameObject[] waterPool;

    [Space(10f)]
    private int waterSpawnableCount = 0;
    private int maxWaterSpawnableCount = 10;

#if UNITY_EDITOR
#else
#endif

    private void Start()
    {
        CreatePool();
        StartCoroutine(SpawnWater_co());
#if UNITY_EDITOR
        OpenFauset();
#else
        OpenFauset_ServerRpc();
#endif
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
#if UNITY_EDITOR
                    CloseFauset();
#else
                    CloseFauset_ServerRpc();
#endif
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
#if UNITY_EDITOR
                    OpenFauset();
#else
                    OpenFauset_ServerRpc();
#endif
                }
            }
        }
    }

#if UNITY_EDITOR
    public void OpenFauset()
    {
        canSpawn = true;
    }
#else
    [ServerRpc]
    public void OpenFauset_ServerRpc()
    {
        canSpawn.Value = true;
    }
#endif

#if UNITY_EDITOR
    private void CloseFauset()
    {
        canSpawn = false;
    }

#else
    [ServerRpc]
    private void CloseFauset_ServerRpc()
    {
        canSpawn.Value = false;
    }

#endif

    private IEnumerator SpawnWater_co()
    {
        while(true)
        {
            yield return null;

#if UNITY_EDITOR
            if(canSpawn)
#else
                if(canSpawn.Value)
#endif
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
