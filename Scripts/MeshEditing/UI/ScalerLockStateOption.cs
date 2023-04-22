using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    public class ScalerLockStateOption : UdonSharpBehaviour
    {
        [SerializeField] ScalerLockStates lockState;
        [SerializeField] Toggle LinkedToggle;

        SyncedDisplaySettings linkedSyncedDisplaySettings;

        public void Setup(SyncedDisplaySettings linkedSyncedDisplaySettings)
        {
            this.linkedSyncedDisplaySettings = linkedSyncedDisplaySettings;
        }

        public ScalerLockStates LockState
        {
            get
            {
                return lockState;
            }
        }

        public void SetToggleState(bool value)
        {
            LinkedToggle.SetIsOnWithoutNotify(value);
        }

        //VRChat UI Call
        public void UpdateToggleState()
        {
            if (!LinkedToggle.isOn)
            {
                //Skip if deselected
                LinkedToggle.SetIsOnWithoutNotify(true);
                return;
            }

            linkedSyncedDisplaySettings.SetLockState(this);
        }
    }
}