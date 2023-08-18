#if UNITY_EDITOR
using System;
using UGM.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#endif
namespace UGM.Editor.UGMTool
{
#if UNITY_EDITOR
    public class UGMEditorWindow : EditorWindow
    {
        private UGMConfig config;
        private const string CONFIG_FILE_NAME = "UGM-Config";
        private bool IsConfigExist => config != null;
        [MenuItem("Window/UGM/Configuration")]
        public static void OpenWindow()
        {
            var window = GetWindow<UGMEditorWindow>("UGM Configuration");
            window.maxSize = new Vector2(260f, 300f);
            window.minSize = window.maxSize;
            window.Show();
        }

        private void OnEnable()
        {
            SetupConfiguration();
            CreateLabel("Thank you for installing UGM.", 10f);
            CreateLabel("To use UGM, you will need API key to \nmake request to the API server. \nDownload the sample scene on \nPackage Manager UGM.", 10);
            CreateTextFieldForApiKey();
            CreateSaveButton();
            CreateLocateConfigurationAssetButton();
        }

        private void SetupConfiguration()
        {
            config = Resources.Load<UGMConfig>(CONFIG_FILE_NAME);
            if (config == null)
            {
                CreateUGMConfiguration();
            }
        }

        

        private void CreateUGMConfiguration()
        {
            if (AssetDatabase.IsValidFolder("Assets/Resources") == false)
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            config = ScriptableObject.CreateInstance<UGMConfig>();
            AssetDatabase.CreateAsset(config, $"Assets/Resources/{CONFIG_FILE_NAME}.asset");
            AssetDatabase.SaveAssets();
        }

        private void CreateLocateConfigurationAssetButton()
        {
            Button locateButton = new Button(LocateConfig);
            locateButton.text = "Locate Config";
            rootVisualElement.Add(locateButton);
        }

        private void LocateConfig()
        {
            Debug.Log("Configuration will show on project tab");
            if (IsConfigExist == false)
            {
                Debug.LogError("Configuration Asset doesn't exist");
                return;
            }
            EditorGUIUtility.PingObject(config);
        }

        private void CreateSaveButton()
        {
            Button saveButton = new Button();
            saveButton.text = "Save";
            rootVisualElement.Add(saveButton);
        }


        private void CreateLabel(string str, float marginTop = 3f, Align align = Align.Center)
        {
            Label label = new Label(str);
            label.style.marginTop = new StyleLength(Length.Percent(marginTop));
            label.style.alignSelf = new StyleEnum<Align>(align);
            rootVisualElement.Add(label);
        }

        private void CreateTextFieldForApiKey()
        {
            CreateLabel("API Key:", 8, Align.FlexStart);
            TextField textFieldApi = new TextField();
            textFieldApi.style.marginTop = new StyleLength(Length.Percent(2));
            if (IsConfigExist)
            {
                SetApiKeyValueToTextfield(textFieldApi);
                RegisterOnValueChanged(textFieldApi);
            }
            
            rootVisualElement.Add(textFieldApi);
        }

        private void RegisterOnValueChanged(TextField textFieldApi)
        {
            textFieldApi.RegisterValueChangedCallback((e) => config.apiKey = e.newValue);
        }

        private void SetApiKeyValueToTextfield(TextField textFieldApi)
        {
            if (string.IsNullOrEmpty(config.apiKey) == false)
            {
                textFieldApi.SetValueWithoutNotify(config.apiKey);
            }
        }
    }
#endif
}