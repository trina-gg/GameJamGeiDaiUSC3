using System.Collections;
using UnityEngine;

/// <summary>
/// 挂在场景里的“插槽/接收点”上：
/// - acceptsItemId 决定接收哪一个物品
/// - showPlacedItem 控制道具本体是不是留下来（火箭可以关掉）
/// - playAnimationBeforeZoom + sequencePlayer：先播放动画，再 ZoomToPanel
/// - delayBeforeAnimation / delayBeforeZoomAfterAnimation 用于控制节奏
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemReceiver : MonoBehaviour
{
    [Header("Receiver Settings")]
    [Tooltip("要求匹配的物品 ID，对应 InventoryItem.itemId")]
    public string acceptsItemId;

    [Tooltip("物体放下时在本地坐标中的位置（相对于本对象）")]
    public Vector3 localPlaceOffset = Vector3.zero;

    [Tooltip("是否覆盖物体原来的 localScale")]
    public bool overrideLocalScale = false;
    public Vector3 localScaleOverride = Vector3.one;

    [Header("Placed Item Visual")]
    [Tooltip("放置成功后是否让道具本体留在场景里显示。\n一般拼图勾上；像“火箭有专门过场动画”的情况可以关掉。")]
    public bool showPlacedItem = true;

    [Header("Optional: 解谜后跳转到下一个画面")]
    public SpritePanel nextPanel;     // 比如黑洞的 SpritePanel
    public float zoomDuration = 1.0f;

    [Header("Optional: 放对后先播放一段动画")]
    [Tooltip("火箭飞向黑洞这种逐帧动画播放器。可为空（不需要动画）。")]
    public SpriteSequencePlayer sequencePlayer;

    [Tooltip("如果勾上，则先播放 sequencePlayer，再在动画结束后 ZoomToPanel。")]
    public bool playAnimationBeforeZoom = false;

    [Tooltip("放下物品后，等待多少秒再开始播放动画（可选）。")]
    public float delayBeforeAnimation = 0f;

    [Tooltip("动画播放完成后，再等待多少秒才开始 ZoomToPanel。")]
    public float delayBeforeZoomAfterAnimation = 0f;

    public virtual bool TryPlaceItem(InventoryItem item)
    {
        if (item == null) return false;

        // 判断物品是否正确
        if (!string.IsNullOrEmpty(acceptsItemId) &&
            !string.Equals(item.itemId, acceptsItemId))
        {
            return false;
        }

        // 1）放置成功时道具本体如何处理
        Vector3 targetLocalPos = localPlaceOffset;
        Vector3? scale = overrideLocalScale ? (Vector3?)localScaleOverride : null;

        if (showPlacedItem)
        {
            // 普通拼图：道具留在场景中显示（作为这个 Receiver 的子物体）
            item.ShowInWorld(transform, targetLocalPos, scale);
        }
        else
        {
            // 像火箭这种有单独动画的情况：道具本体干脆不显示
            item.HideInWorld();
        }

        // 2）处理动画 + 转场
        if (playAnimationBeforeZoom && sequencePlayer != null)
        {
            // 用一个协程把：“等一会 → 播动画 → 再等一会 → Zoom” 串起来
            StartCoroutine(PlayAnimationThenZoom());
        }
        else
        {
            // 没有动画/不需要动画：直接 Zoom
            DoZoomToNextPanel();
        }

        return true;
    }

    IEnumerator PlayAnimationThenZoom()
    {
        // （可选）drop 后先停一会再开始动画
        if (delayBeforeAnimation > 0f)
        {
            yield return new WaitForSeconds(delayBeforeAnimation);
        }

        // 确保动画物体可见（SpriteSequencePlayer 会在 PlayOnce 时启用 SpriteRenderer）
        if (!sequencePlayer.gameObject.activeSelf)
            sequencePlayer.gameObject.SetActive(true);

        var sr = sequencePlayer.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
        }

        // 播放动画（不在这里等回调，而是用 GetDuration 算总时长）
        sequencePlayer.PlayOnce();

        float animDuration = sequencePlayer.GetDuration();
        if (animDuration > 0f)
        {
            yield return new WaitForSeconds(animDuration);
        }

        // 动画结束后再多等一会（如果你想要一点停顿感）
        if (delayBeforeZoomAfterAnimation > 0f)
        {
            yield return new WaitForSeconds(delayBeforeZoomAfterAnimation);
        }

        // 然后才开始摄像机缩放到下一幅画面
        DoZoomToNextPanel();
    }

    protected void DoZoomToNextPanel()
    {
        if (nextPanel != null && SpritePanelManager.Instance != null)
        {
            SpritePanelManager.Instance.ZoomToPanel(nextPanel, zoomDuration);
        }
    }
}
