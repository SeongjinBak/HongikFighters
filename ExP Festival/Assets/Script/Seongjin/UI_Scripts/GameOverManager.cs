/*
 * 게임 오버 되었을 때, (검정 화면) 승리팀의 텍스트를 띄운다.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameOverManager : MonoBehaviour {

   // private Image winnerImage;
    private Text winnerText;
    // 누가 게임 승리자인지 나타내는 이미지
    public Image[] winner_image;

	// Use this for initialization
	void Start () {
        winnerText = GameObject.Find("Text").GetComponent<Text>();
        StartCoroutine(ShowWinner());

        // 배경음악 정지
        BackSoundManager.instance.StopBGM();
    }
	
    // 승자를 출력하는 코루틴.
    IEnumerator ShowWinner()
    {
        string winner = GameManager.instance.winner;

        if (winner == "Player 1")
        {
            winner_image[0].enabled = true;
        }
        else
        {
            winner_image[1].enabled = true;
        }
        yield return new WaitForSecondsRealtime(4f);
        ChangeToMainScene();
    }

    // 승리팀 출력 끝나면 메인 화면으로 돌아온다.
    void ChangeToMainScene()
    {
        GameManager.instance.ResetGameData();
        SceneManager.LoadScene("MainStart");
    }
	
}
