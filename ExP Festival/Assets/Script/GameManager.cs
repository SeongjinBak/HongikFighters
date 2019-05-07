
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
	
	// Update is called once per frame
	void Update () {
		
	}
}
