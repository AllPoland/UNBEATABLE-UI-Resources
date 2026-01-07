using UnityEngine;
using UnityEditor;
using System.IO;

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
                Debug.LogError($"The AssetBundle directory ({AssetBundleDirectory}) does not exist!");
                return;
            }

            BuildPipeline.BuildAssetBundles(AssetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }
    }
}