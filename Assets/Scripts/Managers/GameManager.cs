using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public HashSet<string> AllowedTags = new HashSet<string> { "basic", "hard" };

    [Header("Player")]
    public GameObject player;
    [SerializeField]private float MaxHP = 10;
    [SerializeField]private float bulletSpeed = 10f;
    [SerializeField]private float bulletDamage = 1f;
    [SerializeField]private float bulletSize = 1f;
    [SerializeField] private float bulletReloadTime = 3f;
    [SerializeField]private int bulletMax = 25;

    [Header("Game State")]
    private bool selectSkillTime=false;
    [SerializeField] private int level = 0;
    [SerializeField] private int maxLevel = 4;
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private int SpawnEnemyNum = 1;
    public float GetPlayerMaxHP()
    {
        return MaxHP;
    }
    public float GetBulletDamage()
    {
        return bulletDamage;
    }
    public float GetBulletSpeed()
    {
        return bulletSpeed;
    }

    public float GetBulletSize()
    {
        return bulletSize;
    }
    public float GetBulletReloadTime()
    {
        return bulletReloadTime;
    }
    public void AddBulletMax(int max)
    {
        bulletMax += max;
    }
    public int GetBulletMax()
    {
        return bulletMax;
    }
    private void GameLevelUp()
    {
        if (level <= maxLevel) level += 1;
    }
    public bool GetSelectSkillTime()
    {
        return selectSkillTime;
    }
    public void SetSelectSkillTime(bool isSelectTime)
    {
        selectSkillTime = isSelectTime;
    }
    public int GetSpawnEnemyNum()
    {
        return SpawnEnemyNum;
    }
    private void InitGameSequence()
    {
        MapManager.Instance.GenerateMap();
        player.transform.position = MapManager.Instance.GetPlayerStartPosition();
        EnterSelectSkill();
        
        UIManager.Instance.ShowInGameUI();


    }
    void Awake()
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
        //MapManager.Instance.SetMapConfig(6, 12); //맵 수정 필요한 경우
        InitGameSequence();
        UIManager.Instance.UpdateLevelUI(level);
    }
    void Update()
    {
        if (EnemyManager.Instance.GetRemainEnemyCount() <= 0 && !selectSkillTime)
        {
            EnterSelectSkill();
        }

        // 2. 스킬 선택 시간 동안 입력 처리 로직 실행
        if (selectSkillTime)
        {
            HandleSkillSelectionInput();
        }

    }
    private void HandleSkillSelectionInput()
    {
        // SkillManager에게 클릭된 스킬 카드가 있는지 확인을 요청합니다.
        GameObject selectedCard = SkillManager.Instance.CheckSelectedSkillCard();

        if (selectedCard != null)
        {
            // 스킬 카드가 성공적으로 클릭되었을 경우
            string methodName = selectedCard.name; // 오브젝트 이름 == 메서드 이름

            // 1. 스킬 적용 (SkillManager의 델리게이트 호출)
            bool skillApplied = SkillManager.Instance.InvokeSkillByName(methodName);

            if (skillApplied)
            {
                // 2. 획득한 스킬을 보유 목록에 추가
                SkillManager.Instance.AddSkillToHasSkills(methodName, selectedCard); // SkillManager에 이 메서드를 추가해야 함

                // 3. 스킬 선택 모드 종료 (게임 재개)
                OutSelectSkill();

                // 4. 다음 레벨 적 스폰 시작
                StartCoroutine(EnemyManager.Instance.Spawn());
            }
            else
            {
                Debug.LogError($"[GameManager] 스킬 '{methodName}' 적용 실패.");
            }
        }
    }
    List<GameObject> skillCardList;
    public void EnterSelectSkill()
    {
        selectSkillTime = true;
        GameLevelUp();
        UIManager.Instance.UpdateSkillSelectUI(true);
        MapManager.Instance.skillSelectMap();
        skillCardList=SkillManager.Instance.MakeSkillCard();
        GameManager.Instance.player.GetComponent<BoxCollider>().enabled = false;
    }
    public void OutSelectSkill()
    {
        selectSkillTime = false;
        SkillManager.Instance.DelSkillCard(skillCardList);
        UIManager.Instance.UpdateSkillSelectUI(false);
        EnemyManager.Instance.SetEnemys(level);
        // 획득되지 않은 나머지 스킬 카드 파괴/비활성화 로직 추가
        GameManager.Instance.player.GetComponent<BoxCollider>().enabled = true;
        UIManager.Instance.UpdateLevelUI(level);
        MapManager.Instance.refreshMap();
    }

}
