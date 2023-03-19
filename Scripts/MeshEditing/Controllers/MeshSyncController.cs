//#define enableLimitControls

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

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
        SyncSettings linkedInterface;

        bool queueSync = false;

        float lastSync = 0;
        float nextSync = Mathf.Infinity;

        SerializationResult lastPostSerializationResult;

        int syncLimitPerSecondVRChat = 11000; //Source (Not sure if bit or byte, but it seems to work well when using a syncLimitThreshold of 0.4: https://docs.vrchat.com/docs/network-details

        float syncLimitThreshold = 0.4f;

        public void Setup(MeshController linkedMeshController, SyncSettings linkedInterface)
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

        

        public string DebugState()
        {
            string returnString = "";

            returnString += $"Debug output of {nameof(MeshSyncController)} at {Time.time}:\n";
            returnString += $"Is owner: {Networking.IsOwner(gameObject)}\n";
            returnString += $"{nameof(lastPostSerializationResult)}:\n";
            returnString += $"{nameof(lastPostSerializationResult.success)}: {lastPostSerializationResult.success}\n";
            returnString += $"{nameof(lastPostSerializationResult.byteCount)}: {lastPostSerializationResult.byteCount}\n";
            returnString += $"{nameof(syncLimitThreshold)}: {syncLimitThreshold}\n";
            returnString += $"{nameof(MinTimeBetweenSync)}: {MinTimeBetweenSync}\n";

            return returnString;
        }

        float MinTimeBetweenSync
        {
            get
            {
                return lastPostSerializationResult.byteCount / (syncLimitPerSecondVRChat * syncLimitThreshold);
            }
        }

        private void Update()
        {
            #if enableLimitControls
            if(Input.GetKeyDown(KeyCode.KeypadPlus)) syncLimitThreshold *= 1.25f; 
            if(Input.GetKeyDown(KeyCode.KeypadMinus)) syncLimitThreshold *= 0.8f; 
            #endif

            if (!queueSync) return;

            if(Time.time > nextSync)
            {
                queueSync = false;
                RequestSerialization();
            }
        }

        public void Sync()
        {
            if (!IsOwner) return;

            if(lastSync + MinTimeBetweenSync < Time.time)
            {
                RequestSerialization();
            }
            else
            {
                queueSync = true;
                nextSync = lastSync + MinTimeBetweenSync;
            }
        }

        public override void OnPreSerialization()
        {
            vertices = linkedMeshController.Vertices;
            triangles = linkedMeshController.Triangles;

            lastSync = Time.time;
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            lastPostSerializationResult = result;
        }

        public void RequestOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        //VRChat events
        public override void OnDeserialization()
        {
            linkedMeshController.SetData(vertices, triangles, this);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            //Current bug with VRChat: Will not fire when player leaves https://vrchat.canny.io/udon-networking-update/p/1258-onownershiptransferred-does-not-fire-at-onplayerleft-if-last-owner-is-passi
            
            linkedInterface.Owner = player;
        }
    }
}