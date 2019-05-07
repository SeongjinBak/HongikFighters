/*
 * 작성자 : 백성진
 * 메인 스타트 화면에서 "아무 키나 눌렸을 때" 다음 씬으로 전환 한다.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class MainStart : MonoBehaviour {
    bool isButtonPressed;
    public GameObject spark;

    private void Start()
    {
        isButtonPressed = false;
    }
    // Update is called once per frame
    void Update () {
        // 입력으로 아무 키나 들어왔을 경우
        if (Input.anyKeyDown)
        {
            // 다음씬으로 이동
            // 190412 => 그냥 씬 로드 이지만, 추후 async로 바뀔 가능성 있음
            if (!isButtonPressed)
            {
                isButtonPressed = true;
                StartCoroutine(ChangeSceneToPickWindow());
            }
        }
	}

    IEnumerator ChangeSceneToPickWindow()
    {
        // 깜빡이는 효과 넣을 부분임.
        yield return new WaitForSecondsRealtime(3f);
        float t = 0.0f;
        while(t < 2.5f){
            t += 0.25f;
            spark.SetActive(false);
            yield return new WaitForSecondsRealtime(0.25f);
            spark.SetActive(true);
            t += 0.25f;
            yield return new WaitForSecondsRealtime(0.25f);
        }

        spark.SetActive(false);
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene("CharacterSelectionWindow");
    }
}
