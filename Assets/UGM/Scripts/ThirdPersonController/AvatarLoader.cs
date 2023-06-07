using UnityEngine;

public class AvatarLoader : UGMDownloader
{
    [SerializeField]
    [Tooltip("Preview avatar to display until avatar loads. Will be destroyed after new avatar is loaded")]
    private GameObject previewCharacter;
    [SerializeField]
    [Tooltip("Animator Controller to use on loaded character")]
    private RuntimeAnimatorController animatorController;
    [SerializeField]
    [Tooltip("Animator Avatar to use on loaded character")]
    private Avatar animatorAvatar;
    [SerializeField]
    [Tooltip("Animator use apply root motion")]
    private bool applyRootMotion;
    [SerializeField]
    [Tooltip("Animator update mode")]
    private AnimatorUpdateMode updateMode;
    [SerializeField]
    [Tooltip("Animator culling mode")]
    private AnimatorCullingMode cullingMode;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();
        if (previewCharacter != null)
        {
            SetupAvatar(previewCharacter);
        }
    }

    protected override void OnModelSuccess(GameObject targetAvatar)
    {
        if (previewCharacter != null)
        {
            Destroy(previewCharacter);
            previewCharacter = null;
        }
        SetupAvatar(targetAvatar);
        base.OnModelSuccess(targetAvatar);
    }

    //The Animator is disabled to allow Animation component to play
    protected override void OnAnimationStart(string animationName)
    {
        animator.enabled = false;
        base.OnAnimationStart(animationName);
    }
    //The Animator is enabled when the Animation component completes
    protected override void OnAnimationEnd(string animationName)
    {
        base.OnAnimationEnd(animationName);
        animator.enabled = true;
    }

    private void SetupAvatar(GameObject targetAvatar)
    {
        SetupAnimator();

        var controller = GetComponent<ThirdPersonController>();
        if (controller != null)
        {
            controller.Setup(gameObject);
        }
    }

    private void SetupAnimator()
    {
        //Remove old animator as it doesn't point to the character
        if (animator != null)
        {
            //Get the existing settings
            animatorController = animator.runtimeAnimatorController;
            animatorAvatar = animator.avatar;
            applyRootMotion = animator.applyRootMotion;
            updateMode = animator.updateMode;
            cullingMode = animator.cullingMode;
            DestroyImmediate(animator);
        }
        //Add new animator that points to the new character
        animator = gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;
        animator.avatar = animatorAvatar; //AvatarCreator.CreateAvatar(animator);
        animator.applyRootMotion = applyRootMotion;
        animator.updateMode = updateMode;
        animator.cullingMode = cullingMode;
        animator.enabled = true;
    }

    //TEMP
    private bool playingAnimation = false;
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Period))
        {
            if (playingAnimation)
            {
                StopAnimation();
                playingAnimation = false;
            }
            else
            {
                PlayAnimation("", true);
                playingAnimation = true;
            }
        }
    }
}
