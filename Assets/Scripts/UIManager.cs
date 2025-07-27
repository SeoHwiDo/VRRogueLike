using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Result UI")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject gameOverUI;

    [Header("System UI")]
    [SerializeField] private TMP_Text CautionText;
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
    public void UpdateStatusUI(float hp, float skillCooldown)
    {
        hpInspector.fillAmount = hp / GameManager.Instance.GetPlayerHP();
        skillInspector.fillAmount = skillCooldown;
    }
    public void UpdateStatUI(int level, int enemyCount)
    {
        levelText.text = "Level: " + level;
        enemyCountText.text = "Enemies: " + enemyCount;
    }

    public void ShowGameOverUI(bool isWin)
    {
        gameOverUI.SetActive(true);
        resultText.text = isWin ? "You Win!" : "Game Over!";
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
