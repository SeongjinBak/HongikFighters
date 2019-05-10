using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameOverManager : MonoBehaviour {

   // private Image winnerImage;
    private Text winnerText;

	// Use this for initialization
	void Start () {
        winnerText = GameObject.Find("Text").GetComponent<Text>();
        //  winnerImage = GameObject.Find("Image").GetComponent<Image>();
        StartCoroutine(ShowWinner());
	}
	
    IEnumerator ShowWinner()
    {
        string winner = GameManager.instance.winner;

        winnerText.text = winner + " win!";
        /*  
            if(winner == "Player 1")
            {
                // 이미지 끌어옹기
            }
            else
            {

            }
      */
        yield return new WaitForSecondsRealtime(4f);
        ChangeToMainScene();
    }

    void ChangeToMainScene()
    {
        GameManager.instance.ResetGameData();
        SceneManager.LoadScene("MainStart");
    }
	
}
