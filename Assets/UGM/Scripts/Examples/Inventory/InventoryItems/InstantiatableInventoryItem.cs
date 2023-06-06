using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class InstantiatableInventoryItem : InventoryItem
{
    protected override void DoAction()
    {
        base.DoAction();
        //Change this to initialize an onMouse listener
        InstantiatableInventoryControl.Instance.SetTokenInfo(tokenInfo);
    }
}
