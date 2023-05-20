using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DependenciesSO", menuName = "ScriptableObjects/DependenciesSO")]
public class DependenciesSO : ScriptableObject
{
    [SerializeField]
    private List<DependencyEntry> dependencyEntries = new List<DependencyEntry>();

    public List<DependencyEntry> DependencyEntries => dependencyEntries;

    public Dictionary<string, string> GetDependenciesDictionary()
    {
        Dictionary<string, string> dependencies = new Dictionary<string, string>();
        foreach (var entry in dependencyEntries)
        {
            dependencies[entry.packageName] = entry.packageURL;
        }
        return dependencies;
    }

    [Serializable]
    public class DependencyEntry
    {
        public string packageName;
        public string packageURL;

        public DependencyEntry(string packageName, string packageURL)
        {
            this.packageName = packageName;
            this.packageURL = packageURL;
        }
    }
}
