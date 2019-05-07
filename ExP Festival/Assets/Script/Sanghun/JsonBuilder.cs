using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class JsonBuilder : MonoBehaviour {

    SkillJson[] skillArray;
    PlayerJson dumy;
    int playerNum;

	// Use this for initialization
	void Start () {
        //디버깅 때 쓰려고 만든 임시 스킬들
        skillArray = new SkillJson[7];
        for (int i = 0; i < 3; i++)
        {
            skillArray[i] = new SkillJson();
            char[] arr = new char[i + 1];

            skillArray[i].inputStart = 10;
            skillArray[i].inputEnd = 30;
            for (int j = 0; j < i + 1; j++)
            {
                arr[j] = 'z';
            }
            skillArray[i].command = new string(arr);
            skillArray[i].LoadSkill(skillArray[i].command, "Player" + playerNum.ToString());
            skillArray[i].triggeringIndex = 2;
            skillArray[i].fallDown = true;
        }

        for (int i = 3; i < 6; i++)
        {
            skillArray[i] = new SkillJson();
            char[] arr = new char[i - 2];
            skillArray[i].inputStart = 10;
            skillArray[i].inputEnd = 50;
            for (int j = 0; j < i - 2; j++)
            {
                arr[j] = 'x';
            }
            skillArray[i].command = new string(arr);
            skillArray[i].LoadSkill(skillArray[i].command, "Player" + playerNum.ToString());
            skillArray[i].triggeringIndex = 2;
        }
        dumy = new PlayerJson();
        dumy.skillJsonArray = skillArray;

        string text = JsonUtility.ToJson(dumy,true);
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + "/dumy.txt");
        writer.Write(text);
        writer.Close();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
