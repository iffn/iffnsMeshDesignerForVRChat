using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class SyncSettings : UdonSharpBehaviour
    {
        [Header("Unity assingments")]
        [SerializeField] MeshSyncController LinkedSyncController;
        [SerializeField] TMPro.TextMeshProUGUI CurrentOwnerText;
        [SerializeField] GameObject RequestOwnershipButton;

        public VRCPlayerApi Owner
        {
            set
            {
                RequestOwnershipButton.SetActive(!value.isLocal);

                CurrentOwnerText.text = value.displayName;
            }
        }

        public void RequestOwnership()
        {
            LinkedSyncController.RequestOwnership();
        }
    }
}