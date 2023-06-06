using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandEquipmentInventoryItem : InventoryItem, IPointerClickHandler
{
    AvatarLoader avatarLoader;
    HumanoidEquipmentLoader[] tools;
    private int Hand;

    private void OnEnable()
    {
        //Not good practice for production, create a static reference to your player
        avatarLoader = FindObjectOfType<AvatarLoader>();
        if(avatarLoader) tools = avatarLoader.GetComponentsInChildren<HumanoidEquipmentLoader>();
    }

    protected override void DoAction()
    {
        base.DoAction();
        Hand = 0;
        ChangeEquipmentModel();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Hand = 1;
            ChangeEquipmentModel();
        }
    }

    private void ChangeEquipmentModel()
    {
        if (tools != null && tools.Length > Hand)
        {
            tools[Hand].LoadAsync(tokenInfo.token_id);
        }
    }
}
