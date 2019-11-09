/*
 * 게임 시작 전, 키 설명 등이 이루어지는 도움말 창
 */ 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HelpScriptManager : MonoBehaviour {

    // 뒤의 맵 배경
    public Image backGroundSr;
    // 1p, 2p 도움말
    public Image p1_script_sr, p2_script_sr;
    // 1p, 2p Idle 상태 이미지 계속 출력.
    public Image p1_sr, p2_sr;
    private Sprite[] p1_idle_sprites, p2_idle_sprites;
    public GameObject pressAnyBtnImage;
    // 페이드 아웃용도의 검정 화면
    public Image Fader;
    private bool isButtonPressed = false;
	
    // Use this for initialization
	void Start () {
        // 배경이미지 불러온다.
        backGroundSr.sprite = Resources.Load<Sprite>("BackGroundMap/" + GameManager.instance.mapName);
        // 도움말 스프라이트 렌더러에 동아리에 맞게 이미지 출력
        p1_script_sr.sprite = Resources.Load<Sprite>("HelpWindow/" + GameManager.instance.player1_name + "-1P");
        p2_script_sr.sprite = Resources.Load<Sprite>("HelpWindow/" + GameManager.instance.player2_name + "-2P");
        // idle 애니메이션 출력을 위해 알맞은 sprite 저장.
        p1_idle_sprites = Resources.LoadAll<Sprite>("MotionSprite/" + GameManager.instance.player1_name + "/Idle");
        p2_idle_sprites = Resources.LoadAll<Sprite>("MotionSprite/" + GameManager.instance.player2_name + "/Idle");
    

        pressAnyBtnImage.SetActive(false);
        StartCoroutine(PressAnyBtn());
        StartCoroutine(PlayIdleAnim());
    }

    // 선택된 동아리들의 Idle Animation 재생.
    IEnumerator PlayIdleAnim()
    {
        int index = 0;

        while (true)
        {
            index = index == 0 ? 1 : 0; 
            p1_sr.sprite = p1_idle_sprites[index];
            p2_sr.sprite = p2_idle_sprites[index];
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    // Press Any Button 텍스트를 주기적으로 점멸한다.
    IEnumerator Spark()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);
            pressAnyBtnImage.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
            pressAnyBtnImage.SetActive(false);
        }
    }

    // 도움말 출력 후 Fade out.
    IEnumerator FadeOut()
    {
        // 0.6초에 걸쳐 씬을 페이드 아웃,
        float dividend = 0.01f;
        while (Fader.color.a <= 1)
        {
            Color color = Fader.color;
            color.a += dividend;
            Fader.color = color;
            yield return new WaitForSecondsRealtime(0.01f);
        }
        // 다음 씬으로 이동
        LoadNextScene();
    }

    // 다음씬으로 이동.
    void LoadNextScene()
    {
        SceneManager.LoadScene("test");
    }

    // 입력이 있는지 판단하는 함수.
	IEnumerator PressAnyBtn()
    {
        StartCoroutine(Spark());
        while (true)
        {
            yield return null;
            // 입력으로 아무 키나 들어왔을 경우
            if (Input.anyKeyDown)
            {
                // 다음씬으로 이동
                if (!isButtonPressed)
                {
                    isButtonPressed = true;
                    // 사운드 재생
                    SoundManager.instance.PlaySound(Resources.Load<AudioClip>("UiSound/pressAnyKey"));
                    StartCoroutine(FadeOut());
                }
            }
        }

    }
	
}
