using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyManager : MonoBehaviour
{
    //addressable로부터 preload한 프리팹 관리
    public static EnemyManager Instance { get; private set; }
    private Dictionary<string, GameObject> prefabCache;
    private List<GameObject> enemyPrefabs;
    private List<GameObject> deadPtcPool = new List<GameObject>();
    public void InstanceEnemyDeadPtc(Vector3 pos)
    {
        GameObject reusable = deadPtcPool.Find(t => t != null && !t.activeInHierarchy);
        if (reusable != null)
        {
            reusable.transform.position = pos;
            reusable.SetActive(true);
        }
        else
        {
            Addressables.InstantiateAsync("EnemyDeadPtc").Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var it = handle.Result;
                    it.transform.position = pos;
                    deadPtcPool.Add(it);
                }
            };
        }
    }

public void SetInitializedPrefab()
{
    prefabCache = AssetManager.Instance.GetPrefabCache();
    enemyPrefabs = AssetManager.Instance.GetLabelCache("Enemy");
}
public IEnumerator Spawn()
    {
        int spawnDroneNum = 10;
        for (int i = 0; i < spawnDroneNum; i++)
        {
            yield return new WaitForSeconds(0.5f);
            SpawnEnemy();
            yield return new WaitForSeconds(6f);
        }
    }

    //Enemy 풀링
    private Dictionary<string,List<GameObject>> enemyPool = new Dictionary<string,List<GameObject>>();
    private void SetInstance(GameObject instance, Vector3 localPoistion, Quaternion localRotation, Transform parent, string nameSuffix = "", string poolName = null)
    {
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPoistion;
        instance.transform.localRotation = localRotation;

        instance.name = instance.name.Replace("(Clone)", nameSuffix);
        if (poolName != null)
        {
            if (!enemyPool.ContainsKey(poolName))
                enemyPool[poolName] = new List<GameObject>();

            enemyPool[poolName].Add(instance);
        }
    }
    public void SpawnEnemy()
    {
        //드론 오브젝트 선택

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("No map tiles found with label 'MapTile'");
            return;
        }

        GameObject rndEnemy= enemyPrefabs[Random.Range(0, enemyPrefabs.Count)]; 

        //드론 소환할 위치

        GameObject[] SpawnPointList = MapManager.Instance.GetEnemySpawnPoint();
        if (SpawnPointList == null || SpawnPointList.Length != 4)
        {
            Debug.LogError("SpawnPointList is null or empty");
            return;
        }
        GameObject rndEnemySpawnPoint = SpawnPointList[Random.Range(0, SpawnPointList.Length)];
        if (rndEnemySpawnPoint == null)
        {
            Debug.LogError("Selected enemy spawn point is null");
            return;
        }
        Vector3 rndEnemySpawnPointPos = rndEnemySpawnPoint.transform.position + Vector3.down;

        //pool 잔여량 확인
        if (!enemyPool.ContainsKey(rndEnemy.name))
        {
            enemyPool[rndEnemy.name] = new List<GameObject>();
        }
        GameObject reusable = enemyPool[rndEnemy.name].Find(t => t != null && !t.activeInHierarchy);
        
        if (reusable != null)
        {
            reusable.transform.position = rndEnemySpawnPointPos;
            reusable.SetActive(true);
        }
        else
        {
            reusable = Instantiate(rndEnemy, rndEnemySpawnPointPos, Quaternion.identity);
            enemyPool[rndEnemy.name].Add(reusable);

            var spawnPtc = Instantiate(prefabCache["EnemySpawnPtc"]);
            SetInstance(spawnPtc, Vector3.zero, Quaternion.identity, reusable.transform);
        }
    }
}
