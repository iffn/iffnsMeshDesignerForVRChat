using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MeshSyncController : UdonSharpBehaviour
    {
        //Synced variables
        [UdonSynced] Vector3[] vertices = new Vector3[0];
        [UdonSynced] int[] triangles = new int[0];

        //Runtime variables
        MeshController linkedMeshController;
        MeshBuilderInterface linkedInterface;

        public void Setup(MeshController linkedMeshController, MeshBuilderInterface linkedInterface)
        {
            this.linkedMeshController = linkedMeshController;
            this.linkedInterface = linkedInterface;
        }

        public bool IsOwner
        {
            get
            {
                return Networking.IsOwner(gameObject);
            }
        }

        public void Sync()
        {
            if (!IsOwner) return;

            vertices = linkedMeshController.Vertices;
            triangles = linkedMeshController.Triangles;

            RequestSerialization();
        }

        public void RequestOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        //VRChat events
        public override void OnDeserialization()
        {
            linkedMeshController.SetData(vertices, triangles);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            //Current bug with VRChat: Will not fire when player leaves https://vrchat.canny.io/udon-networking-update/p/1258-onownershiptransferred-does-not-fire-at-onplayerleft-if-last-owner-is-passi
            
            linkedInterface.Ownership = player.isLocal;
        }
    }
}