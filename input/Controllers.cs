using Mono.Cecil.Cil;
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
                    new VectorInput(SteamVR_Actions._default.LeftJoystick, LeftJoyStickHor, LeftJoyStickVert), // left joystick
                    new VectorInput(SteamVR_Actions._default.RightJoystick, RightJoyStickHor, RightJoyStickVert), // right joystick
                    new ButtonInput(SteamVR_Actions._default.ButtonA, ButtonA), // right A button click
                    new ButtonInput(SteamVR_Actions._default.ButtonB, ButtonB), // right B button click
                    new ButtonInput(SteamVR_Actions._default.ButtonX, ButtonX), // left X button click
                    new ButtonInput(SteamVR_Actions._default.ButtonY, ButtonY), // left Y button click
                    new AxisInput(SteamVR_Actions._default.LeftTrigger, LeftTrigger), //  left trigger
                    new AxisInput(SteamVR_Actions._default.RightTrigger, RightTrigger), //  right trigger
                    new ButtonInput(SteamVR_Actions._default.LeftGrip, LeftGrip), // left Y button long press
                    new ButtonInput(SteamVR_Actions._default.ClickLeftJoystick, ClickLeftJoystick), // click left joystick
                    new ButtonInput(SteamVR_Actions._default.ClickRightJoystick, ClickRightJoystick), // click right joystick
                    new ButtonInput(SteamVR_Actions._default.RightGrip, RightGrip),
                    new ButtonInput(SteamVR_Actions._default.NorthDPAD, NorthDPAD),
                    new ButtonInput(SteamVR_Actions._default.EastDPAD, EastDPAD),
                    new ButtonInput(SteamVR_Actions._default.SouthDPAD, SouthDPAD),
                    new ButtonInput(SteamVR_Actions._default.WestDPAD, WestDPAD),
                    new ButtonInput(SteamVR_Actions._default.Back, Back)

                    //new ButtonInput(SteamVR_Actions.default_nexttarget, 14) // right joystick in DPAD mode? pressed east
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
        public static int LeftJoyStickHor = 0;
        public static int LeftJoyStickVert = 1;
        public static int RightJoyStickHor = 2;
        public static int RightJoyStickVert = 3;
        public static int ButtonA = 4;
        public static int ButtonB = 5;
        public static int ButtonX = 6;
        public static int ButtonY = 7;
        public static int LeftTrigger = 8;
        public static int RightTrigger = 9;
        public static int LeftGrip = 10;
        public static int ClickLeftJoystick = 11;
        public static int ClickRightJoystick = 12;
        public static int RightGrip = 13;
        public static int NorthDPAD = 14;
        public static int EastDPAD = 15;
        public static int SouthDPAD = 16;
        public static int WestDPAD = 17;
        public static int Back = 18;
    }
}
