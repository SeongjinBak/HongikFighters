using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    private Text text;
    public float time;
    public bool stop;

    private AudioSource audioSource;
    private void Awake()
    {
        text = GetComponent<Text>();
    }
    // Use this for initialization
    void Start () {
        audioSource = GetComponent<AudioSource>();

        stop = false;
        text = GetComponent<Text>();
        time = 61f;
	}
	
	
    public void CallTimer()
    {
        time = 60f;
        stop = false;
        StartCoroutine(StartTimer());
    }
    public void StopTimer()
    {
        float originTime = time;
        stop = true;
        StopAllCoroutines();
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
            if (time < 10f) audioSource.PlayOneShot(Resources.Load<AudioClip>("UiSound/before10Sec_toEnd"));
            if (time < 0) break;
            text.text = ((int)time).ToString();
        }
    }

    public void TimeSet60()
    {
        text.text = 60.ToString();
    }
    
}
