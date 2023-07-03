using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor.Compilation;
using System;
using UnityEngine;

namespace UGM.Editor
{
    [InitializeOnLoad]
    public static class InstallUGMDependencies
    {
    
        private const string MODULE_NAME = "com.nftygames.ugm";
        private const string PROGRESS_BAR_TITLE = "Universal Game Models";
        private const string INSTALLING_MODULES = "Installing dependencies...";
        private const string ALL_MODULES_ARE_INSTALLED = "All dependencies are installed.";
        private const int THREAD_SLEEP_TIME = 100;
        private const float TIMEOUT_FOR_MODULE_INSTALLATION = 180f;

        static InstallUGMDependencies()
        {
            Events.registeredPackages += OnRegisteredPackages;
            Events.registeringPackages += OnRegisteringPackages;
        }
    
        private static void OnRegisteredPackages(PackageRegistrationEventArgs args)
        {
            Events.registeredPackages -= OnRegisteredPackages;
        
            if (args.added != null && args.added.Any(p => p.name == MODULE_NAME))
            {
                Debug.Log("Installing Dependencies.....");
                InstallModules();
            }
        }

        private static void OnRegisteringPackages(PackageRegistrationEventArgs obj)
        {
            Events.registeringPackages -= OnRegisteringPackages;
        }
        
        private static void InstallModules()
        {
            EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, INSTALLING_MODULES, 0);
            Thread.Sleep(THREAD_SLEEP_TIME);

            DependenciesInfo[] missingDependencies = GetMissingDependencies();
            
            if (missingDependencies.Length > 0)
            {
                var installedModuleCount = 0f;

                foreach (DependenciesInfo module in missingDependencies)
                {
                    var progress = installedModuleCount++ / missingDependencies.Length;
                    EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, $"Installing module {module.name}", progress);
                    AddModuleRequest(module.gitUrl);
                }

                EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, ALL_MODULES_ARE_INSTALLED, 1);
                Thread.Sleep(THREAD_SLEEP_TIME);
            }

            EditorUtility.ClearProgressBar();
        }

        private static void AddModuleRequest(string moduleGitUrl)
        {
            var startTime = Time.realtimeSinceStartup;
            AddRequest addRequest = Client.Add(moduleGitUrl);
            
            while (!addRequest.IsCompleted && Time.realtimeSinceStartup - startTime < TIMEOUT_FOR_MODULE_INSTALLATION)
                Thread.Sleep(THREAD_SLEEP_TIME);


            if (Time.realtimeSinceStartup - startTime >= TIMEOUT_FOR_MODULE_INSTALLATION)
            {
                Debug.LogError($"Package installation timed out for {moduleGitUrl}. Please try again.");
            }
            if (addRequest.Error != null)
            {
                AssetDatabase.Refresh();
                CompilationPipeline.RequestScriptCompilation();
                Debug.LogError("Error: " + addRequest.Error.message);
            }
        }

        private static DependenciesInfo[] GetMissingDependencies()
        {
            PackageInfo[] installed = GetPackageList();
            IEnumerable<DependenciesInfo> missing =
                DependenciesList.Dependencies.Where(e => installed.All(i => e.name != i.name));
            return missing.ToArray();
        }

        private static PackageInfo[] GetPackageList()
        {
            ListRequest listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
                Thread.Sleep(THREAD_SLEEP_TIME);

            if (listRequest.Error != null)
            {
                return Array.Empty<PackageInfo>();
            }

            return listRequest.Result.ToArray();
        }
    }
    
    // public static class ProjectPrefs
    // {
    //     public const string FIRST_TIME_SETUP_DONE = "first-time-setup-guide";
    //
    //     public static bool GetBool(string key)
    //     {
    //         return EditorPrefs.GetBool($"{Application.dataPath}{key}");
    //     }
    //
    //     public static void SetBool(string key, bool value)
    //     {
    //         EditorPrefs.SetBool($"{Application.dataPath}{key}", value);
    //     }
    // }
}
