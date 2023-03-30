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
            //Make it so the joystick movements only register beyond .3
            if (vectorAction.axis.y < -0.3f || vectorAction.axis.y > 0.3f)
                vrController.SetAxisValueById(yAxisID, vectorAction.axis.y);
            else
                vrController.SetAxisValueById(yAxisID, 0f);


            if (xAxisID == Controllers.LeftJoyStickHor) {
                if (vectorAction.axis.x < -0.3f || vectorAction.axis.x > 0.3f)
                    vrController.SetAxisValueById(xAxisID, vectorAction.axis.x);
                else
                    vrController.SetAxisValueById(xAxisID, 0f);
            }
            else if (xAxisID == Controllers.RightJoyStickHor) {
                if (FirstPersonCamera.enemyTargetActive)
                    vrController.SetAxisValueById(xAxisID, SteamVR_Actions._default.LeftJoystick.axis.x);
                else {
                    if (vectorAction.axis.x < -0.3f || vectorAction.axis.x > 0.3f)
                        vrController.SetAxisValueById(xAxisID, vectorAction.axis.x);
                    else
                        vrController.SetAxisValueById(xAxisID, 0f);
                }
            }

        }
    }
}
