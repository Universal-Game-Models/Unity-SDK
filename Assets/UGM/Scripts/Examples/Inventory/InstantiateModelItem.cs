using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class InstantiateModelItem : ModelItem
{
    protected override void DoAction()
    {
        base.DoAction();
        //Change this to initialize an onMouse listener
        InstantiateModelControl.Instance.SetTokenInfo(tokenInfo);
    }
}
