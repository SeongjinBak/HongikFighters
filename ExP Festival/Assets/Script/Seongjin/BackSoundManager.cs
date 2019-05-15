/*
 * 배경음악 재생용 매니저
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackSoundManager : MonoBehaviour {
    public static BackSoundManager instance = null;

    private void Awake()
    {
        // static변수에 할당이 안되어있으면 할당.
        if (instance == null)
        {
            instance = this;
        }
        // 할당이 되어있으면 만들어진거 삭제.
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
        // 씬 전환시에도 게임오브젝트 삭제 안함.
        DontDestroyOnLoad(this.gameObject);
    }

    public AudioSource audioSource;
    // Use this for initialization
    void Start () {
        //audioSource = GetComponent<AudioSource>();
	}
	
    // 배경음악을 매개변수로 받은 클립으로 교체한다.
	public void ChangeBackGroundMusic(AudioClip audioClip)
    {

        audioSource.Stop();
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopBGM()
    {
        audioSource.Stop();
    }
}
