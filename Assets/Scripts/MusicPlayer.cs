using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    public TextMeshProUGUI musicNameText;
    public GameObject MusicPlayerUI;
    public AudioSource audioSource;
    public AudioClip[] musicClips;
    private int currentTrackIndex = 0;
    private bool isPlaying = false;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if(musicNameText == null)
        {
            musicNameText = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        // 自动播放下一首检测
        if (isPlaying && !audioSource.isPlaying && Mouseclick.IS_MUSIC_OPEN)
        {
            PlayNextTrack();
        }
        musicNameText.text = "当前播放的是:"+musicClips[currentTrackIndex].name;
    }

    /// <summary>
    /// 播放当前索引指向的音频
    /// </summary>
    public void PlayCurrentTrack()
    {
        if (musicClips.Length == 0) return;

        audioSource.clip = musicClips[currentTrackIndex];
        if (!audioSource.isPlaying && Mouseclick.IS_MUSIC_OPEN)
        {
            audioSource.Play();
            isPlaying = true;
        }
    }

    // 提供给UI按钮调用的方法
    public void PlayNextTrack()
    {
        if (musicClips.Length == 0) return;

        currentTrackIndex++;
        if (currentTrackIndex >= musicClips.Length)
        {
            currentTrackIndex = 0;
        }
        PlayCurrentTrack();
    }

    public void PlayPreviousTrack()
    {
        if (musicClips.Length == 0) return;

        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = musicClips.Length - 1;
        }
        PlayCurrentTrack();
    }

    public void PauseMusic()
    {
        audioSource.Pause();
        isPlaying = false;
    }

    public void ResumeMusic()
    {
        if (Mouseclick.IS_MUSIC_OPEN)
        {
            audioSource.UnPause();
            isPlaying = true;
        }
    }

    public void StopMusic()
    {
        audioSource.Stop();
        isPlaying = false;
    }

    // 新增：当音乐开关状态变化时调用
    public void OnMusicToggleChanged()
    {
        if (Mouseclick.IS_MUSIC_OPEN)
        {
            // 音乐打开时，如果没有在播放就开始播放
            if (!audioSource.isPlaying)
            {
                PlayCurrentTrack();
            }
        }
        else
        {
            // 音乐关闭时，停止播放
            StopMusic();
        }
    }
}
