using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ModelTester : MonoBehaviour
{
    [SerializeField]
    private string defaultAvatarId;
    [SerializeField]
    private UGMDownloader defaultLoader;
    [SerializeField]
    private AvatarLoader avatarLoader;
    [SerializeField]
    private HumanoidToolLoader toolLoader;

    [Button]
    public void Test()
    {
        Load("8");
    }
    // Start is called before the first frame update
    async void Start()
    {
        //Get the url params
        var args = URLParameters.GetArguments();
        if (args.ContainsKey("id"))
        {
            var nftId = args["id"];
            await Load(nftId);
        }
    }

    private async Task Load(string nftId)
    {
        //Get the nft metadata
        var metadata = await UGMDownloader.DownloadMetadataAsync(UGMManager.METADATA_URI + nftId.PadLeft(64, '0') + ".json");
        //Use the appropriate UGA Downloader to create it
        var characterAttribute = Array.Find(metadata.attributes, a => a.trait_type == "Character");
        if (characterAttribute != null)
        {
            if ((string)characterAttribute.value == "Avatar")
            {
                //Load the avatar
                avatarLoader.Load(nftId);
            }
            else if ((string)characterAttribute.value == "Equipment")
            {
                //Create a default avatar
                avatarLoader.onModelSuccess.AddListener((model) =>
                {
                    //When the avatar is loaded
                    toolLoader.Load(nftId);
                });
                //Start loading the avatar and equipment
                avatarLoader.Load(defaultAvatarId);
            }
        }
        else
        {
            //Load an avatar to explore with
            avatarLoader.Load(defaultAvatarId);
            //Default load method
            var structureAttribute = Array.Find(metadata.attributes, a => a.trait_type == "Structure");
            if(structureAttribute != null && (string)structureAttribute.value == "Building")
            {
                //Buildings should always use mesh colliders for interior wall collisions
                defaultLoader.SetLoadOptions(false, true, true, false, false);
            }
            defaultLoader.Load(nftId);
        }
    }
}
