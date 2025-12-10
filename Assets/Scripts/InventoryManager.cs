using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Single Slot")]
    [Tooltip("唯一的背包 UI 图标脚本（挂在 Canvas/InventoryPanel/InventoryItemImage 上）")]
    public InventoryItemUI inventoryItemUI;

    private string currentItemId;

    public bool HasItem => !string.IsNullOrEmpty(currentItemId);
    public string CurrentItemId => currentItemId;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (inventoryItemUI != null)
        {
            inventoryItemUI.gameObject.SetActive(false);
            inventoryItemUI.Clear();
        }
    }

    /// <summary>
    /// 往背包里放一个物品。当前已经有物品时直接忽略（单物品背包）。
    /// </summary>
    public void AddItem(string itemId, Sprite icon)
    {
        if (HasItem)
        {
            Debug.Log($"[InventoryManager] Already has item {currentItemId}, ignore add {itemId}");
            return;
        }

        currentItemId = itemId;
        Debug.Log($"[InventoryManager] Add item {itemId}");

        if (inventoryItemUI != null)
        {
            inventoryItemUI.Setup(itemId, icon);
            inventoryItemUI.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 使用完物品后清空背包。
    /// </summary>
    public void ClearItem()
    {
        Debug.Log($"[InventoryManager] Clear item {currentItemId}");
        currentItemId = null;

        if (inventoryItemUI != null)
        {
            inventoryItemUI.Clear();
            inventoryItemUI.gameObject.SetActive(false);
        }
    }
}
