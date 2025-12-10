using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class ItemDropTargetUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Expected Item")]
    [Tooltip("这个放置点需要的物品 id，比如 'rocket'")]
    public string requiredItemId;

    [Header("Panels")]
    [Tooltip("当前这一张 Panel，比如 Mars（火星）")]
    public ZoomablePanel myPanel;

    [Tooltip("放置成功并点击后要去的下一张 Panel，比如 BlackHolePanel")]
    public ZoomablePanel targetPanel;

    [Header("Transition Anchor")]
    [Tooltip("用于无缝过渡的锚点，一般直接用自己这个 RectTransform；留空则自动取自身")]
    public RectTransform transitionRect;

    [Header("Visual After Place")]
    [Tooltip("放置成功后用来显示物品的 Image（缺口被补上 / 发射架上出现火箭）")]
    public Image targetImage;

    [Header("Click After Solved")]
    [Tooltip("放置成功后立刻切场景（默认 false）")]
    public bool transitionOnDrop = false;

    [Tooltip("放置成功后需要再点击一次这个位置来触发动画+切场景（默认 true）")]
    public bool transitionOnClick = true;

    [Header("Optional Animation On Solved Click")]
    [Tooltip("点击后要播放动画的 Animator（比如火箭飞向黑洞）")]
    public Animator solvedClickAnimator;

    [Tooltip("上面 Animator 中的 Trigger 名，比如 'Launch'")]
    public string solvedClickTrigger = "Play";

    [Tooltip("触发动画后，等待多少秒再切到下一张 Panel")]
    public float solvedClickDelay = 1f;

    private bool solved = false;
    private bool isTransitionPlaying = false;

    RectTransform selfRect;

    void Awake()
    {
        selfRect = GetComponent<RectTransform>();
    }

    // --- 拖拽物品松手时触发 ---
    public void OnDrop(PointerEventData eventData)
    {
        if (solved) return;
        if (InventoryManager.Instance == null)
            return;

        // 必须当前 Panel 正好是 myPanel
        if (PanelManager.Instance != null && myPanel != null)
        {
            if (PanelManager.Instance.currentPanel != myPanel)
            {
                Debug.Log("[ItemDropTargetUI] Drop ignored, currentPanel is not myPanel.");
                return;
            }
        }

        GameObject draggedObj = eventData.pointerDrag;
        if (draggedObj == null) return;

        InventoryItemUI invItem = draggedObj.GetComponent<InventoryItemUI>();
        if (invItem == null) return;

        Debug.Log($"[ItemDropTargetUI] OnDrop, dragged item = {invItem.ItemId}");

        // 判断是否正确物品
        if (invItem.ItemId != requiredItemId)
        {
            Debug.Log($"[ItemDropTargetUI] Wrong item {invItem.ItemId}, require {requiredItemId}");
            return;
        }

        // 正确物品：解谜成功
        solved = true;

        Sprite usedSprite = invItem.iconImage != null ? invItem.iconImage.sprite : null;

        // 在目标位置显示图像（画面补全）
        if (targetImage != null && usedSprite != null)
        {
            targetImage.sprite = usedSprite;
            targetImage.enabled = true;
        }

        // 清空背包（隐藏背包图标）
        InventoryManager.Instance.ClearItem();

        Debug.Log($"[ItemDropTargetUI] Puzzle solved with item {requiredItemId}");

        // 如果你希望一放置就立刻切场景，可以打开这个开关
        if (transitionOnDrop)
        {
            StartTransitionSequence();
        }
        // 否则就等玩家再点击一次这个位置（transitionOnClick）
    }

    // --- 放置成功后的「点击」：触发动画 + Panel 切换 ---
    public void OnPointerClick(PointerEventData eventData)
    {
        // 必须左键
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // 必须已经解谜成功
        if (!solved)
        {
            Debug.Log("[ItemDropTargetUI] Clicked before solved, ignore.");
            return;
        }

        if (!transitionOnClick)
        {
            Debug.Log("[ItemDropTargetUI] transitionOnClick = false, ignore click.");
            return;
        }

        // 同样只在当前 Panel == myPanel 时才响应
        if (PanelManager.Instance != null && myPanel != null)
        {
            if (PanelManager.Instance.currentPanel != myPanel)
            {
                Debug.Log("[ItemDropTargetUI] Click ignored, currentPanel is not myPanel.");
                return;
            }
        }

        StartTransitionSequence();
    }

    void StartTransitionSequence()
    {
        if (isTransitionPlaying)
            return;

        if (PanelManager.Instance == null || myPanel == null || targetPanel == null)
        {
            Debug.LogWarning("[ItemDropTargetUI] Cannot start transition, missing PanelManager / myPanel / targetPanel.");
            return;
        }

        StartCoroutine(TransitionRoutine());
    }

    IEnumerator TransitionRoutine()
    {
        isTransitionPlaying = true;

        Debug.Log("[ItemDropTargetUI] Start transition sequence.");

        // 1. 播放点击后的动画（比如火箭飞向黑洞）
        if (solvedClickAnimator != null && !string.IsNullOrEmpty(solvedClickTrigger))
        {
            solvedClickAnimator.SetTrigger(solvedClickTrigger);
        }

        // 2. 等一小段时间，让动画飞一会儿 / 播完
        if (solvedClickDelay > 0f)
        {
            yield return new WaitForSeconds(solvedClickDelay);
        }

        // 3. 然后通过 PanelManager 的 Seamless 过渡切到下一张 Panel
        RectTransform rect = transitionRect != null ? transitionRect : selfRect;

        PanelManager.Instance.PlaySeamlessTransition(myPanel, targetPanel, rect);

        isTransitionPlaying = false;

        Debug.Log("[ItemDropTargetUI] Transition finished.");
    }
}
