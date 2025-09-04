using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance {  get; private set; }
    private AudioSource audioSource;

    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    private Queue<GameObject> firePtcPool = new Queue<GameObject>();

    [SerializeField]private GameObject bullet;
    [SerializeField]private GameObject firePtc;

    private float tempBulletSize;
    private Vector3 bulletLegacySize;
    private int tempBullet;
    private float tempBulletReloadTime;
    private bool bulletReloading;

    private float skillReloadTime;
    private bool skillReloading;

  

    private int shootCnt;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지해야 한다면 추가
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
