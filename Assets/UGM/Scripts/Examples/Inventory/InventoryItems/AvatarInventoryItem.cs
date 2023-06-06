using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarInventoryItem : InventoryItem
{
    AvatarLoader avatarLoader;
    private void OnEnable()
    {
        //Not good practice for production, create a static reference to your player
        avatarLoader = FindObjectOfType<AvatarLoader>();
    }
    protected override void DoAction()
    {
        base.DoAction();
        if (avatarLoader)
        {
            avatarLoader.LoadAsync(tokenInfo.token_id);
        }
    }
}
