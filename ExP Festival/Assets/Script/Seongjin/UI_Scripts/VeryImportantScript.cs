using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class VeryImportantScript : MonoBehaviour {
    [TextArea]
    public string [] thankyou;
    Text txt;
    // Use this for initialization
    void Start () {
        txt = GameObject.Find("Text").GetComponent<Text>();
        txt.text = thankyou[0];
        StartCoroutine(OutPutText());
	}
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene("MainStart");
        }
    }
    IEnumerator OutPutText()
    {

        float tmp = 0.02f;
        Color color = txt.color;
    
        for (int i = 1; i < thankyou.Length; i++)
        {
            yield return new WaitForSeconds(1.0f);
            while (txt.color.a >= 0)
            {
                color.a -= tmp;
                txt.color = color;
                yield return new WaitForSeconds(0.02f);
            }

            txt.text = thankyou[i]; 

            while (txt.color.a <= 1)
            {
                color.a += tmp;
                txt.color = color;
                yield return new WaitForSeconds(0.02f);
            }
        }
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("MainStart");
    }
}
