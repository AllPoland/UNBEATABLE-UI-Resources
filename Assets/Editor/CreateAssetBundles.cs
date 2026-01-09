using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UBUI.Serialization;
using Newtonsoft.Json;

namespace UBUI
{
    public class CreateAssetBundles
    {
        public const string AssetBundleDirectory = "Assets/AssetBundles";
        public const string BackupDirectory = "Assets/PrefabBackups";


        [MenuItem("Assets/Build AssetBundles")]
        public static void BuildAllAssetBundles()
        {
            if(!Directory.Exists(AssetBundleDirectory))
            {
                Debug.LogWarning($"The AssetBundle directory ({AssetBundleDirectory}) does not exist!");
                Directory.CreateDirectory(AssetBundleDirectory);
            }

            // Preprocess prefabs by serializing and removing all custom components
            // We serialize them manually since their script references are gone when loaded in a plugin
            string[] bundles = AssetDatabase.GetAllAssetBundleNames();
            Dictionary<string, string> prefabsToRestore = new Dictionary<string, string>();

            foreach(string bundleName in bundles)
            {
                Debug.Log($"Generating serialized components for bundle: {bundleName}");
                Dictionary<string, SerializedGameObject> serializedComponents = new Dictionary<string, SerializedGameObject>();

                string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                foreach(string assetPath in assets)
                {
                    if(!assetPath.EndsWith(".prefab"))
                    {
                        continue;
                    }

                    // Backup the prefab so we can restore it when we're finished
                    string backupPath = Path.Combine(BackupDirectory, assetPath);
                    string directoryName = Path.GetDirectoryName(backupPath);
                    if(!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    prefabsToRestore[backupPath] = assetPath;

                    File.Copy(assetPath, backupPath, true);

                    using(PrefabUtility.EditPrefabContentsScope scope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                    {
                        GameObject prefabRoot = scope.prefabContentsRoot;

                        // Serialize all custom components on the prefab while also destroying them
                        SerializedGameObject serializedPrefab = PrefabSerializer.SerializePrefab(prefabRoot, true);

                        string assetName = Path.GetFileName(assetPath).ToLower();
                        serializedComponents[assetName] = serializedPrefab;
                    }
                }

                // Save the custom components to a json file
                string jsonName = bundleName + "-components.json";
                string json = JsonConvert.SerializeObject(serializedComponents);
                File.WriteAllText(Path.Combine(AssetBundleDirectory, jsonName), json);
            }

            // Build the assetbundles with our removed components - we'll add them back in the plugin
            // Building for StandaloneWindows because the game only has windows builds
            BuildPipeline.BuildAssetBundles(AssetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

            // Restore the originals of each prefab we modified
            foreach(KeyValuePair<string, string> pair in prefabsToRestore)
            {
                string backupPath = pair.Key;
                string assetPath = pair.Value;
                File.Copy(backupPath, assetPath, true);
            }

            try
            {
                Directory.Delete(BackupDirectory, true);
                string metaPath = BackupDirectory + ".meta";
                File.Delete(metaPath);
            }
            catch
            {
                Debug.LogError("Failed to delete backup directory!");
            }
        }
    }
}