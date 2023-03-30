using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OutwardVR
{
    internal static class RewiredAddons
    {

        internal static CustomController CreateRewiredController()
        {
            HardwareControllerMap_Game hcMap = new HardwareControllerMap_Game(
                "VRControllers",
                new ControllerElementIdentifier[]
                {
                    // The ID values here are from Controllers
                    new ControllerElementIdentifier(Controllers.LeftJoyStickHor, "MoveX", "MoveXPos", "MoveXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(Controllers.LeftJoyStickVert, "MoveY", "MoveYPos", "MoveYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(Controllers.RightJoyStickHor, "MoveCameraX", "MoveCameraXPos", "MoveCameraXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(Controllers.RightJoyStickVert, "MoveCameraY", "MoveCameraYPos", "MoveCameraYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(Controllers.ButtonA, "ButtonA", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.ButtonB, "ButtonB", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.ButtonX, "ButtonX", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.ButtonY, "ButtonY", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.LeftTrigger, "LeftTrigger", "", "", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(Controllers.RightTrigger, "RightTrigger", "", "", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(Controllers.LeftGrip, "LeftGrip", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.LeftGripDouble, "LeftGripDouble", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.LeftGripHold, "LeftGripHold", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.ClickLeftJoystick, "ClickLeftJoystick", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.ClickRightJoystick, "ClickRightJoystick", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.RightGrip, "RightGrip", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.NorthDPAD, "NorthDPAD", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.EastDPAD, "EastDPAD", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.SouthDPAD, "SouthDPAD", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(Controllers.WestDPAD, "WestDPAD", "", "", ControllerElementType.Button, true),
                     new ControllerElementIdentifier(Controllers.Back, "Back", "", "", ControllerElementType.Button, true)
                },
           
                new int[] { },
                new int[] { },
                new AxisCalibrationData[]
                {
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, 0, 1, false, true), //analog trigger
                    new AxisCalibrationData(true, 0.1f, 0, 0, 1, false, true) //analog trigger
                },
                new AxisRange[]
                {
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Positive, //analog trigger
                    AxisRange.Positive //analog trigger
                },
                new HardwareAxisInfo[]
                {
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),  //analog trigger
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None)  //analog trigger
                },
                new HardwareButtonInfo[] { },
                null
            );

            ReInput.UserData.AddCustomController();
            CustomController_Editor newController = ReInput.UserData.customControllers.Last();
            newController.name = "VRControllers";
            foreach (ControllerElementIdentifier element in hcMap.elementIdentifiers.Values)
            {
                if (element.elementType == ControllerElementType.Axis)
                {
                    newController.AddAxis();
                    newController.elementIdentifiers.RemoveAt(newController.elementIdentifiers.Count - 1);
                    newController.elementIdentifiers.Add(element);
                    CustomController_Editor.Axis newAxis = newController.axes.Last();
                    newAxis.name = element.name;
                    newAxis.elementIdentifierId = element.id;
                    newAxis.deadZone = hcMap.hwAxisCalibrationData[newController.axisCount - 1].deadZone;
                    newAxis.zero = 0;
                    newAxis.min = hcMap.hwAxisCalibrationData[newController.axisCount - 1].min;
                    newAxis.max = hcMap.hwAxisCalibrationData[newController.axisCount - 1].max;
                    newAxis.invert = hcMap.hwAxisCalibrationData[newController.axisCount - 1].invert;
                    newAxis.axisInfo = hcMap.hwAxisInfo[newController.axisCount - 1];
                    newAxis.range = hcMap.hwAxisRanges[newController.axisCount - 1];
                }
                else if (element.elementType == ControllerElementType.Button)
                {
                    newController.AddButton();
                    newController.elementIdentifiers.RemoveAt(newController.elementIdentifiers.Count - 1);
                    newController.elementIdentifiers.Add(element);
                    CustomController_Editor.Button newButton = newController.buttons.Last();
                    newButton.name = element.name;
                    newButton.elementIdentifierId = element.id;
                }
            }

            CustomController customController = ReInput.controllers.CreateCustomController(newController.id);

            customController.useUpdateCallbacks = false;

            return customController;
        }
        internal static CustomControllerMap CreateGameplayMap(int controllerID) {
            List<ActionElementMap> t2 = new List<ActionElementMap>() {
                new ActionElementMap(AttackOneID , ControllerElementType.Button, Controllers.ButtonA , Pole.Positive, AxisRange.Positive, false), 
                new ActionElementMap(AttackTwoID , ControllerElementType.Button, Controllers.ButtonB, Pole.Positive, AxisRange.Positive, false), 
                new ActionElementMap(StealthID , ControllerElementType.Button, Controllers.ButtonX, Pole.Positive, AxisRange.Positive, false), 
                new ActionElementMap(InteractID , ControllerElementType.Button, Controllers.ButtonY, Pole.Positive, AxisRange.Positive, false), 
                new ActionElementMap(LockToggleID , ControllerElementType.Button, Controllers.ClickRightJoystick, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(BlockID , ControllerElementType.Button, Controllers.RightGrip, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(ChargeWeaponID , ControllerElementType.Button, Controllers.RightGrip, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(DodgeID, ControllerElementType.Button, Controllers.LeftGripDouble, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(SprintID, ControllerElementType.Button, Controllers.ClickLeftJoystick, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(HandleBagID, ControllerElementType.Button, Controllers.NorthDPAD, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(SheatheID, ControllerElementType.Button, Controllers.SouthDPAD, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(ToggleLightsID, ControllerElementType.Button, Controllers.WestDPAD, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(ChargeWeaponID, ControllerElementType.Button, Controllers.RightTrigger, Pole.Positive, AxisRange.Positive, false),
                // Missing Aim and AutoRun
            };
            return CreateCustomMap("Gameplay", 5, controllerID, t2);
        }

        internal static CustomControllerMap CreateMenuMap(int controllerID)
        {
            List<ActionElementMap> t2 = new List<ActionElementMap>() {
                new ActionElementMap(QuickActionID, ControllerElementType.Button, Controllers.ButtonA, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(ShowOptionsID, ControllerElementType.Button, Controllers.ButtonX, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(MenuVertID, ControllerElementType.Button, Controllers.NorthDPAD, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(MenuHorID, ControllerElementType.Button, Controllers.EastDPAD, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(MenuVertID, ControllerElementType.Button, Controllers.SouthDPAD, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(MenuHorID, ControllerElementType.Button, Controllers.WestDPAD, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(PreviousMenuID, ControllerElementType.Button, Controllers.LeftGrip, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(NextMenuID, ControllerElementType.Button, Controllers.RightGrip, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(QuickDialogueID, ControllerElementType.Button, Controllers.ButtonA, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(QuickDialogueID, ControllerElementType.Button, Controllers.ButtonY, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(TakeAllID, ControllerElementType.Button, Controllers.ButtonY, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(InfoID, ControllerElementType.Button, Controllers.ButtonY, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(DeleteID, ControllerElementType.Button, Controllers.ButtonX, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(CompareID, ControllerElementType.Button, Controllers.ClickRightJoystick, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(ToggleDetailPanelID, ControllerElementType.Button, Controllers.ClickLeftJoystick, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(PreviousFilterID, ControllerElementType.Button, Controllers.LeftTrigger, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(NextFilterID, ControllerElementType.Button, Controllers.RightTrigger, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(MenuHorID, ControllerElementType.Button, Controllers.LeftJoyStickHor, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(MenuVertID, ControllerElementType.Button, Controllers.LeftJoyStickVert, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(VC_HorScrollID, ControllerElementType.Button, Controllers.RightJoyStickHor, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(VC_VertScrollID, ControllerElementType.Button, Controllers.RightJoyStickVert, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(VC_RightClickID, ControllerElementType.Button, Controllers.ButtonX, Pole.Positive, AxisRange.Positive, false),

            };
            return CreateCustomMap("Menu", 2, controllerID, t2);
        }


        internal static CustomControllerMap CreateQuickSlotMap(int controllerID)
        {
            List<ActionElementMap> t2 = new List<ActionElementMap>() {
                 new ActionElementMap(QuickSlotOneID, ControllerElementType.Button, Controllers.ButtonA, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(QuickSlotTwoID, ControllerElementType.Button, Controllers.ButtonB, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(QuickSlotThreeID, ControllerElementType.Button, Controllers.ButtonX, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(QuickSlotFourID, ControllerElementType.Button, Controllers.ButtonY, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(QuickSlotToggleOneID, ControllerElementType.Button, Controllers.LeftTrigger, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(QuickSlotToggleTwoID, ControllerElementType.Button, Controllers.RightTrigger, Pole.Positive, AxisRange.Positive, false),

            };
            return CreateCustomMap("QuickSlot", 3, controllerID, t2);
        }

        internal static CustomControllerMap CreateVCMap(int controllerID)
        {
            List<ActionElementMap> t2 = new List<ActionElementMap>() {
                new ActionElementMap(VC_LeftClickID, ControllerElementType.Button, Controllers.ButtonA, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(VC_VertID, ControllerElementType.Button, Controllers.LeftJoyStickVert, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(VC_HorID, ControllerElementType.Button, Controllers.LeftJoyStickHor, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(VC_HorScrollID, ControllerElementType.Button, Controllers.RightJoyStickHor, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(VC_VertScrollID, ControllerElementType.Button, Controllers.RightJoyStickVert, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(VC_RightClickID, ControllerElementType.Button, Controllers.ButtonX, Pole.Positive, AxisRange.Positive, false),
            };
            return CreateCustomMap("VC", 7, controllerID, t2);
        }

        internal static CustomControllerMap CreateDeployMap(int controllerID)
        {
            List<ActionElementMap> t2 = new List<ActionElementMap>() {
                new ActionElementMap(ConfirmDeployID, ControllerElementType.Button, Controllers.ButtonA, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(CancelDeployID, ControllerElementType.Button, Controllers.ButtonB, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(RotateDeployLeftID, ControllerElementType.Button, Controllers.LeftGrip, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(RotateDeployRightID, ControllerElementType.Button, Controllers.RightGrip, Pole.Positive, AxisRange.Positive, false),
            };
            return CreateCustomMap("Deploy", 6, controllerID, t2);
        }


        internal static CustomControllerMap CreateCameraMoveMap(int controllerID)
        {
            List<ActionElementMap> t2 = new List<ActionElementMap>() {
                new ActionElementMap(CameraMoveHorID, ControllerElementType.Axis  , Controllers.RightJoyStickHor , Pole.Positive, AxisRange.Full, false), //MoveCameraHor
                new ActionElementMap(CameraMoveVertID, ControllerElementType.Axis  , Controllers.RightJoyStickVert , Pole.Positive, AxisRange.Full, false), //MoveCameraVer
            };
            return CreateCustomMap("Camera", 9, controllerID, t2);
        }



        internal static CustomControllerMap CreateOtherControlsMap(int controllerID)
        {
            List<ActionElementMap> t2 = new List<ActionElementMap>() {
                // This uses start button on a controller, find something to remap this to
                //new ActionElementMap(HelpID, ControllerElementType.Button, Controllers.ButtonB, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(CancelID, ControllerElementType.Button, Controllers.ButtonB, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(EnableSaveSelectionOneID, ControllerElementType.Button, Controllers.LeftGrip, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(EnableSaveSelectionTwoID, ControllerElementType.Button, Controllers.RightGrip, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(EnableSaveSelectionThreeID, ControllerElementType.Button, Controllers.LeftTrigger, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(EnableSaveSelectionFourID, ControllerElementType.Button, Controllers.RightTrigger, Pole.Positive, AxisRange.Positive, false),
            };
            return CreateCustomMap("Other", 0, controllerID, t2);
        }


        internal static CustomControllerMap CreateOtherDPADMap(int controllerID)
        {
            List<ActionElementMap> t2 = new List<ActionElementMap>() {
                new ActionElementMap(ToggleInventoryID, ControllerElementType.Button, Controllers.Back , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(ToggleMapID, ControllerElementType.Button, Controllers.EastDPAD, Pole.Positive, AxisRange.Positive, false),
            };
            return CreateCustomMap("OtherDPAD", 11, controllerID, t2);
        }


        internal static CustomControllerMap CreateMovementMap(int controllerID)
        {
            List<ActionElementMap> t1 = new List<ActionElementMap>() {
                new ActionElementMap(MoveHorID , ControllerElementType.Axis  , Controllers.RightJoyStickHor , Pole.Positive, AxisRange.Full, false), //MoveHor
                new ActionElementMap(MoveVertID , ControllerElementType.Axis  , Controllers.LeftJoyStickVert , Pole.Positive, AxisRange.Full, false), //MoveVer
            };
            return CreateCustomMap("Movement", 4, controllerID, t1);

        }

        



        private static CustomControllerMap CreateCustomMap(string mapName, int categoryId, int controllerId, List<ActionElementMap> actionElementMaps)
        {
            //ReInput.UserData.CreateCustomControllerMap(categoryId, controllerId, 0);
            ReInput.UserData.CreateCustomControllerMap(categoryId, controllerId, 0);
            ControllerMap_Editor newMap = ReInput.UserData.customControllerMaps.Last();

            newMap.name = mapName;

            foreach (ActionElementMap elementMap in actionElementMaps)
            {
                newMap.AddActionElementMap();
                ActionElementMap newElementMap = newMap.GetActionElementMap(newMap.ActionElementMaps.Count() - 1);
                newElementMap.actionId = elementMap.actionId;
                newElementMap.elementType = elementMap.elementType;
                newElementMap.elementIdentifierId = elementMap.elementIdentifierId;
                newElementMap.axisContribution = elementMap.axisContribution;
                if (elementMap.elementType == ControllerElementType.Axis)
                    newElementMap.axisRange = elementMap.axisRange;
                newElementMap.invert = elementMap.invert;
            }
            return ReInput.UserData.ZkdVVpddavmruicKaOtoiXbLmnO(categoryId, controllerId, 0);
        }
        // Rewired ActionID Mapping
        public static int QuickActionID = 18;
        public static int ShowOptionsID = 19;
        public static int CancelID = 22;
        public static int ToggleInventoryID = 24;
        public static int QuickSlotToggleOneID = 35;
        public static int QuickSlotToggleTwoID = 36;
        public static int QuickSlotOneID = 37;
        public static int QuickSlotTwoID = 38;
        public static int QuickSlotThreeID = 39;
        public static int QuickSlotFourID = 40;
        public static int TakeAllID = 42;
        public static int HelpID = 45;
        public static int MenuVertID = 46;
        public static int MenuHorID = 47;
        public static int SwitchTargetHorID = 51;
        public static int SwitchTargetVertID = 52;
        public static int MoveHorID = 63;
        public static int MoveVertID = 64;
        public static int InteractID = 69;
        public static int AttackOneID = 70;
        public static int AttackTwoID = 71;
        public static int DodgeID = 72;
        public static int BlockID = 73;
        public static int SprintID = 74;
        public static int LockToggleID = 76;
        public static int SheatheID = 79;
        public static int StealthID = 80;
        public static int ConfirmDeployID = 81;
        public static int CancelDeployID = 82;
        public static int PreviousMenuID = 87;
        public static int NextMenuID = 88;
        public static int QuickDialogueID = 89;
        public static int ChargeWeaponID = 91;
        public static int VC_LeftClickID = 92;
        public static int VC_RightClickID = 93;
        public static int VC_HorID = 95;
        public static int VC_VertID = 96;
        public static int AimID = 99;
        public static int ToggleMapID = 103;
        public static int VC_HorScrollID = 105;
        public static int VC_VertScrollID = 106;
        public static int InfoID = 109;
        public static int ToggleLightsID = 111;
        public static int HandleBagID = 112;
        public static int AutoRunID = 114;
        public static int DeleteID = 115;
        public static int CameraMoveHorID = 124;
        public static int CameraMoveVertID = 125;
        public static int CompareID = 126;
        public static int PreviousFilterID = 127;
        public static int NextFilterID = 128;
        public static int EnableSaveSelectionOneID = 129;
        public static int EnableSaveSelectionTwoID = 130;
        public static int EnableSaveSelectionThreeID = 131;
        public static int EnableSaveSelectionFourID = 132;
        public static int EnableSaveSelectionFiveID = 133;
        public static int ToggleDetailPanelID = 135;
        public static int RotateDeployLeftID = 136;
        public static int RotateDeployRightID = 137;


    }
}
