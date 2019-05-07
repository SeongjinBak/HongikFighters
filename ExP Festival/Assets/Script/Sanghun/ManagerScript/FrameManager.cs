/*
 * 작성자 : 정상훈
 * Player스크립트가 캐릭터의 입력에 해당했다면, FrameManager는 두 캐릭터 사이의 상호작용을 담당한다
 * Update에서 매 프레임마다 1프레임씩 세어준다
 * 한 캐릭터가 스킬을 쓸 때, 다른캐릭터가 맞았는지 판정하고, 맞았다면 맞은 플레이어의 함수를 실행해준다.
 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FrameManager : MonoBehaviour {

    public static int nowFrame;
    public int endRound;
    public Player playerOne;
    public Player playerTwo;
    Player[] players;
    int[] roundScore;
    public Vector2[] playerPos;
    public SkillJson[] activatedSkill;  //현재 각 플레이어가 발동중인 스킬
    public bool counter;
    bool activateBool;     //스킬 발동할지 말지
    public bool[] skillHitted;   //스킬이 실제로 맞았는지. 맞았으면 건너편에 전달해줘야한다. 0은 플레이어1, 1은 플레이어 2
                                 //스킬이 맞은놈의 인덱스가 켜지는 것이다!!!
    bool[] followingDamageBool;  // 후속타 맞았는지
    bool folloingDamageCorRunning = false;
        
    //싱글톤은 사용하지 않는다. 플레이 화면에서만 프레임매니저가 필요하다.
    //플레이 화면에서 다른 플레이화면으로 씬이 넘어가지 않는다고 가정한다.
    //씬이 넘어갈 때 플레이어의 정보를 기록할 필요가 없다.
    Vector2 gizmoHitbox;
    Vector2 gizmoRange;
    public Text[] hpText;       //디버그용
    public Text debugText;      //디버그용
    public Text[] ultText;      //디버그용
    public Text[] scoreText;    //디버그용
    bool roundOver;         //한 라운드가 끝났는지   끝났으면 true
    bool gameOver;          //한 게임이 끝났는지     끝났으면 true
    bool stopUpdating;        //Update에서 프레임 세는걸 중지하기
    bool[] ready;       //양 플레이어가 시작 모션을 모두 끝냈는지 확인. Player에서 Restart로 값 바꿈
    int[] leftActiveFrame;
    int one;
    float timeScale;
    float timeCount;
    public float endZoomSize;       //게임끝날때 줌하는거
    public float playZoomSize;      //게임중에 줌하는거
    int[] damageDelayFrame;         //각 플레이어가 동시타 할 수도 있으니까, 동시에 궁 켜지게 해야된다.
    
    // 타이머 오브젝트 저장
    public GameObject timer;
    // TimeLimited 조정을 위한 플래그 변수
    private bool timeFlag;
    // 게임 오버 판단
    private bool isGameOver;
  

    // Use this for initialization
    void Start () {
        // 프레임매니저 초기 호출시에 모든 입력 금지.
        GameManager.instance.forbidEveryInput = true;
        // 초기화
        // 게임 오버 false
        isGameOver = false;

        damageDelayFrame = new int[2];  //..
        followingDamageBool = new bool[2] { false, false };
        stopUpdating = true;   //이걸 해줘야 업데이트가 통제된다.
         //테스트용으로 캐릭터픽이 완료된 상태면 저장된 동아리 이름으로 이동,
        if(GameManager.instance.player1_name != null)
            SceneStart(GameManager.instance.player1_name, GameManager.instance.player2_name);
        // 아니면 카우보이 네페르로 게임 시작
        else
        {
            SceneStart();
        }
        //이거도 나중에 전체 총괄 매니저에서 해줄것.

        timeFlag = false;
	}

    //게임의 최초시작단계
    public void SceneStart(string name1 = "Cowboys", string name2 = "Nefer")
    {
        timeScale = 1;
        Time.timeScale = 1;
        playerOne = GameObject.Find("Player1").GetComponent<Player>();
        playerTwo = GameObject.Find("Player2").GetComponent<Player>();
        //원래는 퍼블릭으로 플레이어를 넣어줬었는데, 혹시몰라서 해놓은다.
        playerOne.playerName = name1;
        playerTwo.playerName = name2;
        //지금 디버그해야돼서 동훈이가 쓸 동안만 이렇게 해놓읍시다
        playerOne.playerNum = 1;
        playerTwo.playerNum = 2;
        players = new Player[2] { playerOne, playerTwo };
        playerPos = new Vector2[2];
        roundScore = new int[2] { 0, 0 };
        gameOver = false;
        RoundStart();
    }

    

    void RoundStart()
    {
        timeFlag = false;
        roundOver = false;
        nowFrame = 0;
        timeScale = 1;
        Time.timeScale = timeScale;
        timeCount = 0;
        activatedSkill = new SkillJson[2];
        skillHitted = new bool[2] { false, false }; 
        // 스킬을 맞았는지 여부를 판단하는 변수, [0]은 playerOne이 맞음, [1]은 playerTwo가 맞음.
        leftActiveFrame = new int[2] { 0, 0 };
        ready = new bool[2] { false, false };
        stopUpdating = false;


        // 라운드 시작시 시작위치 고정(gamemanager) 일단 디버깅을 위해 주석처리 후 하드코딩
        Debug.Log("초기 시작위치 맵별로 조정하는거 필요함!");
        //players[0].transform.position = GameManager.instance.startPosition[0,0];
        //players[1].transform.position = GameManager.instance.startPosition[0,1];
        players[0].transform.position = new Vector2(-50f, 4.09f);
        players[1].transform.position = new Vector2(38.8f, 4.09f);


        players[0].OnGameStart();
        players[1].OnGameStart();

        // 라운드 시작시 타이머 호출
        timer.GetComponent<Timer>().CallTimer();

        //여기서 게임이 시작된다.
    }

    //Player에서 승리, 패배모션이 끝나면 Restart를 호출함.
    public void Restart(int readyPlayer)
    {
        ready[readyPlayer] = true;
        if(ready[0] && ready[1])
        {
            //양 쪽 모두의 호출을 받고, 둘다 승점이 3점이 아니어서 gameOver가 false인 경우
            /*if (gameOver)
            {
                //승리 패배모션 끝나고나서 프레임 정지한다.
                stopUpdating = true;
            }*/
            stopUpdating = gameOver;        //위와 의미가 같음.
            if(!isGameOver)
                StartCoroutine(RestartWaiter());
        }
        
    }
    IEnumerator RestartWaiter()
    {
        
        yield return new WaitForSeconds(1.3f);
        RoundStart();
    }

    // Update is called once per frame
    void Update () {
//        Debug.Log("FPS : " + 1 / Time.deltaTime);
        if (stopUpdating)
        {
            return;
        }
        timeCount++;
        if (timeCount >= 1 / timeScale)
        {
            timeCount = 0;
            nowFrame += 1;
        }
        else
        {
            return;
        }

        playerOne.OnEveryFrame();
        playerTwo.OnEveryFrame();
        if (!roundOver)
        {
            CameraPosChange();
        }
        if (activateBool)
        {
            SkillActivate();
            // HpJudging();
            // 기존의 Hpjudging을 timeLimited 안에 넣었음. 시간이 0초 이상 남았을 경우 그냥 HPjudging이 실행됨.
            TimeLimited();
        }
        TimeLimited();
    }

    void SlowMotionStart()
    {
        timeScale = 0.5f;
        Time.timeScale = timeScale;
        timeCount = 0;
   
    }

    void CameraPosChange()
    {
        Vector2 destination;
        float size = Mathf.Abs(players[0].transform.position.x - players[1].transform.position.x) / 2f;
        destination = (players[0].transform.position + players[1].transform.position) / 2;
        if(size <= playZoomSize)
        {
            size = playZoomSize;
        }
        Camera.main.transform.position = new Vector3(destination.x,destination.y, -10);  //z축 고정
        Camera.main.orthographicSize = size;
    }

    void CameraZoomStart()
    {
        timer.GetComponent<Timer>().StopTimer();
        StartCoroutine(CameraZoom());
    }

    

    IEnumerator CameraZoom()
    {
        roundOver = true;
        Vector2 cameraPos;
        Vector2 destination;
        float lerpT;
        float camSize;
        float camSizeDest;
        Camera mainCam;

        mainCam = Camera.main;
        cameraPos = mainCam.transform.position;
        destination = (players[0].transform.position + players[1].transform.position) / 2;

        camSize = mainCam.orthographicSize;
        lerpT = 0;
        camSizeDest = endZoomSize;
        while (lerpT <= 1)
        {
            destination = (players[0].transform.position + players[1].transform.position) / 2;
            Vector2 lerp = Vector2.Lerp(cameraPos, destination, lerpT);
            mainCam.transform.position = new Vector3(lerp.x, lerp.y, -10);  //z축 고정
            mainCam.orthographicSize = Mathf.Lerp(camSize, camSizeDest, lerpT);
            lerpT += Time.deltaTime * 2;
            yield return null;
        }//줌했다가

        lerpT = 0;
        camSizeDest = camSize;
        camSize = endZoomSize;
        cameraPos = mainCam.transform.position;
        while (lerpT <= 1)
        {
            destination = (players[0].transform.position + players[1].transform.position) / 2;
            Vector2 lerp = Vector2.Lerp(cameraPos, destination, lerpT);
            mainCam.transform.position = new Vector3(lerp.x, lerp.y, -10);  //z축 고정
            lerpT += Time.deltaTime;
            yield return null;
        }
        lerpT = 0;
        camSizeDest = camSize;
        camSize = 8;
        while (lerpT <= 1)
        {
            mainCam.orthographicSize = Mathf.Lerp(camSize, camSizeDest, lerpT);
            lerpT += Time.deltaTime * 2;
            yield return null;
        }//다시 돌아오기
        //여기서 GameStart()가 실행되지 않고, 양쪽 Player에서 GameOver모션이 끝나면 GameStart가 실행된다.
      

    }

    //각 플레이어로부터 스킬 신청을 받고, 그 스킬이 상대에게 맞았는지 안맞았는지 판별해준다.
    //각 플레이어의 OnEveryFrame->DelegateSkill 에서 ActiveFrame일 때 함수를 실행해준다.
    public void SkillRequest(int playerNum, SkillJson skill,int indexOrder, bool watchingRight)
    {
        //playerNum         플레이어 1인지 2인지, 값도 1 2로 줘야한다
        //skill             어느 스킬인지 스킬 정보
        //indexOrder        activateFrame중에서 몇번째 프레임인지. indexOrder / (activateframe-1)이 식이다.
        //watchingRight     오른쪽 보고잇는지 왼쪽보고있는지 판별해준다

        /*왜 activeFrame-1로 나누냐면, 3프레임 발동일 때, 시작지점에서 한번, 중간지점에서 한번,
         * 끝나는 지점에서 한 번 발동해야 한다. 0/3, 1/3. 2/3을 하면 안되니까, 0/2, 1/2, 2/2 지점이다
         * 그래서 액티브가 1이면, 0/0꼴이 되니 예외처리 해주는것이다.
         */

        playerPos[0] = players[0].transform.position;
        playerPos[1] = players[1].transform.position;

        int userNum = playerNum-1;                     //스킬을 발동하는 플레이어의 값
        int oponentNum = Mathf.Abs(userNum-1);         //스킬을 맞는 플레이어의 값
        float frameIndex;     //현재 발동프레임 중 몇번쨰 프레임에 위치하는지
       
        one = 1;        //방향에 따라 고쳐줄꺼다.
        if (watchingRight == false)
        {
            one = -1;
        }
        if (skill.activeFrame == 1)
        {
            frameIndex = 0;
        }
        else
        {
            frameIndex = (float)indexOrder / (skill.activeFrame);
        }
        //오른쪽을 보고있을 때는 x가 커지는게 앞, 왼쪽을 보고있을 때는 x가 감소하는게 앞이다.
        //오른쪽보면 one이 1, 왼쪽보면 one이 -1
        /*
         * 즉시이동이어서 빼도록한다. 이동은 delegateSkill에서 해준다
        Vector2 moveDistance = new Vector2(playerPos[userNum].x + one * frameIndex * skill.xMoveDistance,
            playerPos[userNum].y +frameIndex * skill.yMoveDistance);

        players[userNum].transform.position = moveDistance;

        */


        Vector2 hitbox = new Vector2(playerPos[userNum].x + one*(skill.xHitbox + frameIndex * skill.xHitboxMove),
           playerPos[userNum].y + skill.yHitbox + frameIndex * skill.yHitboxMove);    //현재 히트박스 위치 잡아준다.
        Vector2 range = new Vector2(skill.xRange/2f, skill.yRange/2f);    //히트범위 잡아준다.

        players[userNum].skillEffectObj.transform.position = hitbox;
        players[userNum].skillEffectObj.transform.localScale = range * 0.3f;

        gizmoHitbox = hitbox;
        gizmoRange = range * 2;

        Vector2 distance = hitbox - playerPos[oponentNum];  //히트박스에서 상대까지의 거리.

        if (players[oponentNum].invulnerable)
        {
            return; //무적이면 뭐 할필요도없다
        }

        if (Mathf.Abs(distance.x) <= range.x && Mathf.Abs(distance.y) <= range.y)
        {
            //거리 안쪽이라면
            skillHitted[oponentNum] = true;
            activateBool = true;
            //이제 skillActivate에서 이걸 처리해준다.
            //여기서 함수 실행하면 동시타 처리가 안되기 때문에 update에서 해준다.
        }
        activatedSkill[userNum] = skill;
        leftActiveFrame[userNum] = skill.activeFrame - indexOrder; 
        //발동프레임이 몇프렘 남았는지 알려줘야 후딜 계산이 된다.

    }

    public void SkillActivate()
    {
        //int userNum;
        //int oponentNum;
        float damagedHP;
        bool skillGuarded = false;      //가드시에 true
                                        //상대 궁모션을 킬지, 상대의 스킬 히트모션을 킬지에 중요하다
        
        if (skillHitted[0])
        {
            //플레이어 1이 맞았는지. == 플레이어 2가 시전했는지
            damagedHP = players[0].SkillBeatenStart(activatedSkill[1], leftActiveFrame[1], one,players[1].buffRatio,out skillGuarded);
            if (!skillGuarded)
            {
                players[1].SkillHittedEffect(); //시전자한테 있다.
                //시전자한테 스킬 이펙트가 있어서, 시전자가 해야된다.
            }
            else
            {
                //가드이펙트 들어갈 자리. 이건 시전자가 아니라 맞은사람이 해야겠지.
            }
            players[1].UltPointIncrease(damagedHP); 
            if (activatedSkill[1].command.Contains("u") && !skillGuarded)
            {
                players[1].UltHItted();     //이건 시전자
                if (activatedSkill[1].followingDamage != -1)
                {
                    //만약에 2타성 기술이라면, damage가 언제 들어가야하는지 체크하고, 데미지를 들어가게한다
                    //bool을 켜주면, 코루틴이 작동한다.
                    damageDelayFrame[0] = nowFrame + activatedSkill[1].damageDelayFrame;
                    followingDamageBool[0] = true;
                    //0이 쳐맞은거니까
                }
            }
            one = -1 * one; //동시타 고려
        }
        if (skillHitted[1])
        {
            //플레이어 2가 맞았는지. == 플레이어 1이 시전했는지
            damagedHP = players[1].SkillBeatenStart(activatedSkill[0], leftActiveFrame[0], one, players[0].buffRatio, out skillGuarded);
            if (!skillGuarded)
            {
                players[0].SkillHittedEffect();
            }
            else
            {

            }
            players[0].UltPointIncrease(damagedHP);
            if (activatedSkill[0].command.Contains("u") && !skillGuarded)
            {
                players[0].UltHItted();
                if (activatedSkill[0].followingDamage != -1)
                {
                    //위와 주석 동일, 시전자는 플레이어 1. 맞는자가 플레이어 2
                    //이건 맞는놈 기준
                    damageDelayFrame[1] = nowFrame + activatedSkill[0].damageDelayFrame;
                    followingDamageBool[1] = true;
                }
            }
        }
        if(followingDamageBool[0] || followingDamageBool[1])
        {
            if(folloingDamageCorRunning == false)
            {
                StartCoroutine(SkillFollowingDamageCor());
            }
        }
        skillHitted[0] = false;
        skillHitted[1] = false;
        activateBool = false;
    }

    IEnumerator SkillFollowingDamageCor()
    {
        int waitingFrame;
        float damagedHP;
        bool skillGuarded = false;      //가드시에 true
        float damageSave;
        folloingDamageCorRunning = true;
        one = -1 * one;         //아까 hitted할 때 이거 바꿔줬잖아.
        //사실 여기서 가드는 필요없다.
        //동시타일 때 언제 후속타를 때려야 하나.
        if (followingDamageBool[0])
        {
            if (followingDamageBool[1])
            {
                //동시타일 때.
                if(damageDelayFrame[0] > damageDelayFrame[1])
                {
                    waitingFrame = damageDelayFrame[0];
                }
                else
                {
                    waitingFrame = damageDelayFrame[1];
                }
            }
            else
            {
                waitingFrame = damageDelayFrame[0];
            }
        }
        else
        {
            waitingFrame = damageDelayFrame[1];
        }
        while(nowFrame < waitingFrame)
        {
            yield return null;
        }
        //상대 궁모션을 킬지, 상대의 스킬 히트모션을 킬지에 중요하다
        if (followingDamageBool[0])
        {
            //후속타에는 무조건 fallDown으로 들어가야 한다.
            //사실은 dumy를 만들고 하고싶은데, 깊은복사가 안돼서 하나하나 해야한다.
            //기획상으로 1타는 넉백이 없고, fallDown이 아니다
            //2타는 넉백이 있고, fallDown으로 끝난다. 현재는 2타를 구현하는 것이다.
            damageSave = activatedSkill[1].damage;
            activatedSkill[1].damage = activatedSkill[1].followingDamage;
            activatedSkill[1].followingDamage = -1;     //이래야 skillBeatenStart에서 일반타수로 본다.
            activatedSkill[1].fallDown = true;

            damagedHP = players[0].SkillBeatenStart(activatedSkill[1], leftActiveFrame[1], one, players[1].buffRatio, out skillGuarded);

            activatedSkill[1].followingDamage = activatedSkill[1].damage;
            activatedSkill[1].damage = damageSave;
            activatedSkill[1].fallDown = false;
            players[1].UltPointIncrease(damagedHP);

            one = -1 * one; //동시타 고려
        }
        if (followingDamageBool[1])
        {
            damageSave = activatedSkill[0].damage;
            activatedSkill[0].damage = activatedSkill[0].followingDamage;
            activatedSkill[0].followingDamage = -1;     //이래야 skillBeatenStart에서 일반타수로 본다.
            activatedSkill[0].fallDown = true;

            damagedHP = players[1].SkillBeatenStart(activatedSkill[0], leftActiveFrame[0], one, players[0].buffRatio, out skillGuarded);

            activatedSkill[0].followingDamage = activatedSkill[0].damage;
            activatedSkill[0].damage = damageSave;
            activatedSkill[0].fallDown = false;
        
            players[0].UltPointIncrease(damagedHP);
        }
        followingDamageBool[0] = false;
        followingDamageBool[1] = false;
        activateBool = false;
        folloingDamageCorRunning = false;

        // HpJudging();
        // 기존의 Hpjudging을 timeLimited 안에 넣었음. 시간이 0초 이상 남았을 경우 그냥 HPjudging이 실행됨.
        TimeLimited();
    }

    // 60초 시간제한 확인하고 승리/패배모션 출력
    public void TimeLimited()
    {
        if (timeFlag)
            return;

        // 60초 모두 흘렀다면
        if (timer.GetComponent<Timer>().TimeEnd())
        {
            // 입력 정지
            GameManager.instance.forbidEveryInput = true;

            // timeflag를 true로 함으로써 중복호출 방지
            timeFlag = true;
            // 1p hp > 2p hp
            if(players[0].nowHealthPoint > players[1].nowHealthPoint)
            {
                SlowMotionStart();
                debugText.text = "1이 이김";
                players[0].GameOverStart(true);
                players[1].GameOverStart(false);
                roundScore[0]++;
                CameraZoomStart();
            }
            else if (players[0].nowHealthPoint < players[1].nowHealthPoint)
            {
                SlowMotionStart();
                debugText.text = "2가 이김";
                players[0].GameOverStart(false);
                players[1].GameOverStart(true);
                roundScore[1]++;
                CameraZoomStart();
            }
            // 1p hp == 2p hp
            else
            {
                Debug.Log("꿍극기 계수 판단");
                // 1p ult > = 2p ult(궁 포인트가 1이 더 높거나 1 == 2 인 경우)
                if(players[0].nowUltPoint >= players[1].nowUltPoint)
                {
                    SlowMotionStart();
                    debugText.text = "1이 이김";
                    players[0].GameOverStart(true);
                    players[1].GameOverStart(false);
                    roundScore[0]++;
                    CameraZoomStart();
                }
                else if (players[0].nowUltPoint < players[1].nowUltPoint)
                {
                    SlowMotionStart();
                    debugText.text = "2가 이김";
                    players[0].GameOverStart(false);
                    players[1].GameOverStart(true);
                    roundScore[1]++;
                    CameraZoomStart();
                }
            }
        }
        // 60초 모두 흐르지 않았다면
        else
        {
            
            HpJudging();
        }
    }
    //Hp가 0 아래로 떨어졌는지 판정. Hp바 변화
    public void HpJudging()
    {
        bool died1 = false;
        bool died2 = false;
        if (players[0].nowHealthPoint <= 0)
        {
            died1 = true;
            timeFlag = true;
        }
        if (players[1].nowHealthPoint <= 0)
        {
            died2 = true;
            timeFlag = true;
        }



        if (died1 && died2)
        {

            // 입력 정지
            GameManager.instance.forbidEveryInput = true;

            SlowMotionStart();
            //double KO
            debugText.text = "더블ko";
            players[0].GameOverStart(false);
            players[1].GameOverStart(false);
            roundScore[0]++;
            roundScore[1]++;

            CameraZoomStart();
        }
        else
        {


            if (died1)
            {// 입력 정지
                GameManager.instance.forbidEveryInput = true;
                SlowMotionStart();
                debugText.text = "2가 이김";
                players[0].GameOverStart(false);
                players[1].GameOverStart(true);
                roundScore[1]++;
                CameraZoomStart();
            }
            else if (died2)
            {// 입력 정지
                GameManager.instance.forbidEveryInput = true;
                SlowMotionStart();
                debugText.text = "1이 이김";
                players[0].GameOverStart(true);
                players[1].GameOverStart(false);
                roundScore[0]++;
                CameraZoomStart();
            }
        }
        foreach (int i in roundScore)
        {
            if (i == endRound)
            {

                gameOver = true;
                debugText.text = "게임오버!";

                // 게임 오버시 키 입력 중단시키고, 다음 라운드로 못가게 해야 함. 
                // gameover활성화
                isGameOver = true;
                GameManager.instance.forbidEveryInput = true;
                // 다음씬
                StartCoroutine(GameOverWaiter());

            }
        }

        hpText[0].text = players[0].nowHealthPoint.ToString("N1");
        hpText[1].text = players[1].nowHealthPoint.ToString("N1");
        ultText[0].text = players[0].nowUltPoint.ToString("N1");
        ultText[1].text = players[1].nowUltPoint.ToString("N1");
        scoreText[0].text = roundScore[0].ToString();
        scoreText[1].text = roundScore[1].ToString();
    }

    IEnumerator GameOverWaiter()
    {
        yield return new WaitForSeconds(1.3f);
        SceneManager.LoadScene("GameOverScene");
    }
    
    //라운드 5가 끝나고 씬 전환을 넣어주면 될듯.
    void GameOver()
    {
        //메인화면으로 가기, 혹은 Player가 하지 않는 연출.
       
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(gizmoHitbox, gizmoRange);
    }

}
