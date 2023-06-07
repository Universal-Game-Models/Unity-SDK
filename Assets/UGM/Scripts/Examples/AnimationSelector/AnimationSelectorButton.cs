using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UGMDataTypes;

public class AnimationSelectorButton : MonoBehaviour
{
    [SerializeField]
    private Button selectorButton;
    [SerializeField]
    private TMPro.TextMeshProUGUI animationNameText;

    public void Init(UnityAction buttonAction, string animationName)
    {
        selectorButton.onClick.RemoveAllListeners();
        selectorButton.onClick.AddListener(buttonAction);
        animationNameText.text = animationName;
    }
}