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

        public static CustomController vrControllers;
        private static CustomControllerMap vrGameplayMap;
        private static CustomControllerMap vrOtherControlsMap;
        private static CustomControllerMap vrMenuMap;
        private static CustomControllerMap vrMovementMap;
        private static CustomControllerMap vrQuickSlotMap;
        private static CustomControllerMap vrDeployMap;
        private static CustomControllerMap vrVCMap;
        private static CustomControllerMap vrCameraMoveMap;
        private static CustomControllerMap vrOtherDPADMap;

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


        public static void ResetControllerVars() { 
            initializedMainPlayer = false;
            ControllersAlreadyInit = false;
        }



        private static void SetupControllerInputs()
        {
            vrControllers =         RewiredAddons.CreateRewiredController();

            vrGameplayMap =         RewiredAddons.CreateGameplayMap(vrControllers.id);
            vrOtherControlsMap =    RewiredAddons.CreateOtherControlsMap(vrControllers.id);
            vrMenuMap =             RewiredAddons.CreateMenuMap(vrControllers.id);
            vrMovementMap =         RewiredAddons.CreateMovementMap(vrControllers.id);
            vrQuickSlotMap =        RewiredAddons.CreateQuickSlotMap(vrControllers.id);
            vrDeployMap =           RewiredAddons.CreateDeployMap(vrControllers.id);
            vrVCMap =               RewiredAddons.CreateVCMap(vrControllers.id);
            vrCameraMoveMap =       RewiredAddons.CreateCameraMoveMap(vrControllers.id);
            vrOtherDPADMap =        RewiredAddons.CreateOtherDPADMap(vrControllers.id);

            inputs = new BaseInput[]
            {
                    new VectorInput(SteamVR_Actions._default.LeftJoystick, LeftJoyStickHor, LeftJoyStickVert),
                    new VectorInput(SteamVR_Actions._default.RightJoystick, RightJoyStickHor, RightJoyStickVert),
                    new ButtonInput(SteamVR_Actions._default.ButtonA, ButtonA),
                    new ButtonInput(SteamVR_Actions._default.ButtonB, ButtonB),
                    new ButtonInput(SteamVR_Actions._default.ButtonX, ButtonX),
                    new ButtonInput(SteamVR_Actions._default.ButtonY, ButtonY),
                    new AxisInput(SteamVR_Actions._default.LeftTrigger, LeftTrigger),
                    new AxisInput(SteamVR_Actions._default.RightTrigger, RightTrigger),
                    new ButtonInput(SteamVR_Actions._default.LeftGrip, LeftGrip),
                    new ButtonInput(SteamVR_Actions._default.ClickLeftJoystick, ClickLeftJoystick),
                    new ButtonInput(SteamVR_Actions._default.ClickRightJoystick, ClickRightJoystick),
                    new ButtonInput(SteamVR_Actions._default.RightGrip, RightGrip),
                    new ButtonInput(SteamVR_Actions._default.NorthDPAD, NorthDPAD),
                    new ButtonInput(SteamVR_Actions._default.EastDPAD, EastDPAD),
                    new ButtonInput(SteamVR_Actions._default.SouthDPAD, SouthDPAD),
                    new ButtonInput(SteamVR_Actions._default.WestDPAD, WestDPAD),
                    new ButtonInput(SteamVR_Actions._default.Back, Back),
                    new VectorButton(SteamVR_Actions._default.RightJoystick, RightJoyStickUp),
                    new VectorButton(SteamVR_Actions._default.RightJoystick, RightJoyStickDown),
                    new ButtonInput(SteamVR_Actions._default.Start, Start)

                    //new ButtonInput(SteamVR_Actions.default_nexttarget, 14) // right joystick in DPAD mode? pressed east
            };
        }

        public static void Update()
        {
            if (!initializedMainPlayer)
            {
                Player p = ReInput.players.AllPlayers[1];
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

            if (inputPlayer.controllers.maps.GetAllMaps(ControllerType.Custom).ToList().Count < 9)
            {
                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, OtherControlsMapID, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrOtherControlsMap);
                if (!vrOtherControlsMap.enabled)
                    vrOtherControlsMap.enabled = true;

                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, MenuMapID, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrMenuMap);
                if (!vrMenuMap.enabled)
                    vrMenuMap.enabled = true;

                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, QuickSlotMapID, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrQuickSlotMap);
                if (!vrQuickSlotMap.enabled)
                    vrQuickSlotMap.enabled = true;

                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, MovementMapID, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrMovementMap);
                if (!vrMovementMap.enabled)
                    vrMovementMap.enabled = true;

                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, GameplayMapID, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrGameplayMap);
                if (!vrGameplayMap.enabled)
                    vrGameplayMap.enabled = true;

                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, DeployMapID, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrDeployMap);
                if (!vrDeployMap.enabled)
                    vrDeployMap.enabled = true;

                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, VCMapID, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrVCMap);
                if (!vrVCMap.enabled)
                    vrVCMap.enabled = true;

                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, CameraMoveMapID, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrCameraMoveMap);
                if (!vrCameraMoveMap.enabled)
                    vrCameraMoveMap.enabled = true;

                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, OtherDPADMapID, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrOtherDPADMap);
                if (!vrOtherDPADMap.enabled)
                    vrOtherDPADMap.enabled = true;
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
 

        // Controller ID's
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
        public static int ClickRightJoystick = 10;
        public static int ClickLeftJoystick = 11;
        public static int LeftGrip = 12;
        public static int RightGrip = 13;
        public static int NorthDPAD = 14;
        public static int EastDPAD = 15;
        public static int SouthDPAD = 16;
        public static int WestDPAD = 17;
        public static int Back = 18;
        public static int RightJoyStickUp = 19;
        public static int RightJoyStickDown = 20;
        public static int Start = 21;


        // ReWired custom controller category ID's
        public static int OtherControlsMapID = 0;
        public static int MenuMapID = 2;
        public static int QuickSlotMapID = 3;
        public static int MovementMapID = 4;
        public static int GameplayMapID = 5;
        public static int DeployMapID = 6;
        public static int VCMapID = 7;
        public static int CameraMoveMapID = 9;
        public static int OtherDPADMapID = 11;
    }
}
