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
        [SerializeField] Scaler LinkedScaler;
        [SerializeField] GameObject ScalerTitleVR;
        [SerializeField] GameObject ScalerTitleDesktop;

        [SerializeField] ScalerLockStateOption[] ScalerLockStateOptions;

        ScalerLockStateOption currentLockStateController;

        void Setup()
        {
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
            LinkedScaler.currentLockState = currentLockStateController.LockState;
        }

        //VRChat UI calls
        public void SetLockState(ScalerLockStateOption calledLockStateController)
        {
            if (currentLockStateController) currentLockStateController.SetToggleState(false);

            LinkedScaler.currentLockState = calledLockStateController.LockState;

            currentLockStateController = calledLockStateController;
        }

        public void ResetViewScale()
        {
            LinkedScaler.ResetScale();
        }
    }
}