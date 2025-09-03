using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEditor.PlayerSettings;

public class EnemyManager : MonoBehaviour
{
    //addressable로부터 preload한 프리팹 관리
    public static EnemyManager Instance { get; private set; }
    private List<GameObject> deadPtcPool = new List<GameObject>();
    private List<GameObject> hitPtcPool = new List<GameObject>();
    [SerializeField]private EnemyConfig drone;
    private int spawnEnemyNum;
    private int remainEnemyCount;
    private int killEnemyCount;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    void Start()
    {
        spawnEnemyNum = 10;
        remainEnemyCount = 0;
        killEnemyCount = 0;
        UIManager.Instance.UpdateEnemyCountTextUI(killEnemyCount, spawnEnemyNum);

    }
    //오브젝트 관리

    public IEnumerator Spawn()
    {

        for (int i = 0; i < spawnEnemyNum; i++)
        {
            yield return new WaitForSeconds(0.5f);
            SpawnEnemy();
            yield return new WaitForSeconds(6f);
        }
        if (GetremainEnemyCount() == 0) GameManager.Instance.EnterSelectSkill();
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
            var deadPtc = Instantiate(drone.enemyDeadPtc);
            deadPtc.transform.position = pos;

        }
    }
    public void InstanceHitPtc(Vector3 hitPos)
    {
        GameObject reusable = hitPtcPool.Find(t => t != null && !t.activeInHierarchy);
        if (reusable != null)
        {
            reusable.transform.position = hitPos;
            reusable.SetActive(true);
        }
        else
        {
            var hitPtc = Instantiate(drone.enemyHitPtc);
            hitPtc.transform.position = hitPos;
        }
    }
    public void SpawnEnemy()
    {
        //드론 오브젝트 선택

        if (drone.enemyPrefab == null || drone.enemyPrefab.Length == 0)
        {
            Debug.LogError("No map tiles found with label 'MapTile'");
            return;
        }

        GameObject rndEnemy= drone.enemyPrefab[Random.Range(0, drone.enemyPrefab.Length)]; 

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

            var spawnPtc = Instantiate(drone.enemySpawnPtc);
            SetInstance(spawnPtc, Vector3.zero, Quaternion.identity, reusable.transform);
        }
    }
    //enemySystem 관리
    public void AddSpawnEnemyNum(int num)
    {
        spawnEnemyNum += num; 
    }
    public int GetremainEnemyCount()
    {
        return remainEnemyCount;
    }
    public int GetkillEnemyCount()
    {
        return killEnemyCount;
    }
    public void AddremainEnemyCount(int num = 1)
    {
        remainEnemyCount += num;
    }
    public void DeadEnemy()
    {
        killEnemyCount += 1;
        UIManager.Instance.UpdateEnemyCountTextUI(killEnemyCount, spawnEnemyNum);


    }
}
