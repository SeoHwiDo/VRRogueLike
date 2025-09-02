using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GaugeManager : MonoBehaviour
{
    public static GaugeManager Instance { get; private set; }
    private Image cursorGaugeImage;
    private float gazeTime;
    private float timeElapsed;
    private bool isTriggered;

    void Awake()//중복방지
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    public void ResetGauge()
    {
        timeElapsed = 0.0f;
        isTriggered = false;
    }
    public bool UpdateGaze(RaycastHit hit)
    {
        cursorGaugeImage.fillAmount = timeElapsed;
        isTriggered = Input.GetMouseButtonDown(0);

        if (hit.collider != null && hit.collider.tag != "Untagged")
        {
            timeElapsed += 1.0f / gazeTime * Time.deltaTime;

            if (timeElapsed >= 1.0f || isTriggered)
                return true;
        }
        else
        {
            ResetGauge();
        }

        return false;
    }
}
