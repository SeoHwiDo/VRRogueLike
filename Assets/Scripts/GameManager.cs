using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public HashSet<string> AllowedTags = new HashSet<string> { "basic", "hard" };

    [Header("Player")]
    public GameObject player;

    [Header("Game State")]
    private int level = 1;
    private int maxLevel = 4;
    private float bulletSpeed = 10f;

    private int enemyCount=0;
    private int killEnemyCount = 0;
    private bool isGameOver = false;

    public int GetEnemyCount()
    {
        return enemyCount;
    }
    public int GetkillEnemyCount()
    {
        return killEnemyCount;
    }
    public void AddEnemyCount(int num=1)
    {
        enemyCount += num;
    }
    public void DeadEnemy()
    {
        enemyCount -= 1;
        killEnemyCount += 1;

    }
    public float GetBulletSpeed()
    {
        return bulletSpeed;
    }
    public void SetBulletSpeed(float speed)
    {
        bulletSpeed = speed;
    }
    private IEnumerator InitGameSequence()
    {
        yield return StartCoroutine(AssetManager.Instance.AssetPreload(() =>
        {
            MapManager.Instance.SetInitializedPrefab();
            EnemyManager.Instance.SetInitializedPrefab();
            MapManager.Instance.GenerateMap();
            player.transform.position = MapManager.Instance.GetPlayerStartPosition();
            StartCoroutine(EnemyManager.Instance.Spawn());
        }));
        //yield return StartCoroutine(MapManager.Instance.PreloadMapPrefabs(() =>
        //{
        //    MapManager.Instance.GenerateMap();
        //    player.transform.position = MapManager.Instance.GetPlayerStartPosition();
        //}));

        //yield return StartCoroutine(EnemyManager.Instance.PreloadEnemyPrefabs(() =>
        //{
        //    StartCoroutine(EnemyManager.Instance.Spawn());
        //}));
    }
    void Awake()
    {

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject); // 필요 시
    }
    void Start()
    {
        MapManager.Instance.SetMapConfig(6, 12); //맵 수정 필요한 경우
        StartCoroutine(InitGameSequence());
    }
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    MapManager.Instance.refreshMap();
        //}
        
    }
    public void EnterSelectSkill()
    {
        Time.timeScale = 0;
    }
    public void OutSelectSkill()
    {
        Time.timeScale = 1;
    }
    
}
