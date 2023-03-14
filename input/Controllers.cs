﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace OutwardVR
{
    public static class Controllers
    {
        public static bool ControllersAlreadyInit = false;


        private static CustomController vrControllers;
        private static CustomControllerMap vrGameplayMap;
        private static CustomControllerMap vrUIMap;

        private static bool hasRecentered;
        private static bool initializedMainPlayer;
        private static bool initializedLocalUser;

        private static BaseInput[] inputs;
        private static List<BaseInput> modInputs = new List<BaseInput>();

        internal static int leftJoystickID { get; private set; }
        internal static int rightJoystickID { get; private set; }

        internal static int ControllerID => vrControllers.id;

        internal static void Init()
        {
            if (!ControllersAlreadyInit)
            {
                ReInput.InputSourceUpdateEvent += UpdateVRInputs;
                SetupControllerInputs();
                ControllersAlreadyInit = true;
            }
        }





        private static void SetupControllerInputs()
        {
            vrControllers = RewiredAddons.CreateRewiredController();
            vrGameplayMap = RewiredAddons.CreateGameplayMap(vrControllers.id);

            inputs = new BaseInput[]
            {
                    new VectorInput(SteamVR_Actions.default_move, 0, 1),
                    new VectorInput(SteamVR_Actions.default_movecamera, 2, 3),
                    new ButtonInput(SteamVR_Actions.default_confirm, 4),
                    new ButtonInput(SteamVR_Actions.default_decline, 5),
                    new ButtonInput(SteamVR_Actions.default_pause, 6),
                    new ButtonInput(SteamVR_Actions.default_actionbar, 7),
                    new AxisInput(SteamVR_Actions.default_group, 8),
                    new AxisInput(SteamVR_Actions.default_menus, 9),
                    new ButtonInput(SteamVR_Actions.default_options, 10),
                    new ButtonInput(SteamVR_Actions.default_highlight, 11),
                    new ButtonInput(SteamVR_Actions.default_switchturnbased, 12),
                    new ButtonInput(SteamVR_Actions.default_prevtarget, 13),
                    new ButtonInput(SteamVR_Actions.default_nexttarget, 14)
            };
        }

        public static void Update()
        {
            if (!initializedMainPlayer)
            {
                Logs.WriteInfo("allPlayerCount: ");
                Logs.WriteInfo(ReInput.players.allPlayerCount);
                Player p = null;
                for (int i = 0; i < ReInput.players.allPlayerCount; i++)
                {
                    if (ReInput.players.AllPlayers[i] != null)
                    {
                        p = ReInput.players.AllPlayers[1];
                        Logs.WriteInfo("found non null Player p with name: ");
                        Logs.WriteInfo(p.name);
                        //break;
                    }

                }
                //p = Kingmaker.Assets.Console.GamepadInput.GamePad.Instance.Player;

                if (AddVRController(p))
                {
                    initializedMainPlayer = true;
                    Logs.WriteInfo("VRController successfully added");
                }
            }

        }

        internal static bool AddVRController(Player inputPlayer)
        {
            if (!inputPlayer.controllers.ContainsController(vrControllers))
            {
                inputPlayer.controllers.AddController(vrControllers, false);
                vrControllers.enabled = true;
            }

            if (inputPlayer.controllers.maps.GetAllMaps(ControllerType.Custom).ToList().Count < 1)
            {
                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, 0, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrGameplayMap);
                if (!vrGameplayMap.enabled)
                    vrGameplayMap.enabled = true;
            }

            return inputPlayer.controllers.ContainsController(vrControllers) && inputPlayer.controllers.maps.GetAllMaps(ControllerType.Custom).ToList().Count >= 1;
        }

        private static void UpdateVRInputs()
        {

            foreach (BaseInput input in inputs)
            {
                input.UpdateValues(vrControllers);
            }

            foreach (BaseInput input in modInputs)
            {
                input.UpdateValues(vrControllers);
            }
        }
    }
}
