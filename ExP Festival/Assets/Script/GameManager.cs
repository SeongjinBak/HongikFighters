
/*
 * 작성자 : 백성진
 * 게임매니저 클래스 입니다!
 * 
 */ 




using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour {

    public static GameManager instance = null;
    public Vector2[,] startPosition = new Vector2[4,2];
    public string player1_name;
    public string player2_name;
    public string mapName;
    // 게임 종료 후 승리자를 저장.
    public string winner;
    // 버튼 입력 가능 여부를GM에서 관리함.
    public bool forbidEveryInput;


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
    // Use this for initialization
    void Start () {
        
        mapName = "";
        // 0번쨰(테스트맵)에서의 시작위치 조정.
        startPosition[0, 0] = new Vector2(-50f, 4.09f);
        startPosition[0, 1] = new Vector2(38.8f, 4.09f);

        // 이 변수가 참이면, false될때까지 어떠한 버튼도 입력하지 못하게함.
        forbidEveryInput = true;
    }

    // 게임종료시, 이전 게임의 데이터는 모두 초기화한다. 위치는 맵 전부 공통으로 초기화 되므로 리셋하지 않음.
    public void ResetGameData()
    {
        mapName = "";
        winner = "";
        player1_name = "";
        player2_name = "";
        forbidEveryInput = true;
    }
}
