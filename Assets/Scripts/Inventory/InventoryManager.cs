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
    public TextMeshProUGUI holdingText;

    public Camera playerCamera;

    private PlayerInput playerInput;
    private InputAction scrollAction;
    private InputAction dropAction;
    private InputAction useAction;
    private InputAction selectSlotAction;

    private Inventory inventory;
    private int selectedItemIndex = 0;

    private PopupAnim objectivePopupAnim;
    private Coroutine holdingTextCoroutine; 


    void Start()
    {
        inventory = Inventory.Instance;
        inventory.OnInventoryChanged += UpdateInventoryUI;
        UpdateInventoryUI();

        objectivePopupAnim = GameObject.Find("objectiveCanvas").GetComponent<PopupAnim>();
        objectivePopupAnim.gameObject.SetActive(false);
        holdingText.gameObject.SetActive(false);
    }

    void Awake()
    {
        playerInput = new PlayerInput();
        scrollAction = playerInput.UI.ScrollWheel;
        dropAction = playerInput.Main.Drop;
        useAction = playerInput.Main.Use;
        selectSlotAction = playerInput.Main.SelectSlot;

        OnEnable();
    }

    void OnEnable()
    {
        scrollAction.Enable();
        dropAction.Enable();
        useAction.Enable();
        selectSlotAction.Enable();

        scrollAction.performed += OnScroll;
        dropAction.performed += OnDropItem;
        useAction.performed += OnUseItem;
        useAction.canceled += OnUseItemReleased;
        selectSlotAction.performed += OnSelectSlot;
    }

    void OnDisable()
    {
        scrollAction.performed -= OnScroll;
        dropAction.performed -= OnDropItem;
        useAction.performed -= OnUseItem;
        selectSlotAction.performed -= OnSelectSlot;

        scrollAction.Disable();
        dropAction.Disable();
        selectSlotAction.Disable();
    }

    void OnSelectSlot(InputAction.CallbackContext context)
    {
        //map control to slot index
        string keyPressed = context.control.name;
        int slot = keyPressed switch
        {
            "1" => 0,
            "2" => 1,
            "3" => 2,
            "4" => 3,
            "5" => 4,
            "6" => 5,
            "7" => 6,
            "8" => 7,
            "9" => 8,
            "0" => 9,
            _ => -1, //handle invalid
        };

        if (slot >= 0 && slot < 10) //inventory range
        {
            selectedItemIndex = slot;
            UpdateInventoryUI();
        }
    }

    void ShowHoldingText(string itemName)
    {
        //if coroutine already running stop to reset timer
        if (holdingTextCoroutine != null)
        {
            StopCoroutine(holdingTextCoroutine);
        }

        holdingText.text = $"Holding: {itemName}";
        holdingText.gameObject.SetActive(true);

        //start new
        holdingTextCoroutine = StartCoroutine(FadeHoldingText());
    }

    IEnumerator FadeHoldingText()
    {
        yield return new WaitForSeconds(2f);
        holdingText.gameObject.SetActive(false); //hide after 3s
    }

    void OnUseItem(InputAction.CallbackContext context)
    {
        if (inventory.items.Count > 0 && selectedItemIndex < inventory.items.Count)
        {
            ItemData selectedItem = inventory.items[selectedItemIndex];
            if (selectedItem.isConsumable)
            {
                inventory.UseItem(selectedItem);
                UpdateInventoryUI();
            }
            if (selectedItem.itemName == "Scroll")
            {
                objectivePopupAnim.ShowPopup(); //show objectives while pressed
            }
        }
    }
    void OnUseItemReleased(InputAction.CallbackContext context)
    {
        ItemData selectedItem = inventory.items[selectedItemIndex];
        if (selectedItem.itemName == "Scroll")
        {
            //hide popup when not clicking anymore
            objectivePopupAnim.HidePopup();
        }
      
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

                    //update text for selecteditem to be displayed
                    if (i == selectedItemIndex)
                    {
                        ShowHoldingText(inventory.items[i].itemName);
                    }

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
            selectedItemIndex = (selectedItemIndex - 1 + maxSlots) % maxSlots; //scroll up
        }
        else if (scrollValue.y < 0)
        {
            selectedItemIndex = (selectedItemIndex + 1) % maxSlots; //scroll down
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
            //position in front of player camera
            Vector3 dropPosition = playerCamera.transform.position + playerCamera.transform.forward * 3.5f; // can adjust distance
            GameObject droppedItem = Instantiate(item.itemPrefab, dropPosition, item.itemPrefab.transform.rotation);

            //rb for physics
            Rigidbody rb = droppedItem.AddComponent<Rigidbody>();
            rb.AddForce(playerCamera.transform.up * 5f, ForceMode.Impulse); //upward force

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