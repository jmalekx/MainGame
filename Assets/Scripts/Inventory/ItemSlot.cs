
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
  
    public int index;
    public Image iconImage;
    public Image tintOverlay;
    public TMP_Text countText;
    public TMP_Text itemNameText;

    public GameObject itemVisual;


    private ItemData item;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Canvas canvas;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();  // Ensure RectTransform reference
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }


    //--------------------------------------------------------------------------------------------------------------
    public void UpdateSlot(ItemData itemData)
    {
        
        if (itemData != null)
        {
            item = itemData;
            iconImage.sprite = itemData.icon;
            iconImage.enabled = true;
            tintOverlay.enabled = false;
            countText.text = item.count.ToString();
            iconImage.color = new Color(1.5f, 1.5f, 1.5f, 1.5f);
        }
        else
        {
            ClearSlot(); // Clear the slot if no item is present
        }

        Debug.Log("Item inserted in slot " + index);
    }

    //--------------------------------------------------------------------------------------------------------------
    // Clear the slot when no item is assigned
    public void ClearSlot()
    {
        item = null;
        iconImage.sprite = null;
        iconImage.color = new Color(0f, 0f, 0f, 0f);
        //tintOverlay.enabled = false;
    }

    //--------------------------------------------------------------------------------------------------------------
    public ItemData GetItem()
    {
        return item;
    }

    public void SetSelected(bool isSelected)
    {
        //tintOverlay.enabled = true;
        //tintOverlay.color = new Color(0.6f, 0f, 0.9f, 0.5f);
    }

    public bool isSlotFilled()
    {
        return !isSlotEmpty();
    }
    
    public bool isSlotEmpty()
    {
        return item == null;
    }
    //--------------------------------------------------------------------------------------------------------------

    // Start dragging the item
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isSlotFilled())
        {
            // Store the original position
            originalPosition = rectTransform.anchoredPosition;
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
            countText.text = null;

            // Create a visual copy of the item to follow the mouse
            itemVisual = Instantiate(gameObject, canvas.transform);
            itemVisual.GetComponent<CanvasGroup>().alpha = 0.6f;  // Make it semi-transparent
            itemVisual.GetComponent<RectTransform>().position = rectTransform.position;  // Start at the original position

            // Update the slot with the reduced item count
            if (item.count > 1)
            {
                item.count -= 1;
                UpdateSlot(item);
            }
            else
            {
                // Clear the slot if only one item is picked up
                //ClearSlot();
            }
        }
    }

    //--------------------------------------------------------------------------------------------------------------

    // While dragging the item
    public void OnDrag(PointerEventData eventData)
    {

        // Update the position of the item visual to follow the mouse
        itemVisual.GetComponent<RectTransform>().position = eventData.position;

    }


    //--------------------------------------------------------------------------------------------------------------

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("ItemSlot::OnEndDrag");
    
        //Destroy(itemVisual);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    //--------------------------------------------------------------------------------------------------------------

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("ItemSlot::OnDrop");

       
            Destroy(itemVisual);
            Debug.Log("Destroyed itemVisual on drop.");
        
    }
}

