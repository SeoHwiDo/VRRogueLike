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



    [Header("Result UI")]
    [SerializeField] private TMP_Text resultText;

    [Header("System UI")]
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
    public void UpdateEnemyCountTextUI(int enemyCount)
    {
        enemyCountText.text = "Enemies: " + enemyCount;
    }
    public void UpdateBulletUI(int max,int cnt)
    {

    }

    public void ShowGameOverUI(bool isWin)
    {
        gameOverUI.SetActive(true);
        resultText.text = EnemyManager.Instance.GetkillEnemyCount().ToString();
    }
    public void ShowCautionText(string message)
    {
        CautionText.text = message;
        CautionText.gameObject.SetActive(true);
    }
    public void HideCautionText()
    {
        CautionText.gameObject.SetActive(false);

    }
}
