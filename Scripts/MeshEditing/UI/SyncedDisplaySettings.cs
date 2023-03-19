using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class SyncedDisplaySettings : UdonSharpBehaviour
    {
        [Header("Unity assingments")]
        [SerializeField] GameObject ScalerTitleVR;
        [SerializeField] GameObject ScalerTitleDesktop;

        [SerializeField] ScalerLockStateOption[] ScalerLockStateOptions;

        Scaler linkedScaler;
        ScalerLockStateOption currentLockStateController;

        public void Setup(Scaler linkedScaler)
        {
            this.linkedScaler = linkedScaler;

            //Set display text
            if (Networking.LocalPlayer.IsUserInVR())
            {
                ScalerTitleVR.SetActive(true);
                ScalerTitleDesktop.SetActive(false);
            }
            else
            {
                ScalerTitleVR.SetActive(false);
                ScalerTitleDesktop.SetActive(true);
            }

            //Set default value
            if (currentLockStateController) currentLockStateController.SetToggleState(false);
            
            foreach(ScalerLockStateOption option in ScalerLockStateOptions)
            {
                option.Setup(this);
            }

            currentLockStateController = ScalerLockStateOptions[1];
            currentLockStateController.SetToggleState(true);
            linkedScaler.currentLockState = currentLockStateController.LockState;
        }

        public void SetLockState(ScalerLockStateOption calledLockStateController)
        {
            if (currentLockStateController) currentLockStateController.SetToggleState(false);

            linkedScaler.currentLockState = calledLockStateController.LockState;

            currentLockStateController = calledLockStateController;
        }

        //VRChat UI calls
        public void ResetViewScale()
        {
            linkedScaler.ResetScale();
        }
    }
}