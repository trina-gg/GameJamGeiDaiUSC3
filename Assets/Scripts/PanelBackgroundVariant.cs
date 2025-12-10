// PanelBackgroundVariant.cs
using UnityEngine;

/// <summary>
/// 负责切换某个背景 SpriteRenderer 的差分图，
/// 比如：
/// - 默认是“有火箭”的背景
/// - 拾取火箭后切到“没有火箭”的背景
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PanelBackgroundVariant : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("默认背景（有火箭的那张）。可以留空，自动用当前 sprite。")]
    public Sprite defaultSprite;

    [Tooltip("拾取火箭后要切换到的“没有火箭”的背景")]
    public Sprite withoutRocketSprite;

    SpriteRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();

        // 如果没手动指定 defaultSprite，就用当前的 sprite 当默认
        if (defaultSprite == null && _renderer != null)
        {
            defaultSprite = _renderer.sprite;
        }
    }

    /// <summary>
    /// 切回默认背景（有火箭的版本）。
    /// </summary>
    public void SwitchToDefault()
    {
        if (_renderer != null && defaultSprite != null)
        {
            _renderer.sprite = defaultSprite;
        }
    }

    /// <summary>
    /// 切到“没有火箭”的差分图。
    /// </summary>
    public void SwitchToWithoutRocket()
    {
        if (_renderer != null && withoutRocketSprite != null)
        {
            _renderer.sprite = withoutRocketSprite;
        }
    }
}
