/*
 * 작성자 : 백성진
 * 메인 스타트 화면에서 "아무 키나 눌렸을 때" 다음 씬으로 전환 한다.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainStart : MonoBehaviour {
    bool isButtonPressed;
    public GameObject spark;
    public Image Fader;
    private void Start()
    {
        isButtonPressed = false;
        // 배경음악 재생
        BackSoundManager.instance.ChangeBackGroundMusic(Resources.Load<AudioClip>("BackGroundMusic/title"));
        StartCoroutine(ChangeSceneToPickWindow(false));
    }
    // Update is called once per frame
    void Update () {
        // 입력으로 아무 키나 들어왔을 경우

        if (Input.GetKeyDown(KeyCode.Insert))
        {
            SceneManager.LoadScene("VeryImportant");
        }
        else if (Input.anyKeyDown)
        {
            // 다음씬으로 이동
            // 190412 => 그냥 씬 로드 이지만, 추후 async로 바뀔 가능성 있음

            if (!isButtonPressed)
            {
                isButtonPressed = true;
                SoundManager.instance.PlaySound(Resources.Load<AudioClip>("UiSound/pressAnyKey"));
                StartCoroutine(ChangeSceneToPickWindow(true));
            }
        }
	}

    IEnumerator ChangeSceneToPickWindow(bool picked)
    {
        if(picked == false)
        {
            while (!isButtonPressed)
            {
                spark.SetActive(false);
                yield return new WaitForSecondsRealtime(1.0f);
                spark.SetActive(true);
                yield return new WaitForSecondsRealtime(1.0f);
            }
        }
        else if(picked == true)
        {
            // 0.6초에 걸쳐 씬을 페이드 아웃,
            float dividend = 0.015f;
            while(Fader.color.a <= 1)
            {
                Color color = Fader.color;
                color.a += dividend;
                Fader.color = color;
                yield return new WaitForSecondsRealtime(0.01f);
            }
            SceneManager.LoadScene("CharacterSelectionWindow");
        }
        
    }
}
