using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    private Text text;
    public float time;

	// Use this for initialization
	void Start () {

        text = GetComponent<Text>();
        time = 61f;
	}
	
	
    public void CallTimer()
    {
        time = 61f;
        StartCoroutine(StartTimer());
    }
    public bool TimeEnd()
    {
        if(time >= 0.0f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(1f);
        while (time >= 0.0f)
        {
            yield return null;
            time -= Time.deltaTime;
       
            text.text = ((int)time).ToString();
        }
    }
    
}
