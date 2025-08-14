using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePtc : MonoBehaviour
{
    private void OnParticleSystemStopped()
    {
        Debug.Log("EndPtc");
        this.gameObject.SetActive(false);
        SkillManager.Instance.InPool(this.gameObject);
    }
}
