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
        [UdonSynced] bool symmetryMode;

        //Runtime variables
        MeshController linkedMeshController;
        SyncSettings linkedSyncSettings;
        SyncedDisplaySettings linkedSyncedDisplaySettings;
        Scaler linkedScaler;
        ToolSettings LinkedToolSettings;

        bool queueSync = false;

        float lastSync = 0;
        float nextSync = Mathf.Infinity;

        SerializationResult lastPostSerializationResult;

        int syncLimitPerSecondVRChat = 11000; //Source (Not sure if bit or byte, but it seems to work well when using a syncLimitThreshold of 0.4: https://docs.vrchat.com/docs/network-details

        float syncLimitThreshold = 0.4f;
        public void Setup(MeshController linkedMeshController, SyncSettings linkedInterface, Scaler linkedScaler, SyncedDisplaySettings linkedSyncedDisplaySettings, ToolSettings linkedToolSettings)
        {
            this.linkedMeshController = linkedMeshController;
            this.linkedSyncSettings = linkedInterface;
            this.linkedScaler = linkedScaler;
            this.linkedSyncedDisplaySettings = linkedSyncedDisplaySettings;
            this.LinkedToolSettings = linkedToolSettings;
        }

        public bool SymmetryMode
        {
            set
            {
                if (Networking.IsOwner(gameObject))
                {
                    symmetryMode = value;
                    RequestSerialization();
                }
                else
                {
                    linkedSyncedDisplaySettings.SymmetryMode = false;
                }
            }
            get
            {
                return symmetryMode;
            }
        }

        public bool IsOwner
        {
            get
            {
                return Networking.IsOwner(gameObject);
            }
        }

        float lastDeserializationTime = 0;
        float lastSerializationTime = 0;

        public string MultiLineDebugState()
        {
            string returnString = "";

            returnString += $"Debug output of {nameof(MeshSyncController)} at {Time.time:0.000}:\n"
                + $"{nameof(lastDeserializationTime)}: {lastDeserializationTime}\n"
                + $"{nameof(lastSerializationTime)}: {lastSerializationTime}\n"
                + $"Is owner: {Networking.IsOwner(gameObject)}\n"
                + $"{nameof(symmetryMode)}: {symmetryMode}\n"
                + $"{nameof(vertices)} array length: {vertices.Length}\n"
                + $"{nameof(triangles)} array length: {triangles.Length}\n"
                + $"{nameof(lastPostSerializationResult)}:\n"
                + $"{nameof(lastPostSerializationResult.success)}: {lastPostSerializationResult.success}\n"
                + $"{nameof(lastPostSerializationResult.byteCount)}: {lastPostSerializationResult.byteCount}\n"
                + $"{nameof(syncLimitThreshold)}: {syncLimitThreshold}\n"
                + $"{nameof(MinTimeBetweenSync)}: {MinTimeBetweenSync}\n";

            return returnString;
        }

        float MinTimeBetweenSync
        {
            get
            {
                return lastPostSerializationResult.byteCount / (syncLimitPerSecondVRChat * syncLimitThreshold);
            }
        }

        int syncCounter = 0;

        private void Update()
        {
            #if enableLimitControls
            if(Input.GetKeyDown(KeyCode.KeypadPlus)) syncLimitThreshold *= 1.25f; 
            if(Input.GetKeyDown(KeyCode.KeypadMinus)) syncLimitThreshold *= 0.8f; 
            #endif

            syncCounter++;
            if (syncCounter > 50)
            {
                syncCounter = 0;
                EnsureOwnership();
            }

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

            lastSerializationTime = Time.time;
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            lastPostSerializationResult = result;
        }

        public void RequestOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        void EnsureOwnership()
        {
            if (!Networking.IsOwner(gameObject)) return;

            if(!Networking.IsOwner(linkedScaler.gameObject)) Networking.SetOwner(Networking.LocalPlayer, linkedScaler.gameObject);
        }

        //VRChat events
        public override void OnDeserialization()
        {
            lastDeserializationTime = Time.time;
            linkedMeshController.SetData(vertices, triangles, this);
            linkedSyncedDisplaySettings.SymmetryMode = symmetryMode;
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            //Current bug with VRChat: Will not fire when owner leaves https://vrchat.canny.io/udon-networking-update/p/1258-onownershiptransferred-does-not-fire-at-onplayerleft-if-last-owner-is-passi

            syncCounter = 0;
            EnsureOwnership();

            linkedSyncSettings.Owner = player;

            if (!player.isLocal)
            {
                LinkedToolSettings.InEditMode = false;
            }
        }
    }
}