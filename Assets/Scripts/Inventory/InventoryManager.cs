using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Menu")]
    public GameObject inventoryMenu;
    public GameObject itemPanelPrefab;
    public GameObject itemPanelGrid;

    private PlayerInput playerInput;
    private InputAction scrollAction;
    private InputAction numberKeysAction;
    private InputAction dropAction;

    private Inventory inventory;
    private int selectedItemIndex = 0;

    void Start()
    {
        inventory = Inventory.Instance;
        inventory.OnInventoryChanged += UpdateInventoryUI;
        UpdateInventoryUI();
    }

    void Awake()
    {
        playerInput = new PlayerInput();
        scrollAction = playerInput.UI.ScrollWheel;
        numberKeysAction = playerInput.Main.NumberKeys;
        dropAction = playerInput.Main.Drop;

        OnEnable();
    }

    void OnEnable()
    {
        scrollAction.Enable();
        numberKeysAction.Enable();
        dropAction.Enable();

        scrollAction.performed += OnScroll;
        numberKeysAction.performed += OnNumberKeyPressed;
        dropAction.performed += OnDropItem;
    }

    void OnDisable()
    {
        scrollAction.performed -= OnScroll;
        numberKeysAction.performed -= OnNumberKeyPressed;
        dropAction.performed -= OnDropItem;

        scrollAction.Disable();
        numberKeysAction.Disable();
        dropAction.Disable();
    }

    void UpdateInventoryUI()
    {
        //clear existing item panels
        foreach (Transform child in itemPanelGrid.transform)
        {
            Destroy(child.gameObject);
        }

        //create new item panel for each item in the inventory
        for (int i = 0; i < inventory.items.Count; i++)
        {
            GameObject newItemPanel = Instantiate(itemPanelPrefab, itemPanelGrid.transform);
            ItemSlot itemSlot = newItemPanel.GetComponent<ItemSlot>();

            if (itemSlot != null)
            {
                itemSlot.UpdateSlot(inventory.items[i]);

                //update the item name text color based on selection
                TextMeshProUGUI itemNameText = itemSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (itemNameText != null)
                {
                    itemNameText.text = inventory.items[i].itemName; 
                    itemNameText.color = (i == selectedItemIndex) ? Color.blue : Color.white;  //highlight selected item
                }
            }
        }
    }

    //scroll input to navigate through items
    private void OnScroll(InputAction.CallbackContext context)
    {
        Vector2 scrollValue = context.ReadValue<Vector2>();
        if (scrollValue.y > 0)
        {
            selectedItemIndex = Mathf.Max(0, selectedItemIndex - 1); // Scroll up
        }
        else if (scrollValue.y < 0)
        {
            selectedItemIndex = Mathf.Min(inventory.items.Count - 1, selectedItemIndex + 1); // Scroll down
        }

        UpdateInventoryUI();
    }

    // Handle number keys to select item
    private void OnNumberKeyPressed(InputAction.CallbackContext context)
    {
        int pressedKey = (int)context.ReadValue<float>();  // Get number key pressed (1-0)
        if (pressedKey >= 1 && pressedKey <= 9)
        {
            selectedItemIndex = pressedKey - 1;  // Inventory slots map to 1-9 keys
        }
        else if (pressedKey == 0)
        {
            selectedItemIndex = 9;  // 0 key maps to 10th slot
        }

        UpdateInventoryUI();
    }

    //handle dropping an item
    private void OnDropItem(InputAction.CallbackContext context)
    {
        if (inventory.items.Count > 0)
        {
            ItemData selectedItem = inventory.items[selectedItemIndex];

            if (selectedItem.count > 0)
            {
                //decrease the item count by one
                selectedItem.count--;

                //if the count reaches zero remove the item from inventory
                if (selectedItem.count <= 0)
                {
                    inventory.RemoveItem(selectedItem);
                }

                //place the item back into the world
                DropItemToWorld(selectedItem);

                //update the UI after removal
                UpdateInventoryUI();
            }
        }
    }

    private void DropItemToWorld(ItemData item)
    {
        // Implement logic for spawning the item back into the game world
        Debug.Log("Dropped " + item.itemName + " into the world.");
    }
}