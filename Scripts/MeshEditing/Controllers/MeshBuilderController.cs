#define debugLog

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class MeshBuilderController : UdonSharpBehaviour
    {
        [Header("Unity assingments: General objects")]
        [SerializeField] Transform MeshTransform;

        [Header("Unity assingments: Controllers")]
        [SerializeField] ToolController LinkedToolController;
        [SerializeField] MeshEditor LinkedMeshEditor;
        [SerializeField] MeshController LinkedMeshController;
        [SerializeField] MeshSyncController LinkedMeshSyncController;
        [SerializeField] Scaler LinkedScaler;


        [Header("Unity assingments: Settings")]
        [SerializeField] SyncSettings LinkedSyncSettings;
        [SerializeField] SyncedDisplaySettings LinkedSyncedDisplaySettings;
        [SerializeField] MeshConverterController LinkedMeshConverterController;


        private void Start()
        {
            //Controllers
            LinkedToolController.Setup(LinkedMeshEditor, MeshTransform);
            LinkedMeshEditor.Setup(LinkedMeshController, LinkedToolController);
            LinkedMeshSyncController.Setup(LinkedMeshController, LinkedSyncSettings);

            //Settings
            LinkedSyncSettings.Setup(LinkedMeshSyncController);
            LinkedSyncedDisplaySettings.Setup(LinkedScaler);
            LinkedMeshConverterController.Setup(LinkedMeshController, LinkedMeshEditor);
        }
    }
}