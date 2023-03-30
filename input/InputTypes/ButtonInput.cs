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
            // If the left grip is held down, then you can't use any button besides LeftGrip, RightGrip, A, B, X, Y and Back (To open inv since its left+right grip)
            if (buttonID >= 12 && buttonID <= 20)
                vrController.SetButtonValueById(buttonID, buttonAction.state);
            else { 
                if (SteamVR_Actions._default.LeftGrip.GetState(SteamVR_Input_Sources.Any))
                    vrController.SetButtonValueById(buttonID, false);
                else
                    vrController.SetButtonValueById(buttonID, buttonAction.state);
            }
            //Logs.WriteInfo("Is Button down?: ");
            //Logs.WriteInfo(vrController.GetButtonDownById(buttonID));
        }
    }
}
