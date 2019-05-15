using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FightSceneManager : MonoBehaviour {

    public List<GameObject> mapList;
    // isDebbuging이 true 면, 디버깅용 홍문관 맵 뜸. 
    // 그 외엔 GM에서 받은 맵 이름과 일치하는 오브젝트 활성화
    public bool isDebugging;

    // HP bar
    public Image[] hpBar;
    // ULT bar
    public Image[] ultBar;
    // Victory Icons
    public Image[] vic1P;
    public Image[] vic2P;

    // 카페나무, 운동장 맵에서는 Floor를 끈다.
    public SpriteRenderer floor;

    // 프레임매니저에서 승패 받아옴
    private FrameManager fm;
    // 1p, 2p 플레이어
    private Player p1, p2;
    // 뒷배경
    private SpriteRenderer backGround;
    // Use this for initialization
    void Start () {
        // 맵 정보 세팅함. GM에 저장되어 있는 그대로 받아옴.
        // 플랫폼 세팅

        backGround = GameObject.Find("BATTLE_MAP").GetComponent<SpriteRenderer>();

        if (!isDebugging)
        {
            if (mapList != null)
            {
                foreach (var item in mapList)
                {
                    if ("map_" + item.name == GameManager.instance.mapName)
                    {
                        item.SetActive(true);
                    }
                    else
                    {
                        item.SetActive(false);
                    }
                    
                }
            }
            // 뒷배경 세팅
            Sprite battleMap = Resources.Load<Sprite>("BackGroundMap/" + GameManager.instance.mapName);
            backGround.sprite = battleMap;
        }
        // 프레임매니저 저장.
        fm = GameObject.Find("FrameManager").GetComponent<FrameManager>();
        // 플레이어 스크립트 저장
        p1 = GameObject.Find("Player1").GetComponent<Player>();
        p2 = GameObject.Find("Player2").GetComponent<Player>();

        TurnOffFloor();
    }

    private void Update()
    {
        CheckVictory();
        UltImage();
        HpImage();
    }

    public void TurnOffFloor()
    {
        if (GameManager.instance.mapName == "map_Cafenamu" || GameManager.instance.mapName == "map_Playground")
        {
            floor.enabled = false;
        }
    }

    // 체력 정보를 포인트에 맞게 출력
    void HpImage()
    {
        hpBar[0].fillAmount = p1.NowHpGaugeInfo();
        hpBar[1].fillAmount = p2.NowHpGaugeInfo();
    }
    // 궁극기 포인트 정보에 맞게 화면에 출력
    void UltImage()
    {
        ultBar[0].fillAmount = p1.NowUltGaugeInfo();
        ultBar[1].fillAmount = p2.NowUltGaugeInfo();
    }

    // 승리 아이콘을 승리 횟수에 맞게 화면에 출력
    void CheckVictory()
    {
        switch (fm.RoundCount(1))
        {
            case 1:
                vic1P[0].enabled = true;
                break;
            case 2:
                vic1P[1].enabled = true;
                break;
            case 3:
                vic1P[2].enabled = true;
                break;
        }

        switch (fm.RoundCount(2))
        {
            case 1:
                vic2P[0].enabled = true;
                break;
            case 2:
                vic2P[1].enabled = true;
                break;
            case 3:
                vic2P[2].enabled = true;
                break;
        }
    }


}
