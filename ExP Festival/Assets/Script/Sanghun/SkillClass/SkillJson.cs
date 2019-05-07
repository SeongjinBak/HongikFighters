using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillJson  {
    //프레임단위로 끊으니까, 딜레이도 다 int다
    //public string character;            //어느 캐릭터가 사용하는지. 이걸 토대로 폴더이름 찾아야 하니 string으로 고정.
    public int skillType;               //공격기술이라면 0, 비공격기술이라면 1(버프기, 이동기 등등)
                                        // skillType이 1이면 히트관련된 것들은 모두 0처리 하면 됨.
                                        //만약에 버프 이동기를 넣고싶다! 하면 이동프레임을 늘리면 됨.

    public int inputStart;              //다음 커맨드 입력을 기술발동 이후 몇프레임부터 입력할 수 있게 할건지.
                                        //연속기 입력하면 바로 스프라이트가 변하므로, 후딜 시간에 입력하는게 편함.
    public int inputEnd;                //스킬 입력 시간을 언제 끝낼것인지. 연속기여도 단일기여도 필요.
    //inputStart와 inputEnd는 스킬 시전 즉시부터 프레임을 샌다.
    //ex)inputStart = 25; inputEnd = 30; 이라고 하면, 스킬 발동 이후 25프레임부터 30프레임 사이에만 입력을 받는다.


    public int startFrame;              //선딜.       기준은 스킬 발동 이후 선딜
    public int activeFrame;             //발동프레임, 이 시간에 범위에 있는지 체크. 선딜 이후 몇프레임 기다리는지.
    public int endFrame;                //후딜    발동프레임 이후 몇프레임 기다리는지.
    public int againstGuardFrame;       //상대의 가드딜레이 가드딜레이는, 발동프레임 이후 몇 프레임이나 기다릴지.
    public int againstHitFrame;         //상대의 타격딜레이 타격딜레이 또한 같다.
   
    //InputDelay는 후속타 입력을 위해 쓰인다. InputDelay가 0이라면 키를 누르자마자 바로 다음 키를 눌렀을 때, 연속기로 입력이 되는것이다.
    //InputDelay가 10이라면, 10프레임동안 키 입력 받는것을 멈춘다. 그러면 연속기를 쓰기는 힘들 것이다. 

    public float xHitbox;            //히트박스의 수평판정 시작위치         히트박스가 어느 장소에서 시작하는지
    public float yHitbox;            //히트박스의 수직판정 시작위치         위와 동일
    public float xRange;            //히트박스의 x범위                  히트박스의 수평 범위
    public float yRange;            //히트박스의 y범위                  히트박스의 수직 범위
    public float damage;                //스킬의 데미지                    음수면 자신 체력회복
    public float xGuardBack;             //스킬의 가드백                    음수면 당겨옴, 0이면 넘어지기.
    public float yGuardBack;             //스킬의 가드백                    음수면 당겨옴, 0이면 넘어지기.
    public float xKnockBack;             //스킬의 넉백                      히트되었을 때의 넉백. 음수면 당겨옴.
    public float yKnockBack;             //스킬의 넉백                      히트되었을 때의 넉백. 음수면 당겨옴.
    public float xHitboxMove;         //히트박스의 수평이동 범위.       히트박스가 발동시간동안 얼만큼 수평이동하는지
    public float yHitboxMove;         //히트박스의 수직이동 범위.       위와 동일
    public float xMoveDistance;       //캐릭터의 수평이동 범위          캐릭터가 발동시간동안 얼만큼 수평이동 하는지.
    public float yMoveDistance;       //캐릭터의 수직이동 범위          동일. 스킬로 점프 한다는 뜻.

    public int buffFrameLength;   //버프되는 지속시간
    public float buffedDamage;      //공격력이 버프되는 정도. 1.5처럼 주면 됨.
    //버프는 skillType이 1일떄만 사용가능. skillType이 1이면 히트관련된 것들은 모두 0처리 하면 됨.


    public bool fallDown;               //스킬 타격 이후 넘어지는지, 안넘어지는지.
    public bool projectileEffect;       //스킬에 투사체 이펙트가 있는지
    public int voiceFrame;              //스킬 목소리 사운드가 몇프레임 이후에 나올건지
    public int soundEffectFrame;        //스킬 사운드이펙트가 몇프레임 이후에 나올건지

    public string command;              //스킬의 커맨드       스킬 이름 대신 커맨드로 인식.
    //ZZZ을 커맨드로 넣고싶다면, Z 1타라면 Z만 넣고, Z 2타는 ZZ넣고, Z 3타면 ZZZ 넣으세요.
    //Z 2타에는 Z 1타의 정보가 일체 들어가지 않아도 괜찮습니다.
    //기본 이름은 Idle, Move, Guard, FallDown, Jump 있습니다.


    public int triggeringIndex;   //타격 시의 이미지는 몇번? 0부터 샌다.  궁극기의 발동 시작 번호.
    public int endIndex;         //후딜 발동시의 이미지는 몇번? 궁극기라면 타격 못했을 때의 번호
                                 //트리거 인덱스가 절.대.로 0이면 안된다. 적어도 1이어야한다.
    public int ultTriggeringIndex;  //궁에서 타격시 실행되는 모션의 번호.
    public int ultEndIndex;         //궁에서 타격시 실행되는 모션의 후딜 번호.

    public int ultActiveFrame;      //궁의 발동프레임.
    public int ultEndFrame;         //궁의 후딜.z

    public float followingDamage;         //후속타의 데미지. 이게 -1이면 후속타가 없는것이다.
    public int damageDelayFrame;        //투사체에 맞은 후에 몇프레임 후에 데미지가 들어가는지

    public bool hittedEffect;       //타격모션이 있는지
                                    /*
                                     * 궁은 모션이 2가지이다. 초반모션은 일반스킬과 같다고 생각하면 된다.
                                일반스킬은 선딜 -> 발동프레임 -> 후딜이다
                                궁은 분기가 일어난다
                                (1)선딜->발동프레임 ->(미 타격시)후딜 혹은
                                (2)선딜->발동프레임 ->(타격시)타격프레임->타격후딜이다
                                1. triggeringIndex : 궁의 발동프레임이 일어나는 시점의 sprite번호
                                2. endIndex : 궁의 미타격시 후딜이 일어나는 시점의 sprite번호
                                3. ultTriggeringIndex : 궁의 발동프레임 중, 타격이 일어났을 때, 시작해야하는 스프라이트의 번호.
                                4. ultEndIndex : 궁이 타격하고, 타격프레임이 끝나고 타격후딜이 일어날 때의 스프라이트 번호
                                5. ultActiveFrame : 궁의 타격프레임이 몇프레임인지
                                6. ultEndFrame : 궁의 타격후딜이 몇프레임인지
                                */
                                    //이 위의 정보들은 json파일로 주셔야 합니다. 아래의 sprite[]는 공란으로 해주셔도 됩니다. 

    public AudioClip soundEffectClip;
    public AudioClip voiceClip;
    public Sprite[] spriteArray;            //모션의 이미지들
    public Sprite[] hittedSpriteArray;      //타격모션

    public SkillJson()
    {
        skillType = 0;
        inputStart = 0;
        inputEnd = 0;
        buffFrameLength = 0;
        buffedDamage = 1;
        triggeringIndex = 2;
        endIndex = 4;


        startFrame = 10;
        activeFrame = 10;
        endFrame = 30;
        againstGuardFrame = 30;
        againstHitFrame = 30;

        xHitbox = 1;
        yHitbox = 0;
        xRange = 3;
        yRange = 3;
        damage = 1;
        xGuardBack = 2;
        yGuardBack = 0;
        xKnockBack = 5f;
        yKnockBack = 0;
        xHitboxMove = 10;
        yHitboxMove = 0;
        xMoveDistance = 5;
        yMoveDistance = 0;

        //moveDistance = 0.3f;
        fallDown = false;

    }

    public void LoadSkill(string com, string name)
    {
        command = com;
        spriteArray = Resources.LoadAll<Sprite>("MotionSprite/" + name + "/" + com);
        /*
        voiceFrame = startFrame + activeFrame;    //디버그용
        soundEffectFrame = startFrame;
        */
        if(followingDamage == 0)
        {
            followingDamage = -1;
        }
        soundEffectClip = Resources.Load<AudioClip>("PlayerSound/" + name + "/SkillSound/" + com);
        if (voiceFrame != -1)
        {
            voiceClip = Resources.Load<AudioClip>("PlayerSound/" + name + "/SkillVoice/" + com);
        }

        if (hittedEffect)
        {
            hittedSpriteArray = Resources.LoadAll<Sprite>("SkillHittedSprite/" + name + "/" + com);
        }
    }

}
