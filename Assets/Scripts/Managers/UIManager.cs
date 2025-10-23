using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("platform UI")]
    [SerializeField] private UIConfig vrUI;
    [SerializeField] private UIConfig defaultUI;

    [Header("UI Mode")]
    [SerializeField] private bool isVR;

    private UIConfig tmpUI;
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (isVR)
            {
                tmpUI = vrUI;
            }
            else
            {
                tmpUI = defaultUI;
            }
        }
        else Destroy(gameObject);
    }
    private void Start()
    {


    }
    //public void ShowStartUI()
    //{
    //    startUI?.SetActive(true);
    //    inGameUI?.SetActive(false);
    //}

    public void ShowInGameUI()
    {
        //startUI?.SetActive(false);
        tmpUI.inGameUI?.SetActive(true);
    }
    public void HideAllUI()
    {
        //startUI?.SetActive(false);
        tmpUI.inGameUI?.SetActive(false);
    }
    public void UpdateCursorUI(bool SkillSelect)
    {
        if (SkillSelect)
        {
            tmpUI.skillCursor.gameObject.SetActive(true);
            tmpUI.bulletCursor.gameObject.SetActive(false);
        }
        else
        {
            tmpUI.skillCursor.gameObject.SetActive(false);
            tmpUI.bulletCursor.gameObject.SetActive(true);
        }
    }
    public void UpdateHPUI(float hp)
    {
        tmpUI.hpGauge.fillAmount = hp / GameManager.Instance.GetPlayerMaxHP();
    }
    public void UpdateSkillUI(float skill)
    {
        tmpUI.skillGauge.fillAmount = skill;
    }
    public void UpdateLevelUI(int level)
    {
        tmpUI.levelText.text = "Level: " + level;
    }
    public void UpdateEnemyCountTextUI(int remainEnemyCount,int SpawnEnemyNum)
    {
        tmpUI.enemyCountText.text = "Enemies: " + remainEnemyCount + "/"+SpawnEnemyNum;
    }
    public void UpdateBulletUI(int max,int cnt)
    {
        tmpUI.BulletText.text = cnt + "/" + max;
    }
    public void UpdateReloadGaze(float progress)
    {
        if (tmpUI.ReloadGaze != null)
        {
            tmpUI.ReloadGaze.fillAmount = progress;
        }
    }
    public void ShowGameOverUI(bool isWin)
    {
        tmpUI.gameOverUI.SetActive(true);
        tmpUI.resultText.text = EnemyManager.Instance.GetkillEnemyCount().ToString();
    }
    public void ShowCaution(string message, float time)
    {
        StartCoroutine(ShowCautionText(message, time));
    }
    public IEnumerator ShowCautionText(string message,float time)
    {
        tmpUI.CautionText.text = message;
        tmpUI.CautionUI.SetActive(true);
        yield return new WaitForSeconds(time);
        tmpUI.CautionUI.SetActive(false);
    }
    public void HideCautionText()
    {
        tmpUI.CautionText.gameObject.SetActive(false);

    }
}
