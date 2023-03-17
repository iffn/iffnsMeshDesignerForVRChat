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
        [SerializeField] Toggle LockMovementAndRotationtOption;
        [SerializeField] Toggle LockRotationOnlyOption;
        [SerializeField] Toggle AllowHeadingRotationOption;
        [SerializeField] Toggle AllowFullRotationOption;
        [SerializeField] Toggle ShowSymmetryMeshToggle;

        

        void UpdateFromUI()
        {

        }
    }
}