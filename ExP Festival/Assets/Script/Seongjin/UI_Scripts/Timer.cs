/*
 * 게임 내 UI에 사용되는 공통시간을 관리하는 클래스 입니다.
 */ 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    // 화면 출력용 시간 text
    private Text text;
    // 타이머 계산용 시간.
    public float time;
    // 타이머 On/off용 변수.
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

    // 타이머 재생하는 코루틴.
    IEnumerator StartTimer()
    {
        while (time >= 0.0f && !stop)
        {
            yield return new WaitForSecondsRealtime(1f);
            time -= 1f;
            // 10초 미만인 경우, 특별 사운드 실행
            if (time < 10f)
                audioSource.PlayOneShot(Resources.Load<AudioClip>("UiSound/before10Sec_toEnd"));
            if (time < 0)
                break;

            text.text = ((int)time).ToString();
        }
    }

    // 타이머의 시간을 60(즉, 게임시간인 1분)초로 지정하는 함수.
    public void TimeSet60()
    {
        text.text = 60.ToString();
    }
    
}
