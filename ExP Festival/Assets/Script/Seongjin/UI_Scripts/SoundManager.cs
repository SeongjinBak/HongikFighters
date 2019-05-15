using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance = null;

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
    AudioSource audioSource;
    // Use this for initialization
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }
}
