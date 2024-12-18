using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public List<ItemData> items = new List<ItemData>();
    public bool ScrollObtained { get; private set; }

    public event Action OnInventoryChanged; // Event to notify inventory changes

    public Slider HealthBar;

    [Header("Audio")]
    public AudioClip pickupSound;
    private AudioSource audioSource;
    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        items.Clear();
        ScrollObtained = false;
    }

    public void UseItem(ItemData itemToUse)
    {
        if (itemToUse.count >0)
        {
            if (itemToUse.itemName == "Apple")
            {
                ObjectiveManager.Instance.TrackObjective("Consume an apple");
            }

            HealthBar.value = Mathf.Min(HealthBar.maxValue, HealthBar.value + itemToUse.healthRestoreAmount);
            itemToUse.count--;

            if (itemToUse.count <= 0 )
            {
                items.Remove(itemToUse);
            }

            OnInventoryChanged?.Invoke();
            Debug.Log("Consumed " + itemToUse.itemName + ", health restored by " + itemToUse.healthRestoreAmount);
        }
    }

//------------------------------------------------------------------------------------------------------------
    public void AddItem(ItemData itemToAdd)
    {
        bool itemExists = false;

        foreach (ItemData item in items)
        {
            if (item.itemName == itemToAdd.itemName) // check by item name
            {
                item.count += 1; // always add 1 to the count for each pickup
                itemExists = true;
                break;
            }
        }

        if (!itemExists)
        {
            itemToAdd.count = 1; // initialize count to 1 for a new item
            items.Add(itemToAdd); // add new item if it does not exist

            PopupManager.Instance.DisplayItemHint(itemToAdd.itemName);

        }

        TrackObjective(itemToAdd.itemName);
        OnInventoryChanged?.Invoke(); // notify the UI to update

        audioSource.PlayOneShot(pickupSound,1.0f);

        if (itemToAdd.itemName == "Scroll")
        {
            // Set a flag or perform an action to mark that the scroll is now obtained
            ScrollObtained = true;  // Add a flag to track the scroll's acquisition
        }

        Debug.Log(itemToAdd.itemName + " added to inventory. Total count: " + itemToAdd.count);
    }

    private void TrackObjective(string itemName)
    {
        var objectiveMap = new Dictionary<string, string>
        {
            { "Apple", "Collect an apple" },
            { "Stone", "Collect stone" },
            { "Wood", "Collect wood" }
        };

        // Check if the dictionary contains the itemName and call TrackObjective if found
        if (objectiveMap.TryGetValue(itemName, out string objectiveDescription))
        {
            ObjectiveManager.Instance.TrackObjective(objectiveDescription);
        }
    }

    //------------------------------------------------------------------------------------------------------------
    public void RemoveItem(ItemData itemToRemove)
    {
        foreach (var item in items)
        {
            if (item.itemName == itemToRemove.itemName)
            {
                item.count -= itemToRemove.count;
                if (item.count <= 0)
                {
                    items.Remove(item);
                }
                break;
            }
        }

        OnInventoryChanged?.Invoke(); // notify the UI to update
        Debug.Log(itemToRemove.count + " " + itemToRemove.itemName + " removed from inventory.");
    }
}
