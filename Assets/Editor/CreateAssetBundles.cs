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
                return;
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(AssetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

            string[] bundles = manifest.GetAllAssetBundles();
            foreach(string bundleName in bundles)
            {
                Debug.Log($"Generating serialized components for bundle: {bundleName}");
                Dictionary<string, SerializedGameObject> serializedComponents = new Dictionary<string, SerializedGameObject>();

                AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(AssetBundleDirectory, bundleName));
                string[] assets = bundle.GetAllAssetNames();
                foreach(string assetName in assets)
                {
                    if(!assetName.EndsWith(".prefab"))
                    {
                        continue;
                    }

                    GameObject prefab = bundle.LoadAsset<GameObject>(assetName);
                    string shortName = Path.GetFileName(assetName).ToLower();
                    serializedComponents[shortName] = PrefabSerializer.SerializePrefab(prefab);
                }

                string jsonName = bundleName + "-components.json";
                string json = JsonConvert.SerializeObject(serializedComponents);
                File.WriteAllText(Path.Combine(AssetBundleDirectory, jsonName), json);

                bundle.Unload(true);
            }
        }
    }
}