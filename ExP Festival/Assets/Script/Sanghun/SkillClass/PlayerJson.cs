using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerJson{

    public string playerName;    //로드에 쓰려고. 플레이어 넘버는 키 인푸터에 있다.
    public float hitPoint;  //체력
    public float moveSpeed; //이속
    public float jumpForce;     //점프 힘
    public float xDashForce;
    public float yDashForce;    //대시가 얼만큼?
    public float xJumpDashForce;
    public float yJumpDashForce;    //대시가 얼만큼?
    public float downDashForce;     //밑대쉬 얼만큼

    public int dashFrameLength;  //몇프레임동안 대시를 보여주고 무브로 건너뛸지
    public int dashInputFrame;   //대시가 몇프레임안에 눌려야 하는지


    //가드에 드는 동작의 트리거 인덱스

    //각 스프라이트가 몇 프레임마다 업데이트 될지의 갭
    //이 아래에 있는 것들의 애니메이션 지속시간은 (frame * 이미지의 개수)임
    //아래 것들은 연속적인 거여서 이걸 해줘야함. 대쉬같은거는 1번보여주고 끝이니까 상관x
    //가드는 모든캐릭터 선딜이 일정하고 그 이후로는 같은동작이니까 안 넣는다
    public int moveFrameGap;
    public int idleFrameGap;
    public int jumpFrameGap;
    public int hittedFrameGap;
    public int guardFrameGap;
    public int fallDownFrameGap;
    public int startMotionFrameGap;
    public int victoryFrameGap;



     
    public SkillJson[] skillJsonArray;   //모든 스킬들의 배열

    public PlayerJson()
    {
        playerName = "Nefer";
        moveSpeed = 0.3f;
        jumpForce = 13;
        xDashForce = 13;
        yDashForce = 0;
        dashFrameLength = 10;
        dashInputFrame = 20;

        moveFrameGap = 6;
        idleFrameGap = 6;
        jumpFrameGap = 6;
        hittedFrameGap = 6;
        fallDownFrameGap = 6;

    }

    public PlayerJson(string name)
    {
        playerName = name;
        moveSpeed = 0.3f;
        jumpForce = 13;
        xDashForce = 13;
        yDashForce = 0;
        dashFrameLength = 10;
        dashInputFrame = 20;

        moveFrameGap = 6;
        idleFrameGap = 6;
        jumpFrameGap = 6;
        hittedFrameGap = 6;
        fallDownFrameGap = 6;

    }


}
