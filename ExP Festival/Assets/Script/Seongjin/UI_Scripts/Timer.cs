using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    private Text text;
    public float time;
    public bool stop;
	// Use this for initialization
	void Start () {
        stop = false;
        text = GetComponent<Text>();
        time = 61f;
	}
	
	
    public void CallTimer()
    {
        time = 61f;
        stop = false;
        StartCoroutine(StartTimer());
    }
    public void StopTimer()
    {
        float originTime = time;
        stop = true;
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
      //  yield return new WaitForSeconds(.3f);
        while (time >= 0.0f && !stop)
        {
            yield return new WaitForSecondsRealtime(1f);
            time -= 1f;
       
            text.text = ((int)time).ToString();
        }
    }
    
}
