using Rewired;
using Valve.VR;

namespace OutwardVR
{
    internal class VectorButton : BaseInput
    {
        protected SteamVR_Action_Vector2 vectorAction;
        protected int buttonID;

        internal override string BindingString => vectorAction.localizedOriginName;

        internal override bool IsBound => vectorAction.activeBinding;

        internal VectorButton(SteamVR_Action_Vector2 vectorAction, int buttonID)
        {
            this.vectorAction = vectorAction;
            this.buttonID = buttonID;
        }

        internal override void UpdateValues(CustomController vrController)
        {
            // Make it so the right joystick up only registers when its definitely being pushed up on purpose
            if (vectorAction.axis.y > 0.7f)
                vrController.SetButtonValueById(buttonID, true);
            else
                vrController.SetButtonValueById(buttonID, false);
        }
    }
}
