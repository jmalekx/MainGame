using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Menu")]
    public GameObject inventoryMenu;
    public GameObject itemPanelPrefab;
    public GameObject itemPanelGrid;

    public Camera playerCamera;

    private PlayerInput playerInput;
    private InputAction scrollAction;
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
        dropAction = playerInput.Main.Drop;

        OnEnable();
    }

    void OnEnable()
    {
        scrollAction.Enable();
        dropAction.Enable();

        scrollAction.performed += OnScroll;
        dropAction.performed += OnDropItem;
    }

    void OnDisable()
    {
        scrollAction.performed -= OnScroll;
        dropAction.performed -= OnDropItem;

        scrollAction.Disable();
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
        int maxSlots = 10;
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject newItemPanel = Instantiate(itemPanelPrefab, itemPanelGrid.transform);
            ItemSlot itemSlot = newItemPanel.GetComponent<ItemSlot>();

            if (itemSlot != null)
            {
                if (i < inventory.items.Count)
                {

                    itemSlot.UpdateSlot(inventory.items[i]);

                }
                else
                {
                    itemSlot.ClearSlot();
          
                }

                itemSlot.SetSelected(i == selectedItemIndex);
            }
        }
    }

    //scroll input to navigate through items
    private void OnScroll(InputAction.CallbackContext context)
    {
        Vector2 scrollValue = context.ReadValue<Vector2>();
        int maxSlots = 10;
        if (scrollValue.y > 0)
        {
            selectedItemIndex = (selectedItemIndex - 1 + maxSlots) % maxSlots; // Scroll up
        }
        else if (scrollValue.y < 0)
        {
            selectedItemIndex = (selectedItemIndex + 1) % maxSlots; // Scroll down
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
                UpdateInventoryUI();

                //if the count reaches zero remove the item from inventory
                if (selectedItem.count <= 0)
                {
                    inventory.RemoveItem(selectedItem);
                }

                //place the item back into the world
                DropItemToWorld(selectedItem);
            }
        }
    }

    private void DropItemToWorld(ItemData item)
    {
        //prefab exists
        if (item.itemPrefab != null)
        {
            //position in fron of player camera
            Vector3 dropPosition = playerCamera.transform.position + playerCamera.transform.forward * 3.5f; // can adjust distance
            GameObject droppedItem = Instantiate(item.itemPrefab, dropPosition, Quaternion.identity);

            // Optionally, add a Rigidbody component for physics interactions
            Rigidbody rb = droppedItem.AddComponent<Rigidbody>();
            rb.AddForce(playerCamera.transform.up * 5f, ForceMode.Impulse); // Add some upward force to the item

            CraftingManager craftingManager = droppedItem.AddComponent<CraftingManager>();
            if (item.itemName == "Wood")
            {
                droppedItem.tag = "Wood";
                craftingManager.wood = item;
            }
            else if (item.itemName == "Stone")
            {
                droppedItem.tag = "Stone";
                craftingManager.stone = item;
            }


            //log
            Debug.Log("Dropped " + item.itemName + " into the world.");
        }
    }
}