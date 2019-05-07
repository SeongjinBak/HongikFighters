using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightSceneManager : MonoBehaviour {

    public List<GameObject> mapList;
    // isDebbuging이 true 면, 디버깅용 홍문관 맵 뜸. 
    // 그 외엔 GM에서 받은 맵 이름과 일치하는 오브젝트 활성화
    public bool isDebugging;

	// Use this for initialization
	void Start () {


        if (!isDebugging)
            if (mapList != null)
            {
                foreach (var item in mapList)
                {
                    if (item.name == GameManager.instance.mapName)
                    {
                        item.SetActive(true);
                    }
                    else
                    {
                        item.SetActive(false);
                    }
                }


            }
	}
	
	
}
