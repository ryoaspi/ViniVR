using UnityEngine;
using System.IO;

namespace TheFoundation.Runtime
{
    
    public static class SaveSystem
    {
        public static void SaveItem<T>(T item, string path)
        {
            string completePath = path;
        
            string directory = Path.GetDirectoryName(completePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        
            string json = JsonUtility.ToJson(item);
            File.WriteAllText(completePath, json);
            Debug.Log($"Saved to: {completePath}");
        }

        public static T LoadItem<T>(string path)
        {
            string completePath = path;
            if (!File.Exists(completePath))
            {
                Debug.Log($"<color=red>No save found at:</color> {completePath}");
                return default;
            }

            string json = File.ReadAllText(completePath);
            Debug.Log($"<color=green>Data :</color> {json}");
            return JsonUtility.FromJson<T>(json);
        }
    }

}
