using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UGMAssetManager;

public class ModelItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    protected Button button;
    [SerializeField]
    private TMPro.TextMeshProUGUI nameText;
    protected ModelsOwnedTokenInfo tokenInfo;

    private bool hovering = false;
    //To be override for custom actions
    protected virtual void DoAction(){}

    protected virtual void OnDisable()
    {
        hovering = false;
    }

    public void Init(ModelsOwnedTokenInfo tokenInfo)
    {
        this.tokenInfo = tokenInfo;
        if (nameText) nameText.text = this.tokenInfo.metadata.name;
    }

    // Start is called before the first frame update
    private void Start()
    {
        button.onClick.AddListener(DoAction);
    }
    private void OnDestroy()
    {
        button.onClick.RemoveListener(DoAction);
    }

    private void Update()
    {
        if (hovering)
        {
            var numberKeyPressed = QuickSelectControl.Instance.GetNumberKeyPressed();
            if(numberKeyPressed >= 0)
            {
                QuickSelectControl.Instance.SetQuickSelect(numberKeyPressed, tokenInfo, DoAction);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
