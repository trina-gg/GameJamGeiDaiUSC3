using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 挂在场景里的“可拾取物体”上（SpriteRenderer + Collider2D）
/// 单击时，如果当前 panel 是激活的，就把自己交给 InventoryManager。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class InventoryItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemId;            // 用来和 ItemReceiver 的 acceptsItemId 对上
    public Sprite iconSprite;        // 显示在 Inventory UI 里的图标（可以和本体 sprite 一样）

    [Header("Events")]
    [Tooltip("当物体被成功拾取（加入 Inventory）后触发一次。")]
    public UnityEvent onPickedUp;

    [Header("Cached World State (自动填)")]
    [HideInInspector] public Vector3 originalLocalPos;
    [HideInInspector] public Vector3 originalLocalScale;
    [HideInInspector] public Transform originalParent;

    SpriteRenderer _renderer;
    Collider2D _collider;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    void Start()
    {
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
        Debug.Log("Rocket clicked!");
        
        // 只允许在当前激活 panel 中拾取：
        SpritePanel myPanel = GetComponentInParent<SpritePanel>();
        if (myPanel == null || SpritePanelManager.Instance.currentPanel != myPanel)
            return;

        // 如果 inventory 已经有东西了，就不再拾取
        if (InventoryManager.Instance == null || InventoryManager.Instance.HasItem)
            return;

        // 请求加入 inventory
        InventoryManager.Instance.PickupItem(this);

        // 拾取成功后触发事件（比如切换背景）
        onPickedUp?.Invoke();
    }

    public void HideInWorld()
    {
        if (_renderer != null) _renderer.enabled = false;
        if (_collider != null) _collider.enabled = false;
    }

    public void ShowInWorld(Transform parent, Vector3 localPos, Vector3? localScaleOverride = null)
    {
        transform.SetParent(parent);
        transform.localPosition = localPos;
        transform.localScale = localScaleOverride ?? originalLocalScale;

        if (_renderer != null) _renderer.enabled = true;
        if (_collider != null) _collider.enabled = true;
    }

    public void RestoreToOriginal()
    {
        ShowInWorld(originalParent, originalLocalPos, originalLocalScale);
    }
}
