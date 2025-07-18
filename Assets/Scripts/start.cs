using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class start : MonoBehaviour
{
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Vector3 forward = this.transform.TransformDirection(Vector3.forward) * 1000;

        if (Physics.Raycast(this.transform.position, forward, out hit))    
        {
            if (GaugeManager.Instance.UpdateGaze(hit))
            {
                if (hit.collider.CompareTag("basic"))
                    PlayerPrefs.SetInt("level", 1);
                else if (hit.collider.CompareTag("hard"))
                    PlayerPrefs.SetInt("level", 2);

                PlayerPrefs.Save();
                SceneManager.LoadScene(1);
            }
        }
    }
}
