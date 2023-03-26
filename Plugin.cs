using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Valve.VR;

namespace OutwardVR;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        PlayerPrefs.SetInt("XBOX_EN", 1);
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        InitSteamVR();

        // POSES
        
        SteamVR_Actions._default.RightGrip.AddOnStateDownListener(TriggerLeftDown, SteamVR_Input_Sources.Any);
    }

    public static void TriggerLeftDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        //Logs.WriteInfo("TriggerLeft is Down");
        LogBinds();
    }

    public static void LogBinds()
    {
        
        Controllers.LogAllGameActions(ReInput.players.AllPlayers[1]);
    }

    private static void InitSteamVR()
    {
        SteamVR_Actions.PreInitialize();
        SteamVR.Initialize();
        SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;
    }
}
