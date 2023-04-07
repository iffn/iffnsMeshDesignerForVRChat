//#define enableLimitControls

using System;
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
        [UdonSynced] short[] triangles = new short[0];
        [UdonSynced] bool symmetryMode;

        //Runtime variables
        MeshController linkedMeshController;
        SyncSettings linkedSyncSettings;
        SyncedDisplaySettings linkedSyncedDisplaySettings;
        Scaler linkedScaler;
        ToolSettings LinkedToolSettings;
        
        bool queueSync = false;
        VRCPlayerApi localPlayer;

        float lastSync = 0;
        float nextSync = Mathf.Infinity;
        int ensureOwnershipCounter = 0;

        float lastDeserializationTime = 0;
        float lastSerializationTime = 0;

        SerializationResult lastPostSerializationResult;

        readonly int syncLimitbBytePerSecondVRChat = 11000; //Source ('11kb per second' seems to be kilo bytes acccording to ingame limit tests with debug UI. Factor of 0.4 makes frequent sync more stable): https://docs.vrchat.com/docs/network-details

        float syncLimitThreshold = 0.4f;
        public void Setup(MeshController linkedMeshController, SyncSettings linkedInterface, Scaler linkedScaler, SyncedDisplaySettings linkedSyncedDisplaySettings, ToolSettings linkedToolSettings)
        {
            this.linkedMeshController = linkedMeshController;
            this.linkedSyncSettings = linkedInterface;
            this.linkedScaler = linkedScaler;
            this.linkedSyncedDisplaySettings = linkedSyncedDisplaySettings;
            this.LinkedToolSettings = linkedToolSettings;

            localPlayer = Networking.LocalPlayer;
            isOwner = Networking.IsOwner(gameObject);
        }

        public VRCPlayerApi Owner
        {
            get
            {
                return Networking.GetOwner(gameObject);
            }
        }

        public bool SymmetryMode
        {
            set
            {
                if (isOwner)
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

        bool isOwner = false;
        public bool IsOwner
        {
            get
            {
                return Networking.IsOwner(gameObject);
            }
        }

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
                return lastPostSerializationResult.byteCount / (syncLimitbBytePerSecondVRChat * syncLimitThreshold);
            }
        }

        private void Update()
        {
            #if enableLimitControls
            if(Input.GetKeyDown(KeyCode.KeypadMultiply)) syncLimitThreshold *= 1.25f; 
            if(Input.GetKeyDown(KeyCode.KeypadDivide)) syncLimitThreshold *= 0.8f; 
            #endif

            if (isOwner)
            {
                ensureOwnershipCounter++;
                if (ensureOwnershipCounter > 50)
                {
                    ensureOwnershipCounter = 0;
                    EnsureOwnership();
                }

                if (!queueSync) return;

                if (Time.time > nextSync)
                {
                    queueSync = false;
                    RequestSerialization();
                }
            }
        }

        public void Sync()
        {
            if (!IsOwner || queueSync) return;

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

            int[] intTriangles = linkedMeshController.Triangles;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            triangles = new short[intTriangles.Length];

            for(int i = 0; i < intTriangles.Length; i++)
            {
                triangles[i] = (short)intTriangles[i];
            }

            //Debug.Log($"It took {sw.Elapsed.TotalSeconds}s to convert the array with length {triangles.Length} to short");

            lastSync = Time.time;

            lastSerializationTime = Time.time;
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            lastPostSerializationResult = result;
        }

        public void RequestOwnership()
        {
            Networking.SetOwner(localPlayer, gameObject);
        }

        void EnsureOwnership()
        {
            if(!Networking.IsOwner(linkedScaler.gameObject)) Networking.SetOwner(localPlayer, linkedScaler.gameObject);
        }

        //VRChat events
        public override void OnDeserialization()
        {
            lastDeserializationTime = Time.time;

            int[] intTriangles = new int[triangles.Length];

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < intTriangles.Length; i++)
            {
                intTriangles[i] = triangles[i];
            }

            //Debug.Log($"It took {sw.Elapsed.TotalSeconds}s to convert the array with length {triangles.Length} to short");

            linkedMeshController.SetData(vertices, intTriangles, this);
            
            linkedSyncedDisplaySettings.SymmetryMode = symmetryMode;
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (localPlayer.IsOwner(gameObject) && !isOwner)
            {
                //Current bug with VRChat: Will not fire when owner leaves https://vrchat.canny.io/udon-networking-update/p/1258-onownershiptransferred-does-not-fire-at-onplayerleft-if-last-owner-is-passi
                Debug.Log("If you see this message, VRChat has not fixed OnOwnershipTransferred on owner leave yet ");
                OnOwnershipTransferred(localPlayer);
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            isOwner = player.isLocal;
            
            if (isOwner)
            {
                ensureOwnershipCounter = 0;
                EnsureOwnership();
            }
            else
            {
                LinkedToolSettings.InEditMode = false;
                queueSync = false;
            }

            linkedSyncSettings.Owner = player;
        }
    }
}