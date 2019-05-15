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
        //  winnerImage = GameObject.Find("Image").GetComponent<Image>();
        StartCoroutine(ShowWinner());


        // 배경음악 정지
        BackSoundManager.instance.StopBGM();
    }
	
    IEnumerator ShowWinner()
    {
        string winner = GameManager.instance.winner;

        // winnerText.text = winner + " win!";

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

    void ChangeToMainScene()
    {
        GameManager.instance.ResetGameData();
        SceneManager.LoadScene("MainStart");
    }
	
}
