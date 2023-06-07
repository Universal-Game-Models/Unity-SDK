using System.Collections.Generic;
using UnityEngine;
using static UGMDataTypes;

public class AnimationSelector: MonoBehaviour
{
    [SerializeField]
    private AnimationSelectorButton animationSelectorButtonPrefab;
    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private Transform content;
    [SerializeField]
    private UGMDownloader loader;
    [SerializeField]
    private bool loopAnimation = true;

    private bool contentActive = false;

    private void Awake()
    {
        if (!loader) loader = GetComponent<UGMDownloader>();
        if(!loader) loader = GetComponentInParent<UGMDownloader>();
        if(loader) loader.onMetadataSuccess.AddListener(OnMetadataSuccess);
    }

    private void OnDestroy()
    {
        if(loader) loader.onMetadataSuccess.RemoveListener(OnMetadataSuccess);
    }

    private void OnMetadataSuccess(Metadata metadata)
    {
        List<string> animationNames = new List<string>();
        foreach (var attribute in metadata.attributes)
        {
            var attributeValue = attribute.value.ToString();
            if (attribute.trait_type == "Animation" && !animationNames.Contains(attributeValue))
            {
                animationNames.Add(attributeValue);
            }
        }
        Init(animationNames.ToArray());
    }

    private void Init(string[] animationNames)
    {
        //Destroy the existing animation selector buttons
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        //If there are no animations disable the content
        if (animationNames.Length == 0)
        {
            parent.gameObject.SetActive(false);
            return;
        }
        //Set the active to its current state and display all animation selector buttons
        parent.gameObject.SetActive(contentActive);
        foreach (var animationName in animationNames)
        {
            var newBtn = Instantiate(animationSelectorButtonPrefab, content);
            newBtn.Init(() =>
            {
                if (loader.CurrentEmbeddedAnimationName == animationName)
                {
                    loader.StopAnimation();
                }
                else
                {
                    loader.PlayAnimation(animationName, loopAnimation);
                }
            }, animationName);
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.B))
        {
            ToggleContent(!contentActive);
        }
    }

    public void ToggleContent(bool active)
    {
        //Do not allow if their are no animations
        if (content.childCount <= 0) active = false;
        contentActive = active;
        parent.SetActive(contentActive);
        ExampleUIEvents.OnShowCursor.Invoke(active);
    }

    public void SetLoader(UGMDownloader uGMDownloader, Metadata metadata = null)
    {
        if(uGMDownloader == null)
        {
            Debug.LogWarning("The UGMDownloader was null");
            return;
        }
        if(loader != null)
        {
            loader.onMetadataSuccess.RemoveListener(OnMetadataSuccess);
        }
        loader = uGMDownloader;
        if (metadata != null)
        {
            OnMetadataSuccess(metadata);
        }

        //Wait for metadata to be downloaded
        //Must have UGMDownloader.loadMetadata = true before calling Load
        loader.onMetadataSuccess.AddListener(OnMetadataSuccess);      
    }
}