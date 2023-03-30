using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OutwardVR
{
    class AssetLoader
    {
        private const string assetsDir = "OutwardVRAssets/AssetBundles/";

        public static GameObject Skybox;
        public static GameObject LeftHandBase;
        public static GameObject RightHandBase;

        public AssetLoader()
        {
            var SkyboxBundle = LoadBundle("skyboxassetbundle");
            LeftHandBase = LoadAsset<GameObject>(SkyboxBundle, "SteamVR/Prefabs/vr_glove_left_model_slim.prefab");
            RightHandBase = LoadAsset<GameObject>(SkyboxBundle, "SteamVR/Prefabs/vr_glove_right_model_slim.prefab");
        }

        private T LoadAsset<T>(AssetBundle bundle, string prefabName) where T : UnityEngine.Object
        {
            var asset = bundle.LoadAsset<T>($"Assets/{prefabName}");
            if (asset)
                return asset;
            else
            {
                return null;
            }
                
        }

        private static AssetBundle LoadBundle(string assetName)
        {
            var myLoadedAssetBundle =
                AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, Path.Combine(assetsDir, assetName)));

            if (myLoadedAssetBundle == null)
            {
                return null;
            }

            return myLoadedAssetBundle;
        }

    }
}
