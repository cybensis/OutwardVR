using OutwardVR.camera;
using Rewired;
using Valve.VR;

namespace OutwardVR
{
    internal class VectorInput : BaseInput
    {
        protected SteamVR_Action_Vector2 vectorAction;
        protected int xAxisID;
        protected int yAxisID;

        internal override string BindingString => vectorAction.localizedOriginName;

        internal override bool IsBound => vectorAction.activeBinding;

        internal VectorInput(SteamVR_Action_Vector2 vectorAction, int xAxisID, int yAxisID)
        {
            this.vectorAction = vectorAction;
            this.xAxisID = xAxisID;
            this.yAxisID = yAxisID;
        }

        internal override void UpdateValues(CustomController vrController)
        {
            vrController.SetAxisValueById(yAxisID, vectorAction.axis.y);

            if (xAxisID == Controllers.LeftJoyStickHor)
                    vrController.SetAxisValueById(xAxisID, vectorAction.axis.x);
            else if (xAxisID == Controllers.RightJoyStickHor) {
                if (FirstPersonCamera.enemyTargetActive)
                    vrController.SetAxisValueById(xAxisID, SteamVR_Actions._default.LeftJoystick.axis.x);
                else
                    vrController.SetAxisValueById(xAxisID, vectorAction.axis.x);
            }

        }
    }
}
