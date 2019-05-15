/*
 * 작성자 : 백성진
 * 캐릭터 선택창 매니저 스크립트
 * 
 * 왼쪽, 위쪽 키 눌림 => 왼쪽으로 캐릭터 선택 프레임 이동. 우측, 아래쪽키 => 우측으로 이동.
 * 선택 버튼은, 1p : B  |  2p : I 임. 
 * 
 * pointer 의 숫자에 따라서, 선택되는 동아리가 다르므로 꼭 순서 숙지하고 코딩하기 바랍니다!.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour {

    // 각 캐릭터들이 선택 되었는지 여부를 알려줄 변수.
    private bool[] isSelected;
    // 1p, 2p 캐릭터 선택 포인터
    private int player1_pointer;
    private int player2_pointer;
    // 1p, 2p가 선택한 캐릭터
    private int player1_choice;
    private int player2_choice;
    // 1p, 2p 캐릭터 선택 완료 여부
    private bool player1_complete;
    private bool player2_complete;
    // 1p or 2p가 선택한 동아리를 나타내는 액자
    private List<GameObject> player1_frame = new List<GameObject>();
    private List<GameObject> player2_frame = new List<GameObject>();
    // 액자 배열
    public GameObject[] group;
    // 액자 canvas 2개 , UI창에서의 layer 구분을 위함.
    public Canvas[] canvas;
    // 페이드 인을 표시할 이미지 (검정)
    public Image Fader;

    // 캐릭터 선택이 모두 완료 되었음을 나타내는 flag
    private bool isSelectionComplete;

    // 각 캐릭터들(동아리) 
    // Use this for initialization
    void Start () {

        // 배경음악 재생
        BackSoundManager.instance.ChangeBackGroundMusic(Resources.Load<AudioClip>("BackGroundMusic/pickScene"));
        StartCoroutine(FadeInSelectionSceneInitializer());
        
        
    }

	
	// Update is called once per frame
	void Update () {
        if(!isSelectionComplete && !GameManager.instance.forbidEveryInput)
            CheckKeyInput();
        CheckIsReady();
     //   Debug.Log(player1_pointer + "  " + player2_pointer);
     //   Debug.Log(isSelected[0] + " "+ isSelected[1]+ " "+ isSelected[2]+ " "+isSelected[3]);
    }

    // 페이드 인, 한 후 초기값 세팅
    IEnumerator FadeInSelectionSceneInitializer()
    {
        // 페이드 인 전까진 키 못누르게 한다.
        GameManager.instance.forbidEveryInput = true;

        isSelected = new bool[] { false, false, false, false };
        player1_pointer = 0;
        player2_pointer = isSelected.Length - 1;
        player1_complete = false;
        player2_complete = false;
        isSelectionComplete = false;
        // 액자 초기화
        FramesInitialize();

        player1_choice = 10;
        player2_choice = 10;

        // 0.6초에 걸쳐 씬을 페이드 아웃,
        float dividend = 0.015f;
        while (Fader.color.a >= 0)
        {
            Color color = Fader.color;
            color.a -= dividend;
            Fader.color = color;
            yield return new WaitForSecondsRealtime(0.01f);
        }
        canvas[2].sortingOrder = 0;
        // 페이드 인 후 키 누르게 함.
        GameManager.instance.forbidEveryInput = false;

    }
    // 동아리 가리키는 이미지(액자) 할당 및 초기화 함수
    void FramesInitialize()
    {
        // frame들을 받아 온 후, 1p와 2p 액자 리스트에 넣는 코드. 넣은 직후 모두 비활성화 시킨다. 
        if (group != null)
        {
            int p1 = 0; int p2 = 0;
            for (int i = 0; i < group.Length; i++)
            {
                if (group[i].name.Contains("-1"))
                {
                    player1_frame.Add(group[i]);
                    player1_frame[p1++].SetActive(false);
                }
                else if (group[i].name.Contains("-2"))
                {
                    player2_frame.Add(group[i]);
                    player2_frame[p2++].SetActive(false);
                }
                else
                {
                    Debug.Log("캐릭터 픽 액자 이름 오류!");
                }
            }
        }
        // 할당 직후 초기 포인터에 따라 액자 켜기
        player1_frame[player1_pointer].SetActive(true);
        player2_frame[player2_pointer].SetActive(true);
    }

    // 게임 준비 되었는지 판단하는 함수
    void CheckIsReady()
    {
        // 준비가 완료 되었다면.
        if(player1_complete && player2_complete)
        {
           

            // 1p와 2p가 모두 complete 되었을 때 게임 시작
           // Debug.Log("Selection complete!");
            if(isSelectionComplete == false)
            {
                isSelectionComplete = !isSelectionComplete;
                SetUnselectedClubColorBlack();
                StartCoroutine(NextSceneLoader());
                SetSelectedClubName();
                

            }
        }
    }
    // pointer에 따라 GM의 선택된 동아리 이름, 맵은 렌덤으로 변경 변경
    void SetSelectedClubName()
    {
        switch (player1_pointer)
        {
            case 0:
                GameManager.instance.player1_name = "ExP";
                break;
            case 1:
                GameManager.instance.player1_name = "Taekwon";
                break;
            case 2:
                GameManager.instance.player1_name = "Cowboys";
                break;
            case 3:
                GameManager.instance.player1_name = "Nefer";
                break;
        }
        switch (player2_pointer)
        {
            case 0:
                GameManager.instance.player2_name = "ExP";
                break;
            case 1:
                GameManager.instance.player2_name = "Taekwon";
                break;
            case 2:
                GameManager.instance.player2_name = "Cowboys";
                break;
            case 3:
                GameManager.instance.player2_name = "Nefer";
                break;
        }
        // 맵을 랜덤으로 설정
        int num = Random.Range(0, 4);
        string[] map = new string[4] { "HongikGate", "Playground", "EternalSmile", "Cafenamu" };
        GameManager.instance.mapName = "map_" + map[num];
    }
    // 준비 완료 시, 선택 안된 동아리는 회색처리
    void SetUnselectedClubColorBlack()
    {
        int [] p = new int[2] { -11, -11 };
        int pp = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i != player1_pointer && i != player2_pointer)
            {
                p[pp++] = i;
            }
        }
        string name = "";

        //Image image;
        for (int i = 0; i < p.Length; i++)
        {
            if(p[i] >= 0)
            {   /*
                name = "Candidate" + (p[i] + 1).ToString();
                image = GameObject.Find("/Canvas/PickList/" + name + "/Image").GetComponent<Image>();
                image.color = Color.gray;
                image = GameObject.Find("/Canvas/PickList/" + name).GetComponent<Image>();
                image.color = Color.gray;
                */
                name = "fade" + (p[i] + 1).ToString();
                GameObject.Find(name).GetComponent<Image>().enabled = true;

            }
        }
        
        
    }

    // 격투 씬으로 넘어갈 준비가 되었고, 캐릭터 애니메이션 전부 출력한 후 다음씬으로 넘김.
    // 이 코루틴에서 GM에 이미지 할당할 수도 있음.
    // 준비 완료! 라는 이미지도 띄울 수 있음.
    IEnumerator NextSceneLoader()
    {
        // 2초 기다림.
        yield return new WaitForSeconds(2f);
      
        // 도움말 씬으로 넘어감
        SceneManager.LoadScene("HelpScript");
    }

    // 버튼 탐지 하는 함수
    void CheckKeyInput()
    {
        // 1P의 입력 탐지
        if(player1_complete == false)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.W))
            {
                // 직전 액자 끄기
                player1_frame[player1_pointer].SetActive(false);
                if (player1_pointer - 1 < 0)
                {
                    player1_pointer = isSelected.Length - 1;
                }
                else
                {
                    player1_pointer = player1_pointer - 1;
                }
                // 직후 액자 켜기
                player1_frame[player1_pointer].SetActive(true);
                // 1p의 액자가 위로가게끔 순서 재조정
                ResetSortingOrder(1);
                // 사운드 재생
                SoundManager.instance.PlaySound(Resources.Load<AudioClip>("UiSound/pickMove"));
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                // 직전 액자 끄기
                player1_frame[player1_pointer].SetActive(false);

                player1_pointer = (player1_pointer + 1) % isSelected.Length;

                // 직후 액자 켜기
                player1_frame[player1_pointer].SetActive(true);
                // 1p의 액자가 위로가게끔 순서 재조정
                ResetSortingOrder(1);
                // 사운드 재생
                SoundManager.instance.PlaySound(Resources.Load<AudioClip>("UiSound/pickMove"));
            }
        }


        // 2P의 입력 탐지
        if (player2_complete == false)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                // 직전 액자 끄기
                player2_frame[player2_pointer].SetActive(false);
                if (player2_pointer - 1 < 0)
                {
                    player2_pointer = isSelected.Length - 1;
                }
                else
                {
                    player2_pointer = player2_pointer - 1;
                }
                // 직후 액자 켜기
                player2_frame[player2_pointer].SetActive(true);
                // 2p의 액자가 위로가게끔 순서 재조정
                ResetSortingOrder(2);
                // 사운드 재생
                SoundManager.instance.PlaySound(Resources.Load<AudioClip>("UiSound/pickMove"));
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                // 직전 액자 끄기
                player2_frame[player2_pointer].SetActive(false);

                player2_pointer = (player2_pointer + 1) % isSelected.Length;

                // 직후 액자 켜기
                player2_frame[player2_pointer].SetActive(true);
                // 2p의 액자가 위로가게끔 순서 재조정
                ResetSortingOrder(2);
                // 사운드 재생
                SoundManager.instance.PlaySound(Resources.Load<AudioClip>("UiSound/pickMove"));
            }
        }

        // 이하 코드는 캐릭터 선택 / 해제 버튼임.
        // 1P의 확인 버튼 탐지
        if (Input.GetKeyDown(KeyCode.C))
        {
            // pointer의 캐릭터가 선택 됨
            if(isSelected[player1_pointer] == false)
            {
                isSelected[player1_pointer] = true;
                player1_complete = true;
                // 선택되었을 시 이미지 재생
                PlayPickedAnim(player1_pointer);
                // 선택되었을 시 액자 깜빡임/ 사운드 효과
                StartCoroutine(PickedEffect(1));
                // 선택한 동아리 저장.
                player1_choice = player1_pointer;

            }  
            else
            {
                // 2p가 선택해서 true 된 isSelect를 1p가 변경하지 못하게 함.
                if(player2_choice != player1_pointer)
                {
                    isSelected[player1_pointer] = false;
                    player1_complete = false;
                    // 이미지 초기화
                    ImageInitialize(player1_pointer);
                    // 선택 동아리 초기화
                    player1_choice = 10;
                }
            }
            // 사운드 재생
            SoundManager.instance.PlaySound(Resources.Load<AudioClip>("UiSound/pickLock"));
        }
        // 2P의 확인 버튼 탐지
        if (Input.GetKeyDown(KeyCode.I))
        {
            // pointer의 캐릭터가 선택 됨
            if (isSelected[player2_pointer] == false)
            {
                isSelected[player2_pointer] = true;
                player2_complete = true;
                PlayPickedAnim(player2_pointer);
                // 선택되었을 시 액자 깜빡임/ 사운드 효과
                StartCoroutine(PickedEffect(2));
                player2_choice = player2_pointer;
            }
            else
            {
                if (player2_pointer != player1_choice)
                {
                    isSelected[player2_pointer] = false;
                    player2_complete = false;
                    // 이미지 초기화
                    ImageInitialize(player2_pointer);
                    // 동아리 초기화
                    player2_choice = 10;
                }
            }
            // 사운드 재생
            SoundManager.instance.PlaySound(Resources.Load<AudioClip>("UiSound/pickLock"));
        }
    }
    // 액자의 sorting order를 정하는 함수
    void ResetSortingOrder(int playerNum)
    {
        // 현재 움직인게 1p인경우
        if(playerNum == 1)
        {
            canvas[0].sortingOrder = 5;
            canvas[1].sortingOrder = 4;
        }
        // 현재 움직인게 2p인경우
        else if(playerNum == 2)
        {
            canvas[0].sortingOrder = 4;
            canvas[1].sortingOrder = 5;
        }
    }
    // 캐릭터가 선택 되었을 경우, 캐릭터 창이 '깜빡' 하는 효과
    IEnumerator PickedEffect(int p)
    {
        // 1p인 경우
        if(p == 1)
        {
            player1_frame[player1_pointer].SetActive(false);
            yield return new WaitForSeconds(0.1f);
            player1_frame[player1_pointer].SetActive(true);
        }
        // 2p인 경우
        else
        {
            player2_frame[player2_pointer].SetActive(false);
            yield return new WaitForSeconds(0.1f);
            player2_frame[player2_pointer].SetActive(true);
        }
    }

    // 선택 애니메이션 출력 함수 (Caller)
    void PlayPickedAnim(int pointer)
    {
        string name = "Candidate" + (pointer + 1).ToString();
        string fileName = "";
        switch (pointer)
        {
            case 0: fileName = "ExP"; break;
            case 1: fileName = "TaeKwon"; break;
            case 2: fileName = "Cowboys"; break;
            case 3: fileName = "Nefer"; break;
        }

        if (fileName == "") return;

        // 리스트에 이미지들 담고, 코루틴 내에서 재생. 
        List<Sprite> sprites = new List<Sprite>(Resources.LoadAll<Sprite>("MotionSprite/" + fileName + "/Picked"));
        Image image = GameObject.Find("/Canvas/PickList/" + name+"/Image").GetComponent<Image>();

        if (image.sprite.name == sprites[0].name)
        {
            StartCoroutine(PickedCoroutine(image, sprites, pointer));
        }
    }

    // Caller에 의해 불려진 애니메이션 재생 Callee 코루틴 
    IEnumerator PickedCoroutine(Image sr, List<Sprite> sprites, int pointer) 
    {
        int len = sprites.Count;
       
        for(int i = 1; i < len; i++)
        {
            // 애니메이션 재생 도중 선택이 취소 된 경우
            if (isSelected[pointer] == false)
            {
                ImageInitialize(pointer);
                break;
            }

            sr.sprite = sprites[i];

            yield return new WaitForSecondsRealtime(0.1f);
            
        }
    }

    // 선택 해제 된 경우 이미지를 초기화 한다.
    void ImageInitialize(int pointer)
    {
        string name = "Candidate" + (pointer + 1).ToString();
        string fileName = "";
        switch (pointer)
        {
            case 0: fileName = "ExP"; break;
            case 1: fileName = "TaeKwon"; break;
            case 2: fileName = "Cowboys"; break;
            case 3: fileName = "Nefer"; break;
        }

        if (fileName == "") return;

        Image image = GameObject.Find("/Canvas/PickList/" + name + "/Image").GetComponent<Image>();
        List<Sprite> sprites = new List<Sprite>(Resources.LoadAll<Sprite>("MotionSprite/" + fileName + "/Picked"));
       
        image.sprite = sprites[0];
    }
}
