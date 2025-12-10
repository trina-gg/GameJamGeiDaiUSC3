using UnityEngine;

/// <summary>
/// 挂在场景里的“可拾取物体”上（SpriteRenderer + Collider2D）
/// 单击时，如果当前 panel 是激活的，就把自己交给 InventoryManager
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class InventoryItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemId;            // 用来和 ItemReceiver 的 acceptsItemId 对上
    public Sprite iconSprite;        // 显示在 Inventory UI 里的图标（可以和本体 sprite 一样）

    [Header("Cached World State (自动填)")]
    [HideInInspector] public Vector3 originalLocalPos;
    [HideInInspector] public Vector3 originalLocalScale;
    [HideInInspector] public Transform originalParent;

    SpriteRenderer _renderer;
    Collider2D _collider;
    bool _isInWorld = true;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    void Start()
    {
        // 初始时缓存在世界中的相对信息（在某个 SpritePanel 下面）
        CacheWorldTransform();
    }

    void CacheWorldTransform()
    {
        if (transform.parent != null)
        {
            originalParent = transform.parent;
            originalLocalPos = transform.localPosition;
            originalLocalScale = transform.localScale;
        }
    }

    void OnMouseDown()
    {
        // 只允许在“当前激活的面板”中拾取物品
        SpritePanel myPanel = GetComponentInParent<SpritePanel>();
        if (myPanel == null || SpritePanelManager.Instance.currentPanel != myPanel)
            return;

        // 如果 inventory 已经有东西了，就不再拾取
        if (InventoryManager.Instance == null || InventoryManager.Instance.HasItem)
            return;

        InventoryManager.Instance.PickupItem(this);
    }

    /// <summary>
    /// 从世界中“隐藏”这个物体（本体还留在原 panel 里，只是不可见，不参与碰撞）
    /// </summary>
    public void HideInWorld()
    {
        _isInWorld = false;
        if (_renderer != null) _renderer.enabled = false;
        if (_collider != null) _collider.enabled = false;
    }

    /// <summary>
    /// 把物体显示在世界中，并设置父级和局部位置/缩放。
    /// 这样就会随着所在 SpritePanel 一起缩放。
    /// </summary>
    public void ShowInWorld(Transform parent, Vector3 localPos, Vector3? localScaleOverride = null)
    {
        _isInWorld = true;
        transform.SetParent(parent);
        transform.localPosition = localPos;
        transform.localScale = localScaleOverride ?? originalLocalScale;

        if (_renderer != null) _renderer.enabled = true;
        if (_collider != null) _collider.enabled = true;
    }

    /// <summary>
    /// 如果你想把物体放回原来的 panel / 原先的位置
    /// </summary>
    public void RestoreToOriginal()
    {
        ShowInWorld(originalParent, originalLocalPos, originalLocalScale);
    }
}
