using Rewired;
using Valve.VR;

namespace OutwardVR
{
    internal class AxisInput : BaseInput
    {
        protected SteamVR_Action_Single MyAxisAction;
        protected int MyAxisID;

        internal override string BindingString => MyAxisAction.localizedOriginName;

        internal override bool IsBound => MyAxisAction.activeBinding;

        internal AxisInput(SteamVR_Action_Single AxisAction, int AxisID)
        {
            this.MyAxisAction = AxisAction;
            this.MyAxisID = AxisID;
        }

        internal override void UpdateValues(CustomController vrController)
        {
            //Make it so the joystick movements only register beyond .3
            if (MyAxisAction.axis < -0.3f || MyAxisAction.axis > 0.3f)
                vrController.SetAxisValueById(MyAxisID, MyAxisAction.axis);
            else
                vrController.SetAxisValueById(MyAxisID, 0f);
        }
    }
}
