using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public HashSet<string> AllowedTags = new HashSet<string> { "basic", "hard" };

    [Header("Player")]
    public GameObject player;

    [Header("UI")]
    public GameObject endUI;


    [Header("Game State")]
    private int level = 1;
    private int maxLevel = 4;
    private float HP = 10;
    private int enemyCount=0;
    private int killEnemyCount = 0;
    private bool isGameOver = false;

    public int GetEnemyCount()
    {
        return enemyCount;
    }
    public float GetPlayerHP()
    {
        return HP;
    }
    public void addEnemyCount(int num=1)
    {
        enemyCount += num;
    }
    public void DeadEnemy()
    {
        enemyCount -= 1;
        killEnemyCount += 1;

    }
    private IEnumerator InitGameSequence()
    {
        yield return StartCoroutine(MapManager.Instance.PreloadMapPrefabs(() =>
        {
            MapManager.Instance.GenerateMap();
            player.transform.position = MapManager.Instance.GetPlayerStartPosition();
        }));

        yield return StartCoroutine(EnemyManager.Instance.PreloadEnemyPrefabs(() =>
        {
            StartCoroutine(EnemyManager.Instance.Spawn());
        }));
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
