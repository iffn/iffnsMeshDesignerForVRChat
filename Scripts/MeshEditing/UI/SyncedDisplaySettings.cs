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
        [SerializeField] Toggle SymmetryModeToggle;

        [SerializeField] ScalerLockStateOption[] ScalerLockStateOptions;

        //Synced
        bool symmetryMode = false;

        //Runtime variables
        Scaler linkedScaler;
        ScalerLockStateOption currentLockStateController;
        GameObject symmetryMeshHolder;
        ToolController linkedToolController;

        public void Setup(Scaler linkedScaler, GameObject mirrorMeshHolder, ToolController linkedToolController)
        {
            this.linkedScaler = linkedScaler;
            this.symmetryMeshHolder = mirrorMeshHolder;
            this.linkedToolController = linkedToolController;

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

            SetSymmetryParameters();
        }

        public void SetLockState(ScalerLockStateOption calledLockStateController)
        {
            if (currentLockStateController) currentLockStateController.SetToggleState(false);

            linkedScaler.currentLockState = calledLockStateController.LockState;

            currentLockStateController = calledLockStateController;
        }

        void SetSymmetryParameters()
        {
            linkedToolController.MirrorMode = symmetryMode;
            SymmetryModeToggle.SetIsOnWithoutNotify(symmetryMode);
            symmetryMeshHolder.SetActive(symmetryMode);
        }

        //VRChat UI calls
        public void ResetViewScale()
        {
            linkedScaler.ResetScale();
        }

        public void UpdateFromsSymmetryMeshToggle()
        {
            symmetryMode = SymmetryModeToggle.isOn;

            SetSymmetryParameters();
        }
    }
}