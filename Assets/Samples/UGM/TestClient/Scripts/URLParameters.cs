using System;
using System.Collections.Generic;
using UnityEngine;

public class URLParameters : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var args = GetArguments();
        foreach (var arg in args)
        {
            Debug.Log(arg.Key + ": " + arg.Value);
        }
    }

    public static Dictionary<string, string> GetArguments()
    {
        var arguments = new Dictionary<string, string>();

#if (UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
        // URL with parameters syntax: http://example.com?arg1=value1&arg2=value2
        if (Application.absoluteURL.Contains("?"))
        {
            string parameters = Application.absoluteURL.Substring(Application.absoluteURL.IndexOf("?") + 1);
            string[] parameterPairs = parameters.Split('&');
            foreach (string pair in parameterPairs)
            {
                string[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = Uri.UnescapeDataString(keyValue[0]);
                    string value = Uri.UnescapeDataString(keyValue[1]);
                    arguments[key] = value;
                }
            }
        }
#else
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        for (int i = 1; i < commandLineArgs.Length; i += 2)
        {
            arguments[commandLineArgs[i]] = commandLineArgs[i + 1];
        }
#endif

        return arguments;
    }
}
