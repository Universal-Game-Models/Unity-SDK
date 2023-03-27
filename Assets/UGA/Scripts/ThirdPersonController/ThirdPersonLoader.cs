using UnityEngine;

public class ThirdPersonLoader : UGADownloader
{
    private readonly Vector3 avatarPositionOffset = new Vector3(0, -0.08f, 0);

    private GameObject avatar;
    [SerializeField]
    [Tooltip("Animator to use on loaded avatar")]
    private RuntimeAnimatorController animatorController;
    [SerializeField]
    private Avatar runtimeAvatar;

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
    private void OnDestroy()
    {
        onSuccess.RemoveListener(OnLoadCompleted);
        onFailure.RemoveListener(OnLoadFailed);
    }

    private void OnLoadFailed()
    {
        Debug.LogError("Failed to load asset from: " + url);
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
            controller.Setup(avatar, animatorController, runtimeAvatar);
        }
    }
}
