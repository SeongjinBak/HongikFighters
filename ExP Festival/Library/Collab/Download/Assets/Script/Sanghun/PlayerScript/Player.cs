/*
 * 작성자 : 정상훈
 * 플레이어의 기술입력, 이동, 애니메이션 등 플레이어의 입력과 관련된 전반적인 부분을 담당한다
 * FrameManager에서 시간에 해당하는 Frame을 통해 시간을 알아낸다.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//키 인푸터 상속 이유 : 키 인푸터는 UI화면에서 캐릭터 고를때도 쓸것이다.
//그 때도 다른 스크립트 파서 키 인푸터 상속시키면 편하다.
public class Player : KeyInputter {

    //1P와 2P에 상속될 스크립트이다. 오버라이드도 많이 쓸 것이다.
    //기술은 각 스크립트에 또 적을것이다. 기술이 많지 않으니 하드코딩 해도 될 것 같다.
    //기술을 읽을 수 있는 OnEveryFrame()에서는 키 조합을 인식한다.
    //키 조합인식은 예를들어) Z가 눌린 후, n프레임 이내에 Z가 두번 더 눌리면 ZZZ으로 인식한다.
    //캔슬은 일단 제외한다. ZXZXZ가 n프레임 이내에 눌렸어도 ZZZ으로 인식한다는 뜻이다.


    /* 모션함수 정리 */
    /*각 모션 함수들은 MotionStart와 Motion으로 되어있다. (ex : MoveStart(), Move())
     * 이 중 MotionStart함수는 초기 설정값을 잡아주고, Motion함수는 delegate 형태로 Update에서 호출된다
     * 이렇게 함으로써 Update에서는 플레이어 하나당 한 모션만 실행하게 된다.
     * 여러 모션이 함께 실행되면 아비규환이다. delegate(callEveryFrame)에는 한 번에 한 함수만 들어가기 때문에
     * 이를 대비할 수 있다. 또한, 타격 피격 강제이행을 고려한 프로그래밍 이다.
     * Motion함수에서는 Frame을 체크하여 sprite를 바꿔준다.
     * 현재는 타격, 피격이 없기 떄문에, 타격 피격 강제 이행에 문제가 있어보인다. 특히, jump에 문제가 있을 것 같다.
     * (19.04.17 이전에 작성)
     */

    
    public string playerName;    //로드에 쓰려고. 플레이어 넘버는 키 인푸터에 있다.
    public float healthPoint;       //최초 hp
    public float nowHealthPoint;    //현재 hp
    public float moveSpeed; //이속
    //public float startJumpVelocity;     //rigidbody를 써서 안씀.
    public float jumpAccelaration;      //점프의 가속도.
   public float xDashForce;
    public float yDashForce;
    public float xJumpDashForce = 13;
    public float yJumpDashForce = 0;
    float ultPoint;                     //최대 궁극기 게이지
    public float nowUltPoint;                  //현재 궁극기 게이지

    protected SpriteRenderer spriteRenderer;    //플레이어꺼
    protected int hp;       //체력
    protected bool jumping; //점프중인지
    public Collider2D groundStandingCollider;   // 땅에 서있을 수 있게 플레이어의 발바닥에 부착된 콜라이더
    bool liftedFallDown;    //스킬중에 위로 뜨면서 FallDown이 있다. 위로 떠있는지.
    public bool invulnerable; //무적상태인지
    int invulnerableFrame;      //무적 몇프렘지속인지, 상대방의 발동프레임만큼임
    int buffFrame;              //몇프레임까지 버프를 할 지
    public float buffRatio;         //버프 배수
                                    //FrameManager에서 알아냄
    bool buffed;


    protected List<CommandFrame> latestCommand;    //가장 최근에 입력받은 커맨드. ZZ ZZZ을 판단해야 하기 때문.
    public SkillJson[] skillArray;              //스킬들의 총 배열
    public GameObject skillEffectObj;           //히트박스에 무슨 스프라이트 들어갈지.
    SkillJson dumySkill;    //디버그용
    SkillJson oponentSkill; //내가 뭐에 맞고있는건지
    Vector2 skillKnockBack;
    FrameManager frameManager;

    protected State nowState;     //현재 어느상태인지
    protected enum State
    {
        Ready, Wait,Move, Jump, Dash,
        OnSkill,Beaten,FallDown, Guarding, GuardSuccess,
        GameEnd
        //StartDelay, Hitting, EndDelay
            //대기, 이동, 기술시작, 기술활성화, 기술 끝, 타격맞음, 기술 연타중.
            //wait일 때는 입력받자 마자 입력딜레이 계산을 한다. 그 입력딜레이 동안에 입력받아야 z로 인정.
            //그 입력딜레이가 끝나야 리프레시를 해준다. 입력딜레이 간에 입력된 모든 것들은 인정이 된다.
    }

    protected enum Command            //커맨드 입력을 Enum Command와 Frame의 Queue로 할 것이다.                     
    {                       //Queue를 Pop하면서 존재하는 커맨드인지 확인한다면 매우 간단해질 것 같다.
        Ulti, I,O,P, Left,Down,Right//,Up   //1P커맨드 기준으로 작성한다. 이동은 대쉬커맨드가 있어서 써논다.
        //키 인푸터 기준, 0 1 2 3 4 5 6이 이 그대로 번호다.
    }
    protected struct CommandFrame
    {
        //키 하나를 입력했을 때 생성하는 struct.
        public Command com;        //이거는 char[]로 대체 가능하다.
        public char commandChar;   //
        public int frame;          //큐의 앞요소와 얼마나 프레임 차이가 나는지 보아야 한다.
    }

    int dashInputFrame = 20;                 // 키 두번 누르는 사이 간격이에요
    int dashFrameLength = 15;               //대쉬사이 간격이 얼마냐
    int dashFrameCounter = 0;               //대쉬 얼마까지 했는지 알아주는 거.
    CommandFrame latestMoveKey;         //방금 눌렀던 키.
    CommandFrame nowMoveKey;            //지금 막 누른 키
    Queue<CommandFrame> commandQueue;   //커맨드가 누적되는 큐. 혹시모를 동시타를 위해 생성.
    Coroutine commandClearer;           //연속기 누적을 해제하는 코루틴
    Rigidbody2D rigidBody;              //점프에 쓰이는 리지드바디.
    int clearFrame = 0;     //현재 있는 커맨드리스트를 클리어해주는 시점. 연타기때문에 만듬
    int inputStartFrame = 0;//언제부터 연타기를 입력가능한지. 
    bool inputBool = true;  //커맨드 기술을 사용할 수 있는지.
    //int nowSkillIndex = 0;  //현재 스킬 인덱스가 뭔지. 커맨드 호출되면 그때 바꿔준다.
    //매 프레임마다 서로 다른 모션을 가져와야한다
    //모션 업데이트는 spriteIndex, 선딜 발동딜 후딜시간을 커맨드 호출 때 기록하여 진행한다.

    delegate void StateVoid();  
    StateVoid callEveryFrame;   //델리게이트 선언. 매 프레임마다 호출이 되어야 하니까. 그때그때 바꿔줌.
                                //주로 모션이 될듯. 이동, 공격, 피격 등등

        //이동에서 필요한 보간들
    float positionChangeLerp;       //총 후딜시간이 몇프렘인지. lerp에 넣어야되서 float다
    int lerpCounter;                //총 후딜시간중 몇프렘이나 지났는지
    Vector2 basePosition;           //원래있던 포지션은 어디인지
    Vector2 targetPosition;
    int watchingRightOne;           //오른쪽보면 1이다.

    //int baseFrame;         //현재 프레임을 구한다.
    int nowSkillIndex;     //현재 스킬의 인덱스번호
    int startFrame;          //선딜이 끝나는 시점을 구하고
    int activeFrame;         //발동시간을 구하고.
    int endFrame;            //후딜이 끝나는 시점을 구한다.
    int spriteCount;         //스프라이트 몇개있는지 구하고
    int triggerIndex;       //몇번 까지 켜야되는지 구한다
    int oponentSkillDelay;  //상대 스킬이 끝나는 시점이 언제인지 구한다.
    public int nowSpriteIndex = 0;
    int motionCounts = 11;   //모션이 몇개있는지.
    int[] spriteChangeGab; //몇 프레임에 한 번씩 스프라이트를 바꿔야 하는가.
    //0 1 2 3 4 5 6 7 8 9 10 선딜 후딜 Idle Move Jump Hit Falldown Guard Dash 발동(스킬) 빅토리 이다.
    //가독성이 안좋음에도 어레이로 한 이유는 spriteChange에서 번호로 매겨버리기 위해서이다.
    //발동이 뒤에붙은 이유는 늦게만들어졌기 때문이다
    int spriteChangeCounter = 0; //갭에 이르는 상태가 되었다면 스프라이트 교체.
    Sprite[] nowSpriteArr;  //현재 스프라이트 뭔지.

    bool movingRight = true;   // 움직일 떄오른쪽 보고있는지
    bool skillRight = true;     //스킬중에 오른쪽 보고있는지
    int waitingSkillIndex = -1; //대기중인게 없으면 -1. 있으면 그 스킬의 인덱스. 스킬 끝내자마자 피격 맞을때 중요할거같다.
    Sprite[] moveSpriteArr;     //무브에 쓰는 스프라이트
    Sprite[] idleSpriteArr;     //아이들에 쓰는 스프라이트
    Sprite[] jumpSpriteArr;     //점프에 쓰는거
    Sprite[] beatenSpriteArr;   //피격
    Sprite[] fallDownSpriteArr; //넘어지는거
    Sprite[] guardSpriteArr;    //가드하는거
    Sprite[] dashSpriteArr;     //대시
    Sprite[] gameStartSpriteArr;    //시작모션
    Sprite[] victorySpriteArr;      //승리모션
    int nowGabIndex;            //무브랑 아이들 하나로 합치려고

    PlayerSound playerSound;    //직접만든 사운드 객체. spriteChangeGab의 인덱스를 공유함. 자세한건 클래스 내에서 확인.
    new Collider2D collider;        //콜라이더 꺼줘야한다. mono에 같은이름있어서 new 써줬음.
    Collider2D floor;               //바닥이랑 닿을 때 콜라이더 사라지면 큰일이다. 바닥콜라이더를 받는다.
    public int downDashColFrame;     //동훈이가 넣어줘야하는값. 몇프레임동안 꺼줄것인가
    public float downDashForce;     //아래 대쉬 포스
    int downDashWaitFrame;          //콜라이더 다시 켜는거 언제까지 기다려야하는지.
    bool downDashCorRunning;        //이중코루틴 방지
    GameObject collisionPlatform;   //내가 방금까지 밟고있었던 플랫폼이 뭔지. 이거보다 아래로 내려가면 꺼주게 한다.
    public SpriteRenderer hittedEffectRenderer;   //상대한테 달려있는 투사체 오브젝트. 처음에는 비활이다.
    public SpriteRenderer mapEffectRenderer;      //막 어두워지는 이펙트같은거
    bool hittedEffectCorRunning;
    public int fallDownInvulnerableFrame;

    /*밑 키 한번 누르면서 꾹 누르고 있으면 가드

밑 키 두번 빠르게 누르면 2층일때 하단 플랫폼으로 하강*/

    public bool loadSkill;

    private void Start()
    {

        //OnGameStart();  //현재는 여기에 와있지만. 나중에는 게임 시작할 때 프레임매니저에서 실행해줘야함.
    }

    //player.OnGameStart함수에서 실행
    public void jsonLoad()
    {
        //설정 로딩하는거
        PlayerJson load;
        TextAsset text;
        text = Resources.Load<TextAsset>("Json/" + playerName + "/" + playerName);
        string jsonStr = text.ToString();
        load = JsonUtility.FromJson<PlayerJson>(jsonStr);
        skillArray = load.skillJsonArray;
        
        foreach(SkillJson skill in skillArray)
        {
            skill.LoadSkill(skill.command, playerName);
        }

        healthPoint = load.hitPoint;
        nowHealthPoint = load.hitPoint;
        moveSpeed = load.moveSpeed;
        jumpAccelaration = load.jumpForce;
        xDashForce = load.xDashForce;
        yDashForce = load.yDashForce;
        xJumpDashForce = load.xJumpDashForce;
        yJumpDashForce = load.yJumpDashForce;
        dashFrameLength = load.dashFrameLength;
        dashInputFrame = load.dashInputFrame;
        downDashForce = load.downDashForce;

        //int[] spriteChangeGab; //몇 프레임에 한 번씩 스프라이트를 바꿔야 하는가.
        //0         1   2   3     4    5     6      7     8      9        10
        //선딜    후딜 Idle Move Jump Hit Falldown Guard Dash 발동(스킬) 빅토리 이다.
        spriteChangeGab[2] = load.idleFrameGap;
        spriteChangeGab[3] = load.moveFrameGap;
        spriteChangeGab[4] = load.jumpFrameGap;
        spriteChangeGab[5] = load.hittedFrameGap;
        spriteChangeGab[6] = 5;     //하드코딩
        spriteChangeGab[7] = load.guardFrameGap;
        spriteChangeGab[8] = load.dashFrameLength / dashSpriteArr.Length;
        spriteChangeGab[10] = load.victoryFrameGap;
        //spriteChangeGab[7] = load.guardFrameGap;

        //오디오 넣어주기
        AudioSource[] sources = GetComponents<AudioSource>();
        //보이스하고 사운드이펙트 두 개가 필요하다
        playerSound = new PlayerSound();
        playerSound.SoundLoad(playerName, sources[0], sources[1]);
    }

    public void OnGameStart()
    {
        //위치잡기->프레이밍 시작.
        healthPoint = 100;
        nowHealthPoint = 100;
        invulnerable = false;
        buffRatio = 1;
        ultPoint = 100; //임의설정
        collider = GetComponent<Collider2D>();
        floor = GameObject.Find("Floor").GetComponent<Collider2D>();
        //초기화

        skillEffectObj = transform.GetChild(0).gameObject;
        skillEffectObj.SetActive(false);
        latestCommand = new List<CommandFrame>();
        commandQueue = new Queue<CommandFrame>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        frameManager = GameObject.Find("FrameManager").GetComponent<FrameManager>();
        spriteChangeGab = new int[motionCounts];
        string enemyName;
        if(playerNum == 1)
        {
            enemyName = "Player2";
        }
        else
        {
            enemyName = "Player1";
        }
        hittedEffectRenderer = GameObject.Find(enemyName).transform.GetChild(2).GetComponent<SpriteRenderer>();
        //2번이 히티드 이펙트다.
        mapEffectRenderer = GameObject.Find("MapEffect").GetComponent<SpriteRenderer>();
        //playerName = gameObject.name;

        //시작할 때 resource load가 많아서 렉이 걸릴 수 있지만, 실행중에는 절대 없다.
        float time = Time.time;
        Debug.Log("시작시간 : " + time);
        
        moveSpriteArr = Resources.LoadAll<Sprite>("MotionSprite/" + playerName + "/Move");
        idleSpriteArr = Resources.LoadAll<Sprite>("MotionSprite/" + playerName + "/Idle");
        jumpSpriteArr = Resources.LoadAll<Sprite>("MotionSprite/" + playerName + "/Jump");
        beatenSpriteArr = Resources.LoadAll<Sprite>("MotionSprite/" + playerName + "/Hitted");
        fallDownSpriteArr = Resources.LoadAll<Sprite>("MotionSprite/" + playerName + "/FallDown");
        guardSpriteArr = Resources.LoadAll<Sprite>("MotionSprite/" + playerName + "/Guard");
        dashSpriteArr = Resources.LoadAll<Sprite>("MotionSprite/" + playerName + "/Dash");
        gameStartSpriteArr = Resources.LoadAll<Sprite>("MotionSprite/" + playerName + "/GameStart");
        victorySpriteArr = Resources.LoadAll<Sprite>("MotionSprite/" + playerName + "/Victory");
        Debug.Log("소요시간 : " + (time - Time.time));
        KeyRefresh();
        //keyInputter의 함수이다. 플레이어 1인지, 2인지에 따라 키를 바꿔준다.


        //디버깅 때 쓰려고 만든 임시 스킬들
        if (!loadSkill)
        {
            for (int i = 0; i < motionCounts; i++)
            {
                spriteChangeGab[i] = 6;
            }
            skillArray = new SkillJson[7];

            for (int i = 0; i < 4; i++)
            {
                skillArray[i] = new SkillJson();
                skillArray[0].hittedEffect = true;
                char[] arr = new char[i + 1];

                skillArray[i].inputStart = 10;
                skillArray[i].inputEnd = 30;
                for (int j = 0; j < i + 1; j++)
                {
                    arr[j] = 'z';
                }
                skillArray[i].command = new string(arr);
                skillArray[i].LoadSkill(skillArray[i].command, playerName);
                skillArray[i].triggeringIndex = 2;
                skillArray[i].fallDown = false;
            }


            skillArray[5] = new SkillJson();
            skillArray[5].inputStart = 10;
            skillArray[5].inputEnd = 50;
            skillArray[5].startFrame = 12;
            skillArray[5].activeFrame = 12;
            skillArray[5].endIndex = 2;
            skillArray[5].command = "c";
            skillArray[5].LoadSkill(skillArray[5].command, playerName);
            skillArray[5].triggeringIndex = 1;
            skillArray[5].fallDown = true;

            skillArray[4] = new SkillJson();
            skillArray[4].inputStart = 10;
            skillArray[4].inputEnd = 50;
            skillArray[4].command = "x";
            skillArray[4].damage = 1;
            skillArray[4].LoadSkill(skillArray[4].command, playerName);
            skillArray[4].triggeringIndex = 2;
            skillArray[4].fallDown = true;

            skillArray[6] = new SkillJson();
            skillArray[6].inputStart = 10;
            skillArray[6].inputEnd = 50;
            skillArray[6].startFrame = 100;
            skillArray[6].activeFrame = 100;
            skillArray[6].endFrame = 100;
            skillArray[6].endIndex = 9;
            skillArray[6].command = "u";
            skillArray[6].LoadSkill(skillArray[6].command, playerName);
            skillArray[6].triggeringIndex = 4;
            skillArray[6].fallDown = true;


            //오디오 넣어주기
            AudioSource[] sources = GetComponents<AudioSource>();
            //보이스하고 사운드이펙트 두 개가 필요하다
            playerSound = new PlayerSound();
            playerSound.SoundLoad(playerName, sources[0], sources[1]);
        }
        else
        {
            jsonLoad();
        }
       

        //nowState = State.Ready; //시작 전이 ready 이다
        buffed = false;

        StartMotionStart();    //추후에 3 2 1 Start등 준비모션으로 바뀔것이다.
    }

    //한 라운드 시작할 때 FrameManager에서 실행.
    public void OnRoundStart()
    {
        healthPoint = 100;
        nowHealthPoint = 100;
        invulnerable = false;
        jumping = false;

        skillEffectObj.SetActive(false);
        hittedEffectRenderer.sprite = null;

        latestCommand.Clear();
        clearFrame = 0;
        corRunning = false;
        //클리어를 안해주면 스킬을 못쓸 수도 있다.

        buffRatio = 1;

        //nowState = State.Ready; //시작 전이 ready 이다
        buffed = false;

        StartMotionStart();    //추후에 3 2 1 Start등 준비모션으로 바뀔것이다.
    }

    int startMotionEndFrame = 0;
    public void StartMotionStart()
    {
        startMotionEndFrame = FrameManager.nowFrame + spriteChangeGab[7] * gameStartSpriteArr.Length;
        //시작모션은 갭 * 모션 프레임의 갯수 만큼 진행한다.
        nowState = State.Ready;
        spriteChangeCounter = spriteChangeGab[7];
        nowSpriteIndex = 0;
        nowSpriteArr = gameStartSpriteArr;
        nowGabIndex = 7;
        spriteCount = nowSpriteArr.Length;
        playerSound.SoundPlay(0, false);
        callEveryFrame = StartMotion;
    }

    public void StartMotion()
    {
    
        SpriteChanger();
        if (FrameManager.nowFrame >= startMotionEndFrame)
        {
            IdleStart();
        }
    }

    //callEveryFrame이 모션을 불러온다면, OnEveryFrame은 입력을 받고, callEveryFrame을 실행한다.
    public void OnEveryFrame()
    {
        if(nowState != State.GameEnd && nowState != State.Ready)
        {
            
            GetInput();
        }
        CommandCheck();
        callEveryFrame();   //델리게이트. 여기서 모션바꾸고, 스킬 써주고.
    }
    void GetInput()
    {

        if (Input.GetKey(playerKeys[0]) && Input.GetKey(playerKeys[1]) && Input.GetKey(playerKeys[2]))
        {
            if (nowUltPoint == ultPoint)
            {
                CommandInput(Command.Ulti, 'u');
                return;
            }
        }

        if (Input.GetKeyDown(playerKeys[0]))
        {
            CommandInput(Command.I, 'z');
        }
        if (Input.GetKeyDown(playerKeys[1]))
        {
            CommandInput(Command.O, 'x');
        }
        if (Input.GetKeyDown(playerKeys[2]))
        {
            CommandInput(Command.P, 'c');
        }



        if (Input.GetKeyDown(playerKeys[3]))
        {
            //left
            if (nowState == State.Move || nowState == State.Wait || nowState == State.Jump)
            {
                movingRight = false;
                transform.localRotation = Quaternion.Euler(0, 180, 0);  //이동방향 맞춰주려고 회전

                MoveStart(Command.Left);
            }


        }
        if (Input.GetKeyDown(playerKeys[5]))
        {
            //right
            if(nowState == State.Move || nowState == State.Wait || nowState == State.Jump)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);    //마찬가지로 이동방향
                movingRight = true;
                MoveStart(Command.Right);
            }

        }

        if (Input.GetKey(playerKeys[3]))
        {
            //left
            if (nowState != State.Move && nowState != State.Jump)
            {
                movingRight = false;
                nowMoveKey.frame = 0;
                MoveStart(Command.Left);
            }
        }
        if (Input.GetKey(playerKeys[5]))
        {
            //right
            if (nowState != State.Move && nowState != State.Jump)
            {
                movingRight = true;
                nowMoveKey.frame = 0;
                MoveStart(Command.Right);
            }
        }

        if (Input.GetKey(playerKeys[4]))
        {
            //down
            if (nowState == State.Move || nowState == State.Wait || nowState == State.Jump)
            {
                GuardStart();
            }
        }
        if (Input.GetKeyUp(playerKeys[4]))
        {
            //down
            if (nowState == State.Guarding || nowState == State.GuardSuccess)
            {
                GuardEnd();
            }
        }


        if (Input.GetKeyDown(playerKeys[6]))
        {
            //up
            JumpStart();


        }
        if (Input.GetKeyUp(playerKeys[5]) || Input.GetKeyUp(playerKeys[3]))
        {
            //Idle
            if (nowState == State.Move)
            {
                if (Input.GetKey(playerKeys[5]))
                {
                    movingRight = true;
                    nowMoveKey.frame = 0;
                    MoveStart(Command.Right);
                }
                else if (Input.GetKey(playerKeys[3]))
                {
                    movingRight = false;
                    nowMoveKey.frame = 0;
                    MoveStart(Command.Left);
                }
                else
                {
                    IdleStart();
                }
            }

        }
    }

    //커맨드를 큐에 넣어주지만, 그 즉시 디큐하기 때문에 필요성이 의심된다.
    //지금은 단순히 커맨드 입력시간 저장 목적이다.
    void CommandInput(Command com, char comChar)
    {
        if (inputBool == false)
            return;
        if (nowState != State.Wait && nowState != State.Move && nowState != State.OnSkill && nowState != State.Jump)
            return;
        //이 세 개중에 다른거면 씹는다는 뜻이다.
        //커맨드 대기상태나 이동중이 아니라면 씹는다.
        CommandFrame command;
        command.frame = FrameManager.nowFrame;

        command.com = com;
        command.commandChar = comChar;
        commandQueue.Enqueue(command);
     
    }

    //OnEveryFrame에서 CommandInput이 일어난 후에 일어난다.
    public void CommandCheck()
    {
        if(commandQueue.Count == 0)
        {
            return;
        }
        bool right = false;      //연속기인지
        CommandFrame nowCom = commandQueue.Dequeue();
        char[] strCom;
        int skillIndex = 0;
        //첫째로, 연속기인지 아닌지 판단한다.
        if (latestCommand.Count != 0)
        {
            latestCommand.Add(nowCom);
            strCom = new char[latestCommand.Count];

            for (int i = 0; i < latestCommand.Count; i++)
            {
                strCom[i] = latestCommand[i].commandChar;
            }
            //연속기라고 판명, 애드해본다.
        }
        else
        {
            //만약 지난 커맨드가 없었다면, 
            strCom = new char[1];
            strCom[0] = nowCom.commandChar; //일단 할당해준다.
            latestCommand.Add(nowCom);
        }

        //이제 커맨드 연속기가 존재하는지 확인한다. 확인 했을 때 없다면, 그 커맨드를 단일커맨드로 다시 실행한다.

        do
        {
            string inputStr = new string(strCom);
            string jsonStr;
            for (int i = 0; i < skillArray.Length; i++)
            {
                jsonStr = skillArray[i].command;
                //1개짜리 입력이든, 2개짜리 입력이든 성공시키고 싶다.

                if (inputStr == jsonStr)
                {
                    right = true;
                    skillIndex = i;
                    break;
                }
            }
            Debug.Log("커맨드는 : " + inputStr);
            if (commandQueue.Count > 0 && !right)
            {
                //입력된게 더 남아있고, 맞는거도 없다면 다시 확인.
                //다시 확인했을 때 맞는게 없다면, 단일커맨드로 위에서 재실행.
                latestCommand.Remove(nowCom);
                nowCom = commandQueue.Dequeue();
                latestCommand.Add(nowCom);
                strCom = new char[latestCommand.Count];
                for (int i = 0; i < latestCommand.Count; i++)
                {
                    strCom[i] = latestCommand[i].commandChar;
                }
            }
        } while (commandQueue.Count > 0 && !right);
        if (right)
        {
            //string inputStr = new string(strCom);
           
            SkillActive(skillIndex);
            /*
            zz의 후딜시간에 zzz했다 치자.
            미리 입력을 하고, 후딜끝 시간을 알고있으니까. 그걸 기준으로 inputEnd를 때리고,
            미리 입력한 순간부터 입력받으면 안되니까, inputStart는 후딜 기준으로 받돼, 
            입력한 순간부터 inputBool을 꺼준다.
            */
}
        else
        {
            latestCommand.Remove(nowCom);
        }


    }



    void CommandListClear(int inputStart,int inputEnd)
    {
        //파라미터 받고 코루틴 실행하려구
        inputStartFrame = FrameManager.nowFrame + inputStart;
        clearFrame = FrameManager.nowFrame + inputEnd;
        if (corRunning)
        {
            StopCoroutine(commandClearer);
        }
        commandClearer = StartCoroutine(CommandClearCor());
    }


    /*SpriteChanger 정리 19.04.17 정상훈 */
    /* SpriteChanger는 이동, idle, 스킬, 점프 등 전반적인 스프라이트 애니메이션을 담당한다
     * SpriteChanger는 callEveryFrame에 들어가서 실행된다.
     * spriteChangeCounter는 현재 Sprite로 변한 후 몇프레임이나 지났는지 세어준다. 1프레임당 ++된다
     * spriteChangeGab[n]는 몇프레임마다 한 번씩 스프라이트를 바꿔야 하는지 알려준다.
     * 바꿔야 하는 스프라이트는 nowSpriteArr이다. 여기에 각 모션의 spriteArr를 넣으면 된다.
     * 0 1 2 3 4 5 6 7 8 9 선딜 후딜 Idle Move Jump Hit Falldown Guard Dash 발동이다.(추후추가 가능)
     * 0 1 는 스킬의 선딜 후딜이기 때문에 스킬을 사용할 때마다 값을 바꿔주고, 나머지는 그대로다.
     * 가독성이 떨어지게 array로 하는 이유는 SpriteChanger에서 모든 애니메이션을 실행하기 위해서이다.
     * 모션을 실행할 때는 (motionName)Start() 형식의 함수에서 실행된다.
     * 그 함수에서는 사용하는 indexNumber, spriteChangeCounter와 nowSpriteArr의 초기화를 해준다.
     * spriteChangeCounter의 초기화는 0이 아닌 각 모션의 spriteChangeGab[n]으로 한다.
     * 그래야만 spriteChanger에 진입하자마자 0번 sprite가 켜진다.
     * 다만, SkillActive만 예외적으로 spriteCounter를 0으로 한다.
     *  DelegateSkill에서는 스프라이트 변환에 조건문을 안달아놨다
     *  이럴거면 각 모션 스타트에 spriteRenderer = nowSpriteArr[0] 하면 되는데.. 귀찮다..
     * (19.04.17)
     * 
     */
    public void SpriteChanger()
    {
        //여기서 아이들 점프의 스프라이트 변환을 해준다. 무브는 움직임때문에 안댐
        spriteChangeCounter++;  //카운터 올려주고.
        if (spriteChangeCounter > spriteChangeGab[nowGabIndex])
        {
            if (nowSpriteIndex >= spriteCount)
            {
                nowSpriteIndex = 0;
            }

            spriteRenderer.sprite = nowSpriteArr[nowSpriteIndex];
            nowSpriteIndex++;
            spriteChangeCounter = 0;
        }
    }

    void MoveStart(Command com)
    {
        //spriteChangeGab 3를 쓴다
        //움직이기 시작할 떄 설정맞춰주기위한 함수. 
        if(nowState == State.Jump)
        {
            latestMoveKey = nowMoveKey;
            nowMoveKey.frame = FrameManager.nowFrame;
            nowMoveKey.com = com;

            if (nowMoveKey.frame - latestMoveKey.frame < dashInputFrame)
            {
                if (nowMoveKey.com == latestMoveKey.com)
                {

                    nowMoveKey.frame = 0;
                    DashStart();
                    return;
                }
            }
        }
        if (nowState != State.Wait)
        {
            return;
        }
        if(nowState != State.Dash)
        {
            latestMoveKey = nowMoveKey;
            nowMoveKey.frame = FrameManager.nowFrame;
            nowMoveKey.com = com;

            if (nowMoveKey.frame - latestMoveKey.frame < dashInputFrame)
            {
                if (nowMoveKey.com == latestMoveKey.com)
                {

                    nowMoveKey.frame = 0;
                    DashStart();
                    return;
                }
            }
        }
        
        rigidBody.velocity = Vector2.zero;
        nowState = State.Move;
        spriteChangeCounter = spriteChangeGab[3];
        nowSpriteIndex = 0;
        nowSpriteArr = moveSpriteArr;
        spriteCount = moveSpriteArr.Length;
        nowGabIndex = 3;
        playerSound.SoundPlay(nowGabIndex, true);   //무조건 나우 갭 인덱스 아래에 와야함
        callEveryFrame = Move; //업데이트를 무브로 바꾼다.
    }

    public void Move()
    {
        //direction이 true면 앞으로감.
        /*
        if (nowState != State.Wait && nowState != State.Move)
            return;*/
        float velocity;
        if (!movingRight)
        {
            velocity = -1f * moveSpeed;
            transform.localRotation = Quaternion.Euler(0, 180, 0);  //이동방향 맞춰주려고 회전
        }
        else
        {
            velocity = moveSpeed;
            transform.localRotation = Quaternion.Euler(0, 0, 0);    //마찬가지로 이동방향
        }
        nowState = State.Move;
        Vector3 pos = gameObject.transform.position;
        gameObject.transform.position = pos + Vector3.right * velocity;

        //이제 스프라이트 체인지 한다. 위에서 한건 뻔한 이동임.
        SpriteChanger();
    }
    
    public void JumpMove()
    {
        float velocity;
        if (!movingRight)
        {
            velocity = -1f * moveSpeed;
            transform.localRotation = Quaternion.Euler(0, 180, 0);  //이동방향 맞춰주려고 회전
        }
        else
        {
            velocity = moveSpeed;
            transform.localRotation = Quaternion.Euler(0, 0, 0);    //마찬가지로 이동방향
        }
        Vector3 pos = gameObject.transform.position;
        gameObject.transform.position = pos + Vector3.right * velocity;

    }

   

    public void GuardStart()
    {
        //대쉬시에는 시작할때 조건이 많이 붙었는데, 점프대쉬가 있어서 그랬따.
        //여기서는 DownDashStart에서 조건들을 봐준다.
        latestMoveKey = nowMoveKey;
        nowMoveKey.frame = FrameManager.nowFrame;
        nowMoveKey.com = Command.Down;

        if (nowMoveKey.frame - latestMoveKey.frame < dashInputFrame)
        {
            if (nowMoveKey.com == latestMoveKey.com)
            {
                nowMoveKey.frame = 0;
                DownDashStart();
                return;
            }
        }

        nowState = State.Guarding;
        spriteChangeCounter = spriteChangeGab[7];
        nowSpriteIndex = 0;
        nowSpriteArr = guardSpriteArr;
        spriteCount = nowSpriteArr.Length;
        nowGabIndex = 7;
        playerSound.SoundPlay(nowGabIndex, false);   //무조건 나우 갭 인덱스 아래에 와야함
        callEveryFrame = Guard;



    }

    public void Guard()
    {
        if (nowSpriteIndex == spriteCount)
            return;
        //가드는 순환이 아니라 1번하면 안변해야 한다
        spriteChangeCounter++;  //카운터 올려주고.
        if (spriteChangeCounter > spriteChangeGab[nowGabIndex])
        {
            spriteRenderer.sprite = nowSpriteArr[nowSpriteIndex];
            nowSpriteIndex++;
            spriteChangeCounter = 0;
        }


    }

    public void GuardEnd()
    {
        if (jumping)
        {
            JumpStart();
        }
        else
        {
            IdleStart();
        }
        
    }

    public void DashStart()
    {
        
        if (movingRight)
        {
            watchingRightOne = 1;
        }
        else
        {
            watchingRightOne = -1;
        }
        Vector2 force;
        if (nowState == State.Jump)
        {
            force = new Vector2(watchingRightOne * xJumpDashForce, yJumpDashForce);
        }
        else
        {
            force = new Vector2(watchingRightOne * xDashForce, yDashForce);
        }
        rigidBody.velocity = Vector2.zero;
        rigidBody.AddForce(force, ForceMode2D.Impulse);
        dashFrameCounter = 0;       //대쉬는 dashFrameLength에 도달하면 idle로 건너가야한다.
        nowState = State.Dash;      //대쉬중에 스킬은 아직 못쓴다. rigidbody 펄스 두개 겹치면 클난다
        spriteChangeCounter = spriteChangeGab[8];
        nowSpriteIndex = 0;
        nowSpriteArr = dashSpriteArr;
        spriteCount = dashSpriteArr.Length;
        nowGabIndex = 8;
        playerSound.SoundPlay(nowGabIndex, false);   //무조건 나우 갭 인덱스 아래에 와야함
        callEveryFrame = Dash;
    }

    public void Dash()
    {
        dashFrameCounter++;
        if(dashFrameCounter >= dashFrameLength)
        {
            nowState = State.Wait;
            if (jumping)
            {
                JumpStart();
            }
            else
            {
                IdleStart();
            }
            
        }
        SpriteChanger();
    }

    public void IdleStart()
    {
        //spriteChangeGab 2를 쓴다
        //간혹 스킬중, 게임끝나는 모션이 나와야 하는데 idleStart가 나오는 경우가 있다
        //SkillEnd에서 나와서 그런 것 같다;
        if (nowState == State.OnSkill || nowState == State.GameEnd)
        {
            return;
        }

        //invulnerable = false; 일단 꺼 보자
        nowState = State.Wait;
        spriteChangeCounter = spriteChangeGab[2];
        nowSpriteIndex = 0;
        nowSpriteArr = idleSpriteArr;
        spriteCount = nowSpriteArr.Length;
        nowGabIndex = 2;
        playerSound.StopLoop();
        callEveryFrame = SpriteChanger;
    }

    void DownDashStart()
    {
        
        if (jumping)
        {
            //점프중이라면 콜라이더를 꺼줄 필요가 없다.
            JumpStart();
        }
        else
        {
            //if (collider.IsTouching(floor) || collisionPlatform.CompareTag("Floor"))
            if (collider.IsTouching(floor))
            {
                return;
            }
            //바닥이랑 닿는거면 안해도돼!
            jumping = true;
            JumpStart();
            StartCoroutine(DownDashCor());
        }
        rigidBody.velocity = Vector2.zero;
        rigidBody.AddForce(new Vector2(0, -downDashForce));    //아래로 대쉬해준다.
    }

    IEnumerator DownDashCor()
    {
        if (downDashCorRunning)
        {
            downDashWaitFrame = FrameManager.nowFrame + downDashColFrame;
        }
        else
        {
            // 하향 점프시 땅 짚는 콜라이더 없앰.
            groundStandingCollider.enabled = false;
            StartCoroutine(ProlongColliderOn(false));
            collider.enabled = false;
            downDashCorRunning = true;
            downDashWaitFrame = FrameManager.nowFrame + downDashColFrame;
            while (FrameManager.nowFrame <= downDashWaitFrame && transform.position.y > collisionPlatform.transform.position.y - 6f)
            {
                //일정시간이 지나면 꺼준다.
                yield return null;
            }
            downDashCorRunning = false;
            collider.enabled = true;

        }

    }

    public void JumpStart()
    {
        //점프시작.
        
        if(nowState != State.Wait && nowState != State.Move && nowState != State.Guarding)
        {
            return;
        }
        if (jumping == true)
        {
            //이미 날고있는 상태에서 여기에 다시 진입했을 때
            nowState = State.Jump;
            spriteChangeCounter = spriteChangeGab[4];
            nowSpriteIndex = 1;
            nowSpriteArr = jumpSpriteArr;
            nowGabIndex = 4;
            
            spriteCount = nowSpriteArr.Length;
        }
        else
        {
            rigidBody.velocity = Vector2.zero;
            rigidBody.AddForce(Vector2.up * jumpAccelaration, ForceMode2D.Impulse);
            jumping = true;
            nowState = State.Jump;

            // 땅을 밟을수 있게 하는 콜라이더를 제거(end코루틴에서 다시 on함.)
             groundStandingCollider.enabled = false;
            //groundStandingCollider.isTrigger = true;
            spriteChangeCounter = spriteChangeGab[4];
            nowSpriteIndex = 0;
            nowSpriteArr = jumpSpriteArr;
            nowGabIndex = 4;
            playerSound.SoundPlay(nowGabIndex, false);   //무조건 나우 갭 인덱스 아래에 와야함
            StartCoroutine(ProlongColliderOn(true));
            spriteCount = nowSpriteArr.Length;
        }
    
        callEveryFrame = Jump;
    }
    // 대각선 점프 위해 추가 함. 0.8초 뒤에 콜라이더를 다시 킴.
    IEnumerator ProlongColliderOn(bool up)
    {
        if (up)
        {
            yield return new WaitForSeconds(0.8f);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }
        groundStandingCollider.enabled = true;
    }
    public void Jump()
    {
        /*
        Vector2 pos = transform.position;
        transform.position = pos + Vector2.up * nowJumpVelocity;
        nowJumpVelocity -= jumpAccelaration;
        */

        //점프는 하는 수 없이 점프 스타트에서 rigidbody2D로 올려준다.
        if (Input.GetKey(playerKeys[3]))
        {
            //left
            movingRight = false;
            JumpMove();
        }
        if (Input.GetKey(playerKeys[5]))
        {
            //right
            movingRight = true;
            JumpMove();
        }


        spriteChangeCounter++;  //카운터 올려주고.
        if (spriteChangeCounter > spriteChangeGab[nowGabIndex])
        {
            if (nowSpriteIndex >= 2)
            {
                spriteChangeCounter = 0;
                return;
            }
            spriteRenderer.sprite = nowSpriteArr[nowSpriteIndex];
            nowSpriteIndex++;
            spriteChangeCounter = 0;
        }
    }

    bool jumpCorRunning = false;
    IEnumerator JumpEndCor()
    {
        // 땅을 밟을수 있게 하는 콜라이더를 다시 켬
        groundStandingCollider.enabled = true;
        //착지하는 코루틴
        jumpCorRunning = true;
        spriteRenderer.sprite = jumpSpriteArr[2];
        int endFrame = FrameManager.nowFrame + spriteChangeGab[4];
        while (FrameManager.nowFrame < endFrame)
        {
            yield return null;
        }
        spriteRenderer.sprite = jumpSpriteArr[3];
       
        endFrame = FrameManager.nowFrame + spriteChangeGab[4];
        while (FrameManager.nowFrame < endFrame)
        {
            yield return null;
        }
        jumpCorRunning = false;
        IdleStart();
    }

    protected virtual void SkillActive(int index)
    {
        //스킬이 활성화되는 최초에 실행되는 함수.
        SkillJson skill;
        if (nowState == State.OnSkill)
        {
            //스킬 실행했는데, 시전중이라면 기다려야겠지.
            //대기중인 변수를 만들되, 그걸 int로 해서 스킬 인덱스로 단 하나 저장하자.
            //대기중인 스킬이 없다면, 그 인덱스를 -1로 정하고, 대기중이라면 인덱스를 저장해준다.

            CommandListClear(500, 500); //그 저장을 여기서 해주면 된다.
            if (waitingSkillIndex == -1)
            {
                waitingSkillIndex = index;
            }

            //이거 굉장히 위험하다. 여기에 왔다가 안풀리면 클난다.

        }
        else
        {
            
            waitingSkillIndex = -1;
            skill = skillArray[index];
            nowSkillIndex = index;
            nowSpriteArr = skill.spriteArray;
            //baseFrame = FrameManager.nowFrame;         //현재 프레임을 구한다.
            startFrame = FrameManager.nowFrame + skill.startFrame;          //선딜이 끝나는 시점을 구하고
            activeFrame = startFrame + skill.activeFrame;
            endFrame = activeFrame + skill.endFrame;            //후딜이 끝나는 시점을 구한다.
            spriteCount = skill.spriteArray.Length;         //스프라이트 몇개있는지 구하고
            triggerIndex = skill.triggeringIndex;       //몇번 까지 켜야되는지 구한다
            nowSpriteIndex = 0;
            //spriteChangeGab = new int[2]; //몇 프레임에 한 번씩 스프라이트를 바꿔야 하는가.
            spriteChangeCounter = 0;        //카운터 초기화

            Debug.Log(skill.command + " : 발동");
            //총 8장이라 보자


            //15프레임에 5장이면, 3프레임당 하나씩
            //전체 장수 8, count 3이고, 3w장 보여준거니까, 5장으로 나누면. 
            //후딜은 식이 상당히 애매하다. 으음...
            //스킬 시작하자마자 바로 스프라이트 바꿔주어야한다
            //타격은 시작점이 중요하니까, 타격 중에 모션이 뭐인지는 안봐도 된다.

            nowState = State.OnSkill;
            //스킬 실행중일 때 이동 못하게하려고.
        

            skillRight = movingRight;
            if (movingRight)
            {
                watchingRightOne = 1;
            }
            else
            {
                watchingRightOne = -1;
            }
           // basePosition = transform.position;      //스킬의 이동 보간
            targetPosition = new Vector2(watchingRightOne * skill.xMoveDistance,skill.yMoveDistance);   //이거를 force값으로 쓴다
            //positionChangeLerp = skill.startFrame;
            positionChangeLerp = skill.startFrame + skill.activeFrame + skill.endFrame;
            lerpCounter = 0;

            /*
             * 스킬사용시 이동에 중력을 맞을 수 있게 rigidbody로 진행
            Vector2 force = new Vector2(watchingRightOne * skill.xMoveDistance, skill.yMoveDistance * 5);
            rigidBody.AddForce(force,ForceMode2D.Impulse);
            */
            CommandListClear(skill.inputStart, skill.inputEnd);
            rigidBody.velocity = Vector2.zero;
            rigidBody.AddForce(targetPosition, ForceMode2D.Impulse);

            //클리어도 모션에서 해준다. 코루틴으로 해주는 이유는, 이거는 좀 널널히 하면 좋을거같에서 그렇다.
            //엄밀히해야되는건 update에서 해주고, 나머지는 그냥 코루틴으로 할래.
            if (skill.command.Contains("u"))
            {
                nowUltPoint = 0;
                spriteChangeGab[0] = skill.startFrame / (triggerIndex);//이거 예전에는 triggerindex-1이었음...
                                                                       //10프레임에 3장이라면, 3프레임당 하나씩 0 1 2 3 트리거인덱스 3라면.
                spriteChangeGab[9] = skill.activeFrame / (skill.endIndex - triggerIndex);
                spriteChangeGab[1] = (skill.endFrame) / (skill.ultTriggeringIndex - skill.endIndex);
                spriteCount = skill.ultTriggeringIndex;
                ultTriggerIndex = skill.ultTriggeringIndex;
                ultEndIndex = skill.ultEndIndex;
                ultSpriteCount = skill.spriteArray.Length - ultTriggerIndex;
                ultHitted = false;
                playerSound.SkillSoundPlay(skill.soundEffectClip,skill.soundEffectFrame); 
                if(skill.voiceFrame != -1)
                {
                    //보이스는 있을수도 없을수도 있다
                    playerSound.SkillVoicePlay(skill.voiceClip, skill.voiceFrame);
                }
                callEveryFrame = DelegateUlt;
            }
            else
            {
                spriteChangeGab[0] = skill.startFrame / (triggerIndex);//이거 예전에는 triggerindex-1이었음...
                                                                       //10프레임에 3장이라면, 3프레임당 하나씩 0 1 2 3 트리거인덱스 3라면.
                spriteChangeGab[9] = skill.activeFrame / (skill.endIndex - triggerIndex);
                spriteChangeGab[1] = (skill.endFrame) / (spriteCount - skill.endIndex);

                playerSound.SkillSoundPlay(skill.soundEffectClip, skill.soundEffectFrame);
                if (skill.voiceFrame != -1)
                {
                    //보이스는 있을수도 없을수도 있다
                    playerSound.SkillVoicePlay(skill.voiceClip, skill.voiceFrame);
                }
                callEveryFrame = DelegateSkill;    //업데이트에서 계속 호출.
            }

        }
    }


    public void SkillEnd()
    {
        //스킬이 끝났을 때 호출.
        nowState = State.Wait;      //이걸 먼저해줘야 한다. 그래야 다른데서 안씹힌다.
        skillEffectObj.SetActive(false);    //이펙트 꺼주고
        if (waitingSkillIndex == -1)
        {
            //예약스킬이 없을 때
            if (jumping)
            {
                JumpStart();
            }
            else
            {
                IdleStart();
            }

        }
        else
        {
            //예약스킬이 있을 때
            if (Input.GetKey(playerKeys[3]))
            {
                movingRight = false;
                transform.localRotation = Quaternion.Euler(0, 180, 0);  //이동방향 맞춰주려고 회전

            }
            if (Input.GetKey(playerKeys[5]))
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);    //마찬가지로 이동방향
                movingRight = true;
            }

         

            //바라보는 방향하고 스킬이 나가는 방향하고 안맞는 괴리가 생긴다.
            //누적된 커맨드가 있다면.

            SkillActive(waitingSkillIndex);
        }
        
    }

    //이거 FrameManager에서 호출함.
    public void SkillHittedEffect()
    {
        if (skillArray[nowSkillIndex].hittedEffect)
        {
            hittedEffectCorRunning = false; //중간에 이걸 꺼줘야 스프라이트 두개가 안뜬다.
            //지금 코루틴이 스킬의 후딜 끝날때까지 진행되는데, 스킬의 후딜중에 다른걸 다시 맞을 수 있다.
            //zzzz콤보는 후딜중에 콤보가 들어갈 수 있게 설정하였기 때문에, 무조건 이걸 꺼줘야 코루틴이 된다.
            StartCoroutine(HittedEffectCor());
        }
    }


    IEnumerator HittedEffectCor()
    {
        yield return null;  //1프레임을 시작에 기다려줘야 후딜중에 맞을 때 이중코루틴이 걸리지 않는다.
        hittedEffectCorRunning = true;

        SkillJson skill = skillArray[nowSkillIndex];
        Sprite[] array = skill.hittedSpriteArray;
        int nowIndex = 0;
        int count = skillArray[nowSkillIndex].hittedSpriteArray.Length;
        int end =  endFrame; //코루틴 중간에 값이바뀔 우려가 있어서 그랬다.
        int gab = (endFrame - FrameManager.nowFrame) / count;
        int counter = 0;
        hittedEffectRenderer.sprite = array[nowIndex];
        nowIndex++;

        while (FrameManager.nowFrame <= end && hittedEffectCorRunning)
        {
            Debug.Log(end - FrameManager.nowFrame);
            counter++;
            if (counter >= gab)
            {
                hittedEffectRenderer.sprite = array[nowIndex];
                nowIndex++;
                counter = 0;
                if (nowIndex > count - 1)
                {
                    nowIndex = count - 1;
                }
            }
            yield return null;
        }
        hittedEffectRenderer.sprite = null;

        hittedEffectCorRunning = false;
    }


    public void UltHItted()
    {
        //이게 동시타가 되버리면 hitted상태에서 맞아버리고, 여기서 gab이 꼬여버린다.


        SkillJson skill = skillArray[nowSkillIndex];
        ultHitted = true;
        if (nowState == State.OnSkill)
        {
            nowSpriteIndex = skill.ultTriggeringIndex;
            spriteCount = skill.spriteArray.Length;
        }
        //10프레임에 3장이라면, 3프레임당 하나씩 0 1 2 3 트리거인덱스 3라면.
        spriteChangeGab[9] = skill.ultActiveFrame / (skill.ultEndIndex - skill.ultTriggeringIndex);
        spriteChangeGab[1] = skill.ultEndFrame / (spriteCount - skill.ultEndIndex);
        activeFrame = FrameManager.nowFrame + skill.ultActiveFrame;
        endFrame = activeFrame + skill.ultEndFrame;            //후딜이 끝나는 시점을 구한다.
    }

    //FrameManager에서 불러준다. Player끼리는 상대에게 얼마나 데미지를 주었는지 알 수가 없다.
    public void UltPointIncrease(float damage)
    {
        nowUltPoint += damage * 0.8f;
        if(nowUltPoint > ultPoint)
        {
            nowUltPoint = ultPoint;
        }
    }

    int ultTriggerIndex;
    int ultEndIndex;
    int ultSpriteCount;
    public bool ultHitted;
    void DelegateUlt()
    {
        //중간에 맞았으면 ultTriggerIndex로 넘어가야한다.
        if (nowState != State.OnSkill)
        {
            nowState = State.OnSkill;
        }
        /*
        lerpCounter++;
        Vector2 moveDistance = Vector2.Lerp(basePosition, targetPosition, lerpCounter / positionChangeLerp);
        transform.position = moveDistance;*/

        spriteChangeCounter++;  //1프레임 올려주고.
        if (!ultHitted)
        {
            if (FrameManager.nowFrame <= startFrame)
            {

                //선딜 중이라면.
                if (spriteChangeCounter >= spriteChangeGab[0])
                {
                    //두장 나와야되는데;
                    nowSpriteIndex++;           //갭에 도달했으면, 인덱스1올려주고
                    if (nowSpriteIndex > spriteCount - 1)
                    {
                        nowSpriteIndex--;
                        //예외처리
                    }
                    spriteChangeCounter = 0;    //카운터를 0으로 초기화.
                }
            }
            else if (FrameManager.nowFrame <= activeFrame)
            {
                frameManager.SkillRequest(playerNum, skillArray[nowSkillIndex], FrameManager.nowFrame - activeFrame + skillArray[nowSkillIndex].activeFrame, skillRight);
                if (spriteChangeCounter >= spriteChangeGab[9])
                {
                    nowSpriteIndex++;           //갭에 도달했으면, 인덱스1올려주고 
                    if (nowSpriteIndex > spriteCount - 1)
                    {
                        nowSpriteIndex--;
                        //예외처리
                    }
                    spriteChangeCounter = 0;    //카운터를 0으로 초기화.
                }
            }
            else if (FrameManager.nowFrame <= endFrame)
            {

                if (spriteChangeCounter >= spriteChangeGab[1])
                {
                    nowSpriteIndex++;           //갭에 도달했으면, 인덱스1올려주고 
                    if (nowSpriteIndex > spriteCount - 1)
                    {
                        nowSpriteIndex--;
                        //예외처리
                    }
                    spriteChangeCounter = 0;    //카운터를 0으로 초기화.
                }
            }
            else
            {
                //스킬범위를 벗어났다면 끝내야지.
                SkillEnd();
            }
        }
        else
        {
            //궁맞았으면.
            if (FrameManager.nowFrame <= activeFrame)
            {
                if (spriteChangeCounter >= spriteChangeGab[9])
                {
                    nowSpriteIndex++;           //갭에 도달했으면, 인덱스1올려주고 
                    if (nowSpriteIndex > spriteCount - 1)
                    {
                        nowSpriteIndex--;
                        //예외처리
                    }
                    spriteChangeCounter = 0;    //카운터를 0으로 초기화.
                }
            }
            else if (FrameManager.nowFrame <= endFrame)
            {

                if (spriteChangeCounter >= spriteChangeGab[1])
                {
                    nowSpriteIndex++;           //갭에 도달했으면, 인덱스1올려주고 
                    if (nowSpriteIndex > spriteCount - 1)
                    {
                        nowSpriteIndex--;
                        //예외처리
                    }
                    spriteChangeCounter = 0;    //카운터를 0으로 초기화.
                }
            }
            else
            {
                //스킬범위를 벗어났다면 끝내야지.
                SkillEnd();

            }
        }
    
        if (spriteCount == 0)
        {
            return;
        }
        spriteRenderer.sprite = nowSpriteArr[nowSpriteIndex];
        //이건 그냥 마지막에 넣어준다.
    }

    void DelegateSkill()
    {
        // 한 스킬의 모션은 한 함수에서 관리한다. 연속기 인풋을 미리 해놓는것도 여기서 관리한다.
        if (nowState != State.OnSkill)
        {
            nowState = State.OnSkill;
        }
        /*
        lerpCounter++;
        Vector2 moveDistance = Vector2.Lerp(basePosition, targetPosition, lerpCounter / positionChangeLerp);
        transform.position = moveDistance;*/
        
        spriteChangeCounter++;  //1프레임 올려주고.
        if (FrameManager.nowFrame <= startFrame)
        {

            //선딜 중이라면.
            if (spriteChangeCounter >= spriteChangeGab[0])
            {
                //두장 나와야되는데;
                nowSpriteIndex++;           //갭에 도달했으면, 인덱스1올려주고
                if (nowSpriteIndex > spriteCount - 1)
                {
                    nowSpriteIndex--;
                    //예외처리
                }
                spriteChangeCounter = 0;    //카운터를 0으로 초기화.
            }
        }
        else if (FrameManager.nowFrame <= activeFrame)
        {
            if(skillArray[nowSkillIndex].skillType == 0)//스킬타입 0은 타격.
            {
              
                //FrameManager.nowFrame - activeFrame + skillArray[nowSkillIndex].activeFrame - 1 : 이게 indexOrder이다.
                //activeFrame은 FrameManager.nowFrame + skill.activeFrame이다. 즉 FrameManager와 비교하는 값이다.
                //여기서 그냥 FrameManager.nowFrame을 빼버리면 0값부터 시작을 안해서, skill.activeFrame을 더해주어야한다.
                frameManager.SkillRequest(playerNum, skillArray[nowSkillIndex], FrameManager.nowFrame - activeFrame + skillArray[nowSkillIndex].activeFrame - 1, skillRight);
            }
            else if(skillArray[nowSkillIndex].skillType == 1)
            {
                if(FrameManager.nowFrame == activeFrame)
                {
                    buffRatio = skillArray[nowSkillIndex].buffedDamage;
                    buffFrame = FrameManager.nowFrame + skillArray[nowSkillIndex].buffFrameLength;
                    StartCoroutine(buffCor());
                    //버프는 한 번만 써준다
                }
            }
            
            if (spriteChangeCounter >= spriteChangeGab[9])
            {
                nowSpriteIndex++;           //갭에 도달했으면, 인덱스1올려주고 
                if (nowSpriteIndex > spriteCount - 1)
                {
                    nowSpriteIndex--;
                    //예외처리
                }
                spriteChangeCounter = 0;    //카운터를 0으로 초기화.
            }

            if (skillArray[nowSkillIndex].projectileEffect)
            {
                skillEffectObj.SetActive(true);
                //이펙트 켜주는거는 최초 한 번만 하고싶은데, 그러면 1프레임 일찍 켜져버려서 힘들다.
            }
        }
        else if (FrameManager.nowFrame <= endFrame)
        {
            if(FrameManager.nowFrame == endFrame)
            {
                skillEffectObj.SetActive(false);
            }
            if (spriteChangeCounter >= spriteChangeGab[1])
            {
                nowSpriteIndex++;           //갭에 도달했으면, 인덱스1올려주고 
                if (nowSpriteIndex > spriteCount - 1)
                {
                    nowSpriteIndex--;
                    //예외처리
                }
                spriteChangeCounter = 0;    //카운터를 0으로 초기화.
            }
        }
        else
        {
            //스킬범위를 벗어났다면 끝내야지.
            SkillEnd();

        }
        if (spriteCount == 0)
        {
            return;
        }
        spriteRenderer.sprite = nowSpriteArr[nowSpriteIndex];
        //이건 그냥 마지막에 넣어준다.
    }

    //FrameManager에서 호출된다
    /* 상대편에게 입힌데미지 정보를 알려주어야 하기 때문에 float damagedHP를 return한다
     * 입힌 데미지 정보는 상대플레이어의 UltPointIncrease로 들어가고, 상대편의 ultPoint를 높여준다.
     * 스파게티 인정합니다 죄송합니다.
     */
    public float SkillBeatenStart(SkillJson skill, int leftActiveFrame, int one, float oponentBuffRatio, out bool skillGuarded)
    {
        //leftActiveFrame = 상대방에게 남은 발동프레임. 이 시간도 후딜에 포함된다.
        //one = 어느쪽보고있는지 전달해준다. 오른쪽보면 -1 쪽으로 밀려나고, 왼쪽보면 +1쪽으로 밀려난다
        //이거는 발동 제한조건이 없다. 다만 다른거에서는 모든게 발동 못하게 넣어놔야겠지.

        Debug.Log(playerNum + "의 스킬 비튼");
        float damagedHP;  //내가 얼마나 맞았는지.
        skillEffectObj.SetActive(false);    //나의 투사체를 꺼준다.
        invulnerable = true;                //무적
        one = one * -1; //FrameManager에서 받는 one은 시전자 기준이다. 여기는 맞는거니까 반대로 해줘야한다.
        oponentSkill = skill;               //상대스킬이 뭔지.
        if (one == -1)
        {
            movingRight = false;
            transform.localRotation = Quaternion.Euler(0, 180, 0);  //이동방향 맞춰주려고 회전

        }
        else
        {
            movingRight = true;
            transform.localRotation = Quaternion.Euler(0, 0, 0);  //이동방향 맞춰주려고 회전

        }

        skillGuarded = false; //궁 맞았는지 이거 초기화

        nowSpriteIndex = 0;
        //basePosition = transform.position;
        // lerpCounter = 0; 위치보간은 안쓴다
        Debug.Log(leftActiveFrame + "남은 액티브프레임");
        invulnerableFrame = FrameManager.nowFrame + leftActiveFrame;
        if (nowState == State.Guarding || nowState == State.GuardSuccess)
        {
            damagedHP = (skill.damage * oponentBuffRatio) * 0.4f;
            nowHealthPoint -= damagedHP;
            nowUltPoint += damagedHP * 0.4f;
            if (nowUltPoint > ultPoint)
            {
                //궁 게이지가 올라간다
                nowUltPoint = ultPoint;
            }
            nowState = State.GuardSuccess;
            skillGuarded = true;
            oponentSkillDelay = FrameManager.nowFrame + skill.againstGuardFrame + leftActiveFrame;  //내 후딜은 얼마냐
            //가드 스프라이트
            spriteChangeGab[7] = (oponentSkillDelay - FrameManager.nowFrame) / guardSpriteArr.Length;
            nowSpriteArr = guardSpriteArr;
            nowGabIndex = 7;
            skillKnockBack = new Vector2(-1 * one * skill.xGuardBack, skill.yGuardBack);
            spriteChangeCounter = spriteChangeGab[7];
            playerSound.SoundPlay(nowGabIndex, false);
        }
        else
        {
            damagedHP = skill.damage * oponentBuffRatio;
            nowHealthPoint -= damagedHP;
            nowUltPoint += damagedHP * 0.4f;
            if (nowUltPoint > ultPoint)
            {
                nowUltPoint = ultPoint;
            }
            skillKnockBack = new Vector2(-1 * one * skill.xKnockBack, skill.yKnockBack);
            if (skill.fallDown == true)
            {
                if (skill.yKnockBack == 0)
                {
                    oponentSkillDelay = FrameManager.nowFrame + 65;//이거 하드코딩. 넘어졌을때의 후딜은 65
                    invulnerableFrame = FrameManager.nowFrame + 70;
                    StartCoroutine(InvulnerableCor());  //기획상으로 코루틴으로 무적 해제해줘야함

                }
                else
                {
                    //위로 뜨는 FallDown
                    oponentSkillDelay = FrameManager.nowFrame + fallDownInvulnerableFrame; //이거 하드코딩. 50초이상 떠있으면 문제니까 되돌리기다.
                    invulnerableFrame = FrameManager.nowFrame + fallDownInvulnerableFrame + 20; //이거는 20프레임 더 있어얗마
                    StartCoroutine(InvulnerableCor());  //기획상으로 코루틴으로 무적 해제해줘야함
                    liftedFallDown = true;
                    //얘는 땅에 닿고나서 무적판정이랑 후딜 실행해야한다.

                }
                nowState = State.FallDown;
                //넘어지는 스프라이트
                nowSpriteArr = fallDownSpriteArr;
                nowGabIndex = 6;
                playerSound.SoundPlay(nowGabIndex, false);
                spriteChangeCounter = spriteChangeGab[6];

            }
            else
            {
                oponentSkillDelay = FrameManager.nowFrame + skill.againstHitFrame + leftActiveFrame;
                nowState = State.Beaten;
                //단순 피격스프라이트
                spriteChangeGab[5] = (oponentSkillDelay - FrameManager.nowFrame) / beatenSpriteArr.Length;
                nowSpriteArr = beatenSpriteArr;

                nowGabIndex = 5;
                playerSound.SoundPlay(nowGabIndex, false);
                spriteChangeCounter = spriteChangeGab[5];

            }
        }
        //skillKnockBack = basePosition + skillKnockBack;     lerp는 쓰지않는다.
        // positionChangeLerp = oponentSkillDelay - FrameManager.nowFrame;
        spriteCount = nowSpriteArr.Length;
        if (skill.followingDamage == -1)
        {
            //2타성 기술이 아닐 때에만 넉백해준다.
            rigidBody.velocity = Vector2.zero;
            rigidBody.AddForce(skillKnockBack, ForceMode2D.Impulse);        //넉백해준다.
        }
        callEveryFrame = SkillBeaten;

        return damagedHP;
    }


    //가드, 피격, 넘어짐 모두 여기서 담당한다. 모션은 모두 후딜시간에 발동한다.

    void SkillBeaten()
    {
        // if (invulnerable == true&& nowState == State.Beaten && FrameManager.nowFrame > invulnerableFrame)
        //원래 nowState == State.Beaten이 있었는데, Guard때도 invulnerable이 있어서 뺴줬다
        if (invulnerable == true && FrameManager.nowFrame > invulnerableFrame)
        {
            invulnerable = false;
        }
        if (FrameManager.nowFrame > oponentSkillDelay)
        {
            IdleStart();
            return;
        }
        /*
         * 이거 중력적용되가지고 안쓴다.
        transform.position = Vector2.Lerp(basePosition, skillKnockBack,lerpCounter/positionChangeLerp);
        lerpCounter++;*/
        SpriteChanger();

        if (liftedFallDown && nowSpriteIndex >2)
        {
            nowSpriteIndex = 2;
            spriteRenderer.sprite = nowSpriteArr[2];   //동훈이가 이렇게 하래요...
            //하늘에 떠있을때는 강제로 2다.
        }
    }

    IEnumerator InvulnerableCor()
    {
        //FallDown일 때 일어나서도 invulnerable이 되어야함.
        while(FrameManager.nowFrame < invulnerableFrame-1)
        {
            yield return null;
        }
        invulnerable = false;
    }


    bool corRunning;
    IEnumerator CommandClearCor()
    {
        corRunning = true;
        //어느 프레임에서 커맨드를 클리어 해 줄지 결정해준다.
        inputBool = false;
        while (inputStartFrame > FrameManager.nowFrame)
        {
            // 클리어 시간보다 시간이 더 지났으면 커맨드 누적 클리어. 여기서 클리어 프레임은 모션에서의 InputDelay로 정해준다.
            yield return null;
        }
        inputBool = true;
        //이거 심각한 오류를 만들거같은데, 예의주시해보자.
        while (clearFrame > FrameManager.nowFrame)
        {
            // 클리어 시간보다 시간이 더 지났으면 커맨드 누적 클리어. 여기서 클리어 프레임은 모션에서의 InputDelay로 정해준다.
            yield return null;
        }

        latestCommand.Clear();
        Debug.Log("클리어");
        clearFrame = 0;
        corRunning = false;
    }

    //버프시간 재어주는 코루틴
    IEnumerator buffCor() //StartFrame은 FrameManager에 더해진 값이다
    {   
        //스킬 실행해줄때 코루틴 실행해준다.
        
        buffed = true;
        while(FrameManager.nowFrame <= buffFrame)
        {
            yield return null;
        }
        buffed = false;
        buffRatio = 1.0f;
    }

    void OnCollisionEnter2D(Collision2D col)
    {

        if (col.gameObject.CompareTag("Player"))
        {
            return; //플레이어끼리 닿으면 무효
        }
        if (jumping == true)
        {
            jumping = false;
            if(nowState == State.Jump)
            {
                if (!jumpCorRunning)
                {
                    StartCoroutine(JumpEndCor());   //착지모션 해주는거.
                }
            }
         
        }

        if (liftedFallDown)
        {
            liftedFallDown = false;
            Debug.Log(5 * (spriteCount - nowSpriteIndex + 2));
            oponentSkillDelay = FrameManager.nowFrame + 5*(spriteCount - nowSpriteIndex + 2); //이제 원래 후딜인 10장짜리 50프레임을 실행시켜야한다
            invulnerableFrame = FrameManager.nowFrame + 5 * (spriteCount - nowSpriteIndex + 2) + 20; //무적시간도 해준다

        }

    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            return; //플레이어끼리 닿으면 무효
        }
        collisionPlatform = col.gameObject;
        /*
         * 이부분은 플랫폼에서 떨어질 때 점핑모션이 나오게 하는것인데, 하지말라고한다.
        if(jumping == true)
        {
            //점프하면서 나간거는 문제가없다.
            return;
        }
        else
        {
            jumping = true;
            JumpStart();
        }*/
    }


    //FrameManager에서 호출
    public void GameOverStart(bool victory)
    {
        rigidBody.velocity = Vector2.zero;
        if (victory)
        {
            playerSound.SoundPlay(10, false);
            StartCoroutine(VictoryCor());
        }
        else
        {
            playerSound.SoundPlay(9, false);
            LoseStart();
        }
    }

    public void LoseStart()
    {

        nowSpriteArr = fallDownSpriteArr;
        nowGabIndex = 6;
        spriteChangeCounter = spriteChangeGab[6];
        spriteCount = nowSpriteArr.Length - 3;
        nowSpriteIndex = 0;
        nowState = State.GameEnd;
       
        callEveryFrame = GameOver;
    }

    public void GameOver()
    {
        SpriteChanger();
        if (nowSpriteIndex == spriteCount)
        {
            nowState = State.Ready;
            callEveryFrame = DoNothing; //애니메이션 멈추기용
            frameManager.Restart(playerNum - 1);
            //여기에 겜끝나면 나오는 무언가가 있겟지 뭐;
        }

    }
    
    public void DoNothing()
    {
        //애니메이션 호출 안해야 될 때, CallEveryFrame에 null을 집어넣고 싶다. 그런데 null넣고 돌리면 NullReference뜨니까, 이렇게한다
    }

    IEnumerator VictoryCor()
    {
        int waiting = 0;
        while (waiting < 120)
        {
            waiting++;
            yield return null;
        }
        rigidBody.velocity = Vector2.zero;
        nowSpriteArr = victorySpriteArr;
        nowGabIndex = 10;
        spriteChangeGab[10] = 3;
        spriteChangeCounter = spriteChangeGab[10];
        spriteCount = nowSpriteArr.Length;
        nowSpriteIndex = 0;
        nowState = State.GameEnd;
        callEveryFrame = GameOver;

    }



}
