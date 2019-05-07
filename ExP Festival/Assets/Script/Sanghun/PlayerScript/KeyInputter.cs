using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyInputter : MonoBehaviour {
    protected KeyCode[] playerKeys;   //1P 2P에 따른 키의 배치. I,O,P, Left,Down,Right,Up 이 순서대로 0 1 2 3 4 5 6
    public int playerNum;   //게임매니저에서 건드려야하니까. 플레이어 넘.

    public void KeyRefresh()
    {
     
        if(playerNum == 1)
        {
            playerKeys = new KeyCode[7]
            {
                KeyCode.C,
                KeyCode.V,
                KeyCode.B,
                KeyCode.A,
                KeyCode.S,
                KeyCode.D,
                KeyCode.W
            };
        }
        else
        {

            playerKeys = new KeyCode[7]
            {
                KeyCode.I,
                KeyCode.O,
                KeyCode.P,
                KeyCode.LeftArrow,
                KeyCode.DownArrow,
                KeyCode.RightArrow,
                KeyCode.UpArrow
            };
        }
    }

    
}
