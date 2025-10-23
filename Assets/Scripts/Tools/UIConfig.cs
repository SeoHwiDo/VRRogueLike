using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIConfig : MonoBehaviour
{

    [Header("Cursor")]
    public Image skillCursor;
    public Image bulletCursor;

    [Header("Inspect Data")]
    public Image hpGauge;
    public Image skillGauge;
    public Image skillWindow;

    [Header("Stat Data")]
    public TMP_Text levelText;
    public TMP_Text enemyCountText;
    public TMP_Text BulletText;
    public Image ReloadGaze;

    [Header("Result Data")]
    public TMP_Text resultText;

    [Header("System Data")]
    public TMP_Text CautionText;

    [Header("UI")]
    public GameObject CautionUI;
    public GameObject inGameUI;
    public GameObject gameOverUI;

}
