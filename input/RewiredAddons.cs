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
                    new ControllerElementIdentifier(0, "MoveX", "MoveXPos", "MoveXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(1, "MoveY", "MoveYPos", "MoveYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(2, "MoveCameraX", "MoveCameraXPos", "MoveCameraXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(3, "MoveCameraY", "MoveCameraYPos", "MoveCameraYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(4, "confirm", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(5, "decline", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(6, "pause", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(7, "actionbar", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(8, "group", "groupPos", "groupNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(9, "menus", "menusPos", "menusNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(10, "options", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(11, "highlight", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(12, "switchturnbased", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(13, "prevtarget", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(14, "nexttarget", "", "", ControllerElementType.Button, true)
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


        internal static CustomControllerMap CreateGameplayMap(int controllerID)
        {

            List<ActionElementMap> defaultElementMaps = new List<ActionElementMap>()
            {
                new ActionElementMap(63 , ControllerElementType.Axis  , 0 , Pole.Positive, AxisRange.Full, false), //MoveHor
                new ActionElementMap(64 , ControllerElementType.Axis  , 1 , Pole.Positive, AxisRange.Full, false), //MoveVer
                new ActionElementMap(124, ControllerElementType.Axis  , 2 , Pole.Positive, AxisRange.Full, false), //MoveCameraHor
                new ActionElementMap(125, ControllerElementType.Axis  , 3 , Pole.Positive, AxisRange.Full, false), //MoveCameraVer
                new ActionElementMap(8 , ControllerElementType.Button, 4 , Pole.Positive, AxisRange.Positive, false), //Confirm
                new ActionElementMap(9 , ControllerElementType.Button, 5, Pole.Positive, AxisRange.Positive, false), //Decline
                new ActionElementMap(10 , ControllerElementType.Button, 6, Pole.Positive, AxisRange.Positive, false), //Func01 = pause
                new ActionElementMap(11 , ControllerElementType.Button, 7, Pole.Positive, AxisRange.Positive, false), //Func02 = Actionbar
                new ActionElementMap(12 , ControllerElementType.Axis, 8, Pole.Positive, AxisRange.Positive, false), // LeftBottom = group
                new ActionElementMap(13 , ControllerElementType.Axis, 9, Pole.Positive, AxisRange.Positive, false), // RightBottom = menus
                new ActionElementMap(16 , ControllerElementType.Button, 10, Pole.Positive, AxisRange.Positive, false), //Options
                new ActionElementMap(18 , ControllerElementType.Button, 11, Pole.Positive, AxisRange.Positive, false), //Higlight
                new ActionElementMap(19 , ControllerElementType.Button, 12, Pole.Positive, AxisRange.Positive, false), //Switch Turn Based
                new ActionElementMap(14 , ControllerElementType.Button, 13, Pole.Positive, AxisRange.Positive, false), //Prev Target
                new ActionElementMap(15 , ControllerElementType.Button, 14, Pole.Positive, AxisRange.Positive, false) //Next Target
            };

            return CreateCustomMap("VRDefault", 0, controllerID, defaultElementMaps);

        }



        private static CustomControllerMap CreateCustomMap(string mapName, int categoryId, int controllerId, List<ActionElementMap> actionElementMaps)
        {
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


    }
}
