using UnityEngine;

public class ThirdPersonLoader : UGADownloader
{
    private GameObject avatar;
    [SerializeField]
    [Tooltip("Animator to use on loaded avatar")]
    private RuntimeAnimatorController animatorController;
    [SerializeField]
    private Vector3 avatarPositionOffset = new Vector3(0, 0, 0);
    [SerializeField]
    [Tooltip("Preview avatar to display until avatar loads. Will be destroyed after new avatar is loaded")]
    private GameObject previewCharacter;

    private void Awake()
    {
        onSuccess.AddListener(OnLoadCompleted);
        onFailure.AddListener(OnLoadFailed);
    }
    protected override void Start()
    {
        base.Start();
        if (previewCharacter != null)
        {
            SetupAvatar(previewCharacter);
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        onSuccess.RemoveListener(OnLoadCompleted);
        onFailure.RemoveListener(OnLoadFailed);
    }

    private void OnLoadFailed()
    {
        Debug.LogError("Failed to load asset from: " + assetName);
    }

    private void OnLoadCompleted(GameObject targetAvatar)
    {
        if (previewCharacter != null)
        {
            Destroy(previewCharacter);
            previewCharacter = null;
        }
        SetupAvatar(targetAvatar);
    }

    private void SetupAvatar(GameObject targetAvatar)
    {
        if (avatar != null)
        {
            Destroy(avatar);
        }

        avatar = targetAvatar;
        // Re-parent and reset transforms
        avatar.transform.parent = transform;
        avatar.transform.localPosition = avatarPositionOffset;
        avatar.transform.localRotation = Quaternion.Euler(0, 0, 0);

        var controller = GetComponent<ThirdPersonController>();
        if (controller != null)
        {
            controller.Setup(avatar, animatorController);
        }
    }
}
