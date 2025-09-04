using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.TouchScreenKeyboard;

public class UIManager : MonoBehaviour
{

    [Header("Cursor UI")]
    [SerializeField] private Image skillCursor;
    [SerializeField] private Image bulletCursor;

    [Header("Status UI")]
    [SerializeField] private Image hpInspector;
    [SerializeField] private Image skillInspector;

    [Header("Stat UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text enemyCountText;
    [SerializeField] private TMP_Text BulletText;
    [SerializeField] private Image ReloadGaze;




    [Header("Result UI")]
    [SerializeField] private TMP_Text resultText;

    [Header("System UI")]
    [SerializeField] private GameObject CautionUI;
    [SerializeField] private TMP_Text CautionText;

    [SerializeField] private GameObject inGameUI;
    [SerializeField] private GameObject gameOverUI;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //public void ShowStartUI()
    //{
    //    startUI?.SetActive(true);
    //    inGameUI?.SetActive(false);
    //}

    public void ShowInGameUI()
    {
        //startUI?.SetActive(false);
        inGameUI?.SetActive(true);
    }
    public void HideAllUI()
    {
        //startUI?.SetActive(false);
        inGameUI?.SetActive(false);
    }
    public void UpdateCursorUI(bool SkillSelect)
    {
        if (SkillSelect)
        {
            skillCursor.gameObject.SetActive(true);
            bulletCursor.gameObject.SetActive(false);
        }
        else
        {
            skillCursor.gameObject.SetActive(false);
            bulletCursor.gameObject.SetActive(true);
        }
    }
    public void UpdateHPUI(float hp)
    {
        hpInspector.fillAmount = hp / GameManager.Instance.GetPlayerMaxHP();
    }
    public void UpdateSkillUI(float skill)
    {
        skillInspector.fillAmount = skill;
    }
    public void UpdateLevelUI(int level)
    {
        levelText.text = "Level: " + level;
    }
    public void UpdateEnemyCountTextUI(int remainEnemyCount,int SpawnEnemyNum)
    {
        enemyCountText.text = "Enemies: " + remainEnemyCount + "/"+SpawnEnemyNum;
    }
    public void UpdateBulletUI(int max,int cnt)
    {
        BulletText.text = cnt + "/" + max;
    }
    public void UpdateReloadGaze(float progress)
    {
        if (ReloadGaze != null)
        {
            ReloadGaze.fillAmount = progress;
        }
    }
    public void ShowGameOverUI(bool isWin)
    {
        gameOverUI.SetActive(true);
        resultText.text = EnemyManager.Instance.GetkillEnemyCount().ToString();
    }
    public void ShowCaution(string message, float time)
    {
        StartCoroutine(ShowCautionText(message, time));
    }
    public IEnumerator ShowCautionText(string message,float time)
    {
        CautionText.text = message;
        CautionUI.SetActive(true);
        yield return new WaitForSeconds(time);
        CautionUI.SetActive(false);
    }
    public void HideCautionText()
    {
        CautionText.gameObject.SetActive(false);

    }
}
