using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance {  get; private set; }
    private AudioSource audioSource;

    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    private Queue<GameObject> firePtcPool = new Queue<GameObject>();
    //스킬명-실제스킬함수 매치 명단
    private Dictionary<string, System.Action> skillActions = new Dictionary<string, System.Action>();
    //스킬명-실제스킬카드오브젝트 매치 명단
    
    private Dictionary<string, GameObject> skillCardInstances = new Dictionary<string, GameObject>();
    //현재 플레이어가 소유중인 스킬들
    private Dictionary<string, GameObject> hasSkills = new Dictionary<string, GameObject>();
    //랜덤 스킬선택을 위한 스킬명 리스트
    private List<string> skillNames;
    [SerializeField]private List<GameObject> cardInstances = new List<GameObject>();
    [SerializeField]private GameObject bullet;
    [SerializeField]private GameObject firePtc;

    private float tempBulletSize;
    private Vector3 bulletLegacySize;
    private int tempBullet;
    private float tempBulletReloadTime;
    private bool bulletReloading;

    private float skillReloadTime;
    private bool skillReloading;
    private int maxSkillNum;

    private int shootCnt;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSkillActions();
            InitializeSkillCardInstance();
            skillNames = skillActions.Keys.ToList();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        bulletReloading = false;
        tempBulletReloadTime = GameManager.Instance.GetBulletReloadTime();

        skillReloading = false;
        skillReloadTime = 1f;
        maxSkillNum = 3;
        shootCnt = 1;
        audioSource = GetComponent<AudioSource>();
        tempBulletSize = GameManager.Instance.GetBulletSize();
        bulletLegacySize=bullet.transform.localScale;
        tempBullet = GameManager.Instance.GetBulletMax();
        UIManager.Instance.UpdateBulletUI(GameManager.Instance.GetBulletMax(), tempBullet);

    }
    private void Update()
    {
        if (tempBullet <= 0)
        {
            ReloadBullet();
        }
    }

    private void PoolingObj(Queue<GameObject> pool,GameObject obj)
    {
        if (pool.Count > 0)
        {
            GameObject reusable = pool.Dequeue();
            reusable.transform.position = this.transform.position;
            reusable.transform.rotation = this.transform.rotation;
            reusable.SetActive(true);
            reusable.transform.localScale = bulletLegacySize* tempBulletSize;
        }
        else
        {
            GameObject newBullet = Instantiate(obj, this.transform.position, this.transform.rotation);
            newBullet.transform.localScale = bulletLegacySize* tempBulletSize;
        }
    }
    public void InPool(GameObject obj)
    {
        switch (obj.tag)
        {
            case "bullet":
                bulletPool.Enqueue(obj);
                return;
            case "ptc":
                firePtcPool.Enqueue(obj);
                return;
            default:
                return;
        }
    }

    private void ShootBullet()
    {
        //audioSource.PlayOneShot(fireSound);
        tempBullet--;
        UIManager.Instance.UpdateBulletUI(GameManager.Instance.GetBulletMax(),tempBullet);
        PoolingObj(firePtcPool, firePtc);
        PoolingObj(bulletPool, bullet);
    }
    private IEnumerator ShootRoutine()
    {
        for (int i = 0; i < shootCnt; i++)
        {
            ShootBullet();
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void StartShooting()
    {
        StartCoroutine(ShootRoutine());
    }
    private IEnumerator ReloadBulletTimerGaze()
    {
        float elapsedTime = 0f;

        while (elapsedTime < tempBulletReloadTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / tempBulletReloadTime;
            UIManager.Instance.UpdateReloadGaze(progress);
            yield return null;
        }
        tempBullet = GameManager.Instance.GetBulletMax();
        UIManager.Instance.UpdateBulletUI(GameManager.Instance.GetBulletMax(), tempBullet);
        if (bulletReloading) bulletReloading = false;
    }
    public void ReloadBullet()
    {
        bulletReloading = true;
        StartCoroutine(ReloadBulletTimerGaze());
    }
    private IEnumerator ReloadSkillTimerGaze()
    {
        float elapsedTime = skillReloadTime;

        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;
            float progress = elapsedTime / skillReloadTime;
            UIManager.Instance.UpdateSkillUI(progress);
            yield return null;
        }
        if (skillReloading) skillReloading = false;
    }
    public GameObject CheckSelectedSkillCard()
    {
        // Raycast를 위한 변수 설정
        RaycastHit hit;
        Vector3 forward = this.transform.TransformDirection(Vector3.forward) * 1000;

        // 1. Raycast 실행
        if (Physics.Raycast(this.transform.position, forward, out hit))
        {
            // 디버깅: Raycast가 무언가를 감지했는지 확인
            Debug.Log($"[SkillManager] Raycast 감지: {hit.transform.gameObject.name}");

            // 2. 입력(클릭) 확인
            if (InputManager.Instance.GetFireKeyDown())
            {
                // 3. 감지된 오브젝트(스킬 카드)를 반환
                return hit.transform.gameObject;
            }
        }

        // 감지되지 않았거나 클릭이 없으면 null 반환
        return null;
    }
    private void OnDrawGizmos()
    {
        // Raycast가 실행될 때와 동일한 변수를 사용하여 Gizmo를 그립니다.
        Vector3 startPoint = this.transform.position;
        Vector3 forwardDirection = this.transform.TransformDirection(Vector3.forward) * 1000;

        // Gizmo 색상 설정
        Gizmos.color = Color.red;

        // Raycast 선 그리기
        Gizmos.DrawLine(startPoint, startPoint + forwardDirection);

        // Unity 에디터에서 Scene 뷰를 켜고, Gizmos 버튼이 활성화되어 있어야 보입니다.
    }
    public bool InvokeSkillByName(string methodName)
    {
        // 1. 딕셔너리에 해당 이름의 Action이 등록되어 있는지 확인
        if (skillActions.ContainsKey(methodName))
        {
            try
            {
                // 2. 딕셔너리에 저장된 Action(메서드)을 실행
                skillActions[methodName].Invoke();
                Debug.Log($"[SkillManager] 델리게이트 호출 성공: {methodName}");
                return true;
            }
            catch (System.Exception ex)
            {
                // 메서드 내부에서 발생한 런타임 오류 처리
                Debug.LogError($"[SkillManager] 델리게이트 실행 중 오류 발생 ({methodName}): {ex.Message}");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"[SkillManager] SkillManager에서 이름이 '{methodName}'인 델리게이트를 찾을 수 없습니다.");
            return false;
        }
    }
    
    public string GetRandomSkillName()
    {
        if (skillActions.Count == 0)
        {
            Debug.LogWarning("등록된 스킬이 없습니다!");
            return null;
        }

        int randomIndex = Random.Range(0, skillNames.Count);
        string randomSkillName = skillNames[randomIndex];
        return randomSkillName;
    }
    public void MakeSkillCard()
    {
        UIManager.Instance.UpdateSkillSelectUI(true);
        
        //생성할 스킬 리스트 
        Dictionary<string,GameObject> selectedSkillNames = new Dictionary<string, GameObject>();
        for (int i = 0; i < maxSkillNum; i++)
        {
            string skillName = GetRandomSkillName();
            Vector3 cardSpawnPos = GameManager.Instance.player.transform.position;
            //랜덤으로 호출한 카드가 중복으로 호출되었거나 이미 보유중인 스킬일때
            if (hasSkills.ContainsKey(skillName) || selectedSkillNames.ContainsKey(skillName))
            {
                //다시 선택
                i--;
                continue;
            }
            //카드의 스폰 위치 지정
            cardSpawnPos.x += -2 + 2 * i;
            cardSpawnPos.y += 1;
            cardSpawnPos.z += 3;
            //소환 후 스킬 카드 인스턴스 관리를 위한 배열에 삽입
            selectedSkillNames.Add(skillName, Instantiate(skillCardInstances[skillName], cardSpawnPos, Quaternion.identity));
            //해당 카드 이름을 스킬카드 이름과 같게 변경
            selectedSkillNames[skillName].name = skillName;
            //스킬카드를 보기 편하게 플레이어를 바라보도록 설정
            selectedSkillNames[skillName].transform.LookAt(GameManager.Instance.player.transform);
        }
    }
    public void AddSkillToHasSkills(string skillName, GameObject cardInstance)
    {
        if (!hasSkills.ContainsKey(skillName))
        {
            // 획득한 스킬 카드 인스턴스 자체를 hasSkills에 저장합니다.
            hasSkills.Add(skillName, cardInstance);

            // 획득하지 않은 나머지 카드를 파괴하는 로직도 여기서 처리할 수 있습니다.
        }
    }
    private void InitializeSkillActions()
    {
        // Add(스킬 이름, 실행할 메서드)
        skillActions.Add("BulletMaxUp", BulletMaxUp);
        skillActions.Add("BulletSizeUp", BulletSizeUp);
        skillActions.Add("BulletDoubleShot", BulletDoubleShot);

    }
    private void InitializeSkillCardInstance()
    {
        foreach(GameObject c in cardInstances)
        {
            skillCardInstances.Add(c.name, c);
        }

    }
    public void ReloadSkill()
    {
        skillReloading = true;
        StartCoroutine(ReloadSkillTimerGaze());
    }
    
    private void BulletMaxUp()
    {
        GameManager.Instance.AddBulletMax(25);
    }
    private void BulletSizeUp()
    {
        tempBulletSize *= 2f;
    }
    private void BulletDoubleShot()
    {
        shootCnt++;
    }
    public bool IsBulletReloading()
    {
        return bulletReloading;
    }
    public bool IsSkillReloading()
    {
        return skillReloading;
    }
}
