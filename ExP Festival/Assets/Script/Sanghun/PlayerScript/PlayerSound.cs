using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound{

    AudioClip[] clipArray;
    AudioSource effectAudioSource;
    AudioSource voiceAudioSource;
    
    //0 1 2 3 4 5 6 7 8 9 10 GameStart Null Idle Move Jump Hit Falldown Guard Dash Lose 빅토리 이다.
    //번호 기준은 Player에서 사용한 모션 sprite의 기준이다. 그래서 사운드가 없는 0번과 8번은 비어있다
    //9번은 스킬 사용시마다 그 스킬의 오디오클립으로 갈아끼워준다.
    //스킬 관련 사운드는 0번에서 사용한다

    public void SoundLoad(string playerName, AudioSource effectAudio, AudioSource voiceAudio)
    {
        clipArray = new AudioClip[11];
        //폴더 안에 오디오클립을 넣을 필요가 없다. 어차피 하나니까.
        //다만 스킬은 문제가 되겠다. 스킬은 스킬 폴더 자체에 오디오클립을 넣어놓고, 그 클립을 오디오소스로 옮겨서 플레이하게 한다.

        clipArray[0] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/GameStart");
        //clipArray[2] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/Idle");
        clipArray[3] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/Move");
        clipArray[4] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/Jump");
        clipArray[5] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/Hitted");
        clipArray[6] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/FallDown");
        clipArray[7] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/Guard");
        clipArray[8] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/Dash");
        clipArray[9] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/Lose");
        clipArray[10] = Resources.Load<AudioClip>("PlayerSound/" + playerName + "/Victory");

        effectAudioSource = effectAudio;
        voiceAudioSource = voiceAudio;
    }

    //각 Player에서 사운드 플레이할 떄가 되면 이걸 켜준다
    //nowGabIndex를 사용한다.
    public void SoundPlay(int index, bool loop)
    {
        if (voiceAudioSource.isPlaying)
        {
            if(index == 5 || index == 6)
            {
                voiceAudioSource.Stop();
            }
        }
        effectAudioSource.Stop();
        effectAudioSource.clip = clipArray[index];
        effectAudioSource.loop = loop;
        effectAudioSource.Play();
    }

    public void SkillSoundPlay(AudioClip skillClip, int delayFrame)
    {
        //스킬의 클립을 받아서 실행한다.
        float delaySecond = delayFrame / 60f;
        effectAudioSource.clip = skillClip;
        effectAudioSource.loop = false;
        if (delaySecond == 0)
        {
            effectAudioSource.Play();
        }
        else
        {
            effectAudioSource.PlayDelayed(delaySecond);
        }
    }

    public void SkillVoicePlay(AudioClip voiceClip,int delayFrame)
    {
        //
        float delaySecond = delayFrame / 60f;
        voiceAudioSource.clip = voiceClip;
        voiceAudioSource.loop = false;
        if(delaySecond == 0)
        {
            voiceAudioSource.Play();
        }
        else
        {
            voiceAudioSource.PlayDelayed(delaySecond);
        }
        
    }

    public void StopLoop()
    {
        effectAudioSource.loop = false;
    }

}
