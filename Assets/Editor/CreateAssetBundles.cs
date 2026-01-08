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
            Dictionary<string, SerializedGameObject> prefabsToRestore = new Dictionary<string, SerializedGameObject>();

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

                    using(PrefabUtility.EditPrefabContentsScope scope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                    {
                        GameObject prefabRoot = scope.prefabContentsRoot;

                        // Serialize all custom components on the prefab while also destroying them
                        SerializedGameObject serializedPrefab = PrefabSerializer.SerializePrefab(prefabRoot, true);

                        string assetName = Path.GetFileName(assetPath).ToLower();
                        serializedComponents[assetName] = serializedPrefab;
                        prefabsToRestore[assetPath] = serializedPrefab;
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

            // Restore the deleted components on each prefab we modified
            foreach(KeyValuePair<string, SerializedGameObject> pair in prefabsToRestore)
            {
                string assetPath = pair.Key;
                SerializedGameObject serialized = pair.Value;

                using(PrefabUtility.EditPrefabContentsScope scope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                {
                    GameObject prefabRoot = scope.prefabContentsRoot;
                    PrefabInitializer.AddMissingComponents(prefabRoot, serialized);
                }
            }
        }
    }
}