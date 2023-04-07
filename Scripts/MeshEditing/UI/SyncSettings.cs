using Newtonsoft.Json.Linq;
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
        [SerializeField] TMPro.TextMeshProUGUI CurrentOwnerText;
        [SerializeField] GameObject RequestOwnershipButton;
        
        MeshSyncController linkedSyncController;

        public void Setup(MeshSyncController linkedSyncController)
        {
            this.linkedSyncController = linkedSyncController;

            Owner = linkedSyncController.Owner;
        }

        public VRCPlayerApi Owner
        {
            set
            {
                RequestOwnershipButton.SetActive(!value.isLocal);

                CurrentOwnerText.text = value.displayName;
            }
        }

        //VRChat UI Events
        public void RequestOwnership()
        {
            linkedSyncController.RequestOwnership();
        }
    }
}