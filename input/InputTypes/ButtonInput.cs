using Rewired;
using Valve.VR;

namespace OutwardVR
{
    internal class ButtonInput : BaseInput
    {
        protected SteamVR_Action_Boolean buttonAction;
        protected int buttonID;

        internal override string BindingString => buttonAction.localizedOriginName;

        internal override bool IsBound => buttonAction.activeBinding;

        internal ButtonInput(SteamVR_Action_Boolean buttonAction, int buttonID)
        {
            this.buttonAction = buttonAction;
            this.buttonID = buttonID;
        }

        internal override void UpdateValues(CustomController vrController)
        {
            vrController.SetButtonValueById(buttonID, buttonAction.state);
            //Logs.WriteInfo("Is Button down?: ");
            //Logs.WriteInfo(vrController.GetButtonDownById(buttonID));
        }
    }
}
