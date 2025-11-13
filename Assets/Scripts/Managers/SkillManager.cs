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

    //랜덤 스킬선택을 위한 스킬명단
    private List<string> skillNames;
    //스킬명-실제스킬함수 매치 명단
    private Dictionary<string, System.Action> skillActions = new Dictionary<string, System.Action>();
    //스킬명-실제스킬카드 오브젝트 매치 명단
    private Dictionary<string, GameObject> skillCardInstances = new Dictionary<string, GameObject>();

    //현재 플레이어가 소유중인 스킬들
    private List<string> hasSkills = new List<string>();

    //스킬카드 소환 풀
    Dictionary<string, Queue<GameObject>> skillCardPool = new Dictionary<string, Queue<GameObject>>();

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
            maxSkillNum = 3;
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

    private GameObject PoolingObj(Queue<GameObject> pool, GameObject obj, Vector3 position, Quaternion rotation)
    {
        if (pool != null && pool.Count > 0)
        {
            GameObject reusable = pool.Dequeue();
            reusable.transform.position = position;
            reusable.transform.rotation = rotation;
            reusable.SetActive(true);
            return reusable;
        }
        else
        {
            GameObject newObj = Instantiate(obj, position, rotation);
            return newObj;
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
            case "card":
                skillCardPool[obj.name].Enqueue(obj);
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
        PoolingObj(firePtcPool, firePtc, this.transform.position, this.transform.rotation);
        var _bullet=PoolingObj(bulletPool, bullet, this.transform.position, this.transform.rotation);
        if( _bullet != null ) _bullet.transform.localScale = bulletLegacySize * tempBulletSize;
    }
    private IEnumerator ShootRoutine()
    {
        for (int i = 0; i < shootCnt; i++)
        {
            if(tempBullet<=0)break;
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
    public List<GameObject> MakeSkillCard()
    {
        Debug.Log("MakeSkillCard 호출");
        //생성할 스킬 리스트 
        List<GameObject> tmpSkillCard=new List<GameObject>();
        for (int i = 0; i < maxSkillNum; i++)
        {
            string skillName = GetRandomSkillName();
            if (!skillCardPool.ContainsKey(skillName))
            {
                skillCardPool[skillName] = new Queue<GameObject>();
            }
            Debug.Log("호출된 스킬:"+skillName);
            Transform cardTransform= GameManager.Instance.player.transform;
            Vector3 cardSpawnPos= cardTransform.position;
            cardSpawnPos.x += -2 + 2 * i;
            cardSpawnPos.y = 1.5f;
            cardSpawnPos.z += 3;
            var _skillCard=PoolingObj(skillCardPool[skillName], skillCardInstances[skillName], cardSpawnPos, cardTransform.rotation);
            //해당 카드 이름을 스킬카드 이름과 같게 변경
            _skillCard.name = skillName;
            //스킬카드를 보기 편하게 플레이어를 바라보도록 설정
            _skillCard.transform.LookAt(new Vector3(GameManager.Instance.player.transform.position.x,1, GameManager.Instance.player.transform.position.z));
            tmpSkillCard.Add(_skillCard);
        }
        return tmpSkillCard;
    }
    public void DelSkillCard(List<GameObject> cards)
    {
        foreach (var c in cards)
        {
            c.SetActive(false);
        }
    }
    public void AddSkillToHasSkills(string skillName,GameObject skillCard)
    {
            // 획득한 스킬 카드 인스턴스 자체를 hasSkills에 저장합니다.
            hasSkills.Add(skillName);
            InPool(skillCard);

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
        foreach(var bp in bulletPool)
        {
            bp.transform.localScale = bulletLegacySize * tempBulletSize;
        }
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
}
