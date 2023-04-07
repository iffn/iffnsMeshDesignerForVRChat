using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class DebugController : UdonSharpBehaviour
    {
        [SerializeField] TMPro.TextMeshProUGUI ToolDebug;
        [SerializeField] TMPro.TextMeshProUGUI SyncDebug;

        ToolController linkedToolController;
        MeshSyncController linkedSyncController;

        bool setupCalled = false;

        public void Setup(ToolController linkedToolController, MeshSyncController linkedSyncController)
        {
            this.linkedToolController = linkedToolController;
            this.linkedSyncController = linkedSyncController;

            setupCalled = true;
        }

        private void Update()
        {
            if (!setupCalled) return;

            ToolDebug.text = linkedToolController.MultiLineDebugState();
            SyncDebug.text = linkedSyncController.MultiLineDebugState();
        }
    }
}