#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UGM.Editor.UGMTool;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

[InitializeOnLoad]
public class UGMInstallConfigure
{
    static UGMInstallConfigure()
    {
        Events.registeredPackages += OnRegisteredPackages;
    }

    private static void OnRegisteredPackages(PackageRegistrationEventArgs args)
    {
        Events.registeredPackages -= OnRegisteredPackages;
        
        if (args.added != null && args.added.Any(p => p.name == "com.nftygames.ugmcore"))
        {
            UGMEditorWindow.OpenWindow();
        }
    }
}
#endif
