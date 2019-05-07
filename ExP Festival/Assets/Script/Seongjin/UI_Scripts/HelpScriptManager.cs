﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpScriptManager : MonoBehaviour {

    // 뒤의 맵 배경
    public SpriteRenderer backGroundSr;
    // 1p, 2p 도움말
    public SpriteRenderer p1_Sr, p2_Sr;

    private bool isButtonPressed = false;
	// Use this for initialization
	void Start () {
        
        backGroundSr.sprite = Resources.Load<Sprite>("Help_test/" + GameManager.instance.mapName);
        p1_Sr.sprite = Resources.Load<Sprite>("MotionSprite/" + GameManager.instance.player1_name + "/c/" + GameManager.instance.player1_name + "_c_01-removebg");
        p2_Sr.sprite = Resources.Load<Sprite>("MotionSprite/" + GameManager.instance.player2_name + "/c/" + GameManager.instance.player2_name + "_c_01-removebg");
        StartCoroutine(PressAnyBtn());
    }
	IEnumerator PressAnyBtn()
    {
        yield return new WaitForSeconds(1f);
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
                    SceneManager.LoadScene("test");
                }
            }
        }

    }
	
}
