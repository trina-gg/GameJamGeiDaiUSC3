using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 简单的序列帧动画播放器：
/// - 把一组 Sprite 按固定帧率依次切换
/// - frameRate 越小，动画越慢；越大，越快
/// - 可以选择开局隐藏，只在 PlayOnce 的时候出现
/// - 可以通过 GetDuration() 拿到总时长，方便外部控制后续事件
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSequencePlayer : MonoBehaviour
{
    [Header("Frames")]
    [Tooltip("按顺序拖入每一帧 PNG")]
    public Sprite[] frames;

    [Tooltip("每秒播放多少帧。数值越小动画越慢，越大越快。\n例如：6 = 很慢，12 = 正常，24 = 偏快")]
    public float frameRate = 12f;

    [Header("Visibility")]
    [Tooltip("一开始是否隐藏 SpriteRenderer，只在 PlayOnce 时才出现")]
    public bool startHidden = true;

    [Tooltip("播放完成后是否自动隐藏 SpriteRenderer")]
    public bool hideOnComplete = true;

    [Header("Audio")]
    [Tooltip("Sound to play when animation starts")]
    public AudioClip soundEffect;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    SpriteRenderer _sr;
    AudioSource _audioSource;
    Coroutine _playRoutine;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();

        // Create audio source for sound effect
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (startHidden)
        {
            _sr.enabled = false;
        }
        else
        {
            if (frames != null && frames.Length > 0)
            {
                _sr.sprite = frames[0];
                _sr.enabled = true;
            }
        }
    }

    /// <summary>
    /// 从头播放一遍动画，播完调用 onComplete（可选）。
    /// </summary>
    public void PlayOnce(Action onComplete = null)
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
        }
        _playRoutine = StartCoroutine(PlayRoutine(onComplete));
    }

    IEnumerator PlayRoutine(Action onComplete)
    {
        if (frames == null || frames.Length == 0 || frameRate <= 0f)
        {
            onComplete?.Invoke();
            yield break;
        }

        float frameTime = 1f / frameRate;
        int index = 0;

        // 播放前确保可见
        _sr.enabled = true;

        // Play sound effect when animation starts
        if (soundEffect != null && _audioSource != null)
        {
            _audioSource.clip = soundEffect;
            _audioSource.volume = soundVolume;
            _audioSource.Play();
        }

        while (index < frames.Length)
        {
            _sr.sprite = frames[index];
            index++;
            yield return new WaitForSeconds(frameTime);
        }

        if (hideOnComplete)
        {
            _sr.enabled = false;
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// 返回完整播放一遍动画需要的时间（秒）。
    /// </summary>
    public float GetDuration()
    {
        if (frames == null || frames.Length == 0 || frameRate <= 0f)
            return 0f;

        return frames.Length / frameRate;
    }
}