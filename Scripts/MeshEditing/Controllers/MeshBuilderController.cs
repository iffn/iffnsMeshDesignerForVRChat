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
        [SerializeField] MeshFilter MainMeshFilter;
        [SerializeField] MeshFilter ReferenceMeshFilter;
        [SerializeField] GameObject ReferenceMeshHolder;
        [SerializeField] GameObject MirrorReferenceMeshHolder;

        [Header("Unity assingments: Controllers")]
        [SerializeField] ToolController LinkedToolController;
        [SerializeField] MeshEditor LinkedMeshEditor;
        [SerializeField] MeshInteractionInterface LinkedMeshInteractionInterface;
        [SerializeField] MeshController LinkedMeshController;
        [SerializeField] MeshSyncController LinkedMeshSyncController;
        [SerializeField] Scaler LinkedScaler;

        [Header("Unity assingments: Settings")]
        [SerializeField] ToolSettings LinkedToolSettings;
        [SerializeField] SyncSettings LinkedSyncSettings;
        [SerializeField] SyncedDisplaySettings LinkedSyncedDisplaySettings;
        [SerializeField] MeshConverterController LinkedMeshConverterController;

        private void Start()
        {
            //Controllers
            LinkedToolController.Setup(LinkedToolSettings, LinkedMeshEditor, LinkedMeshInteractionInterface, MeshTransform);
            LinkedMeshEditor.Setup(LinkedMeshController, LinkedToolController, MeshTransform);
            LinkedMeshInteractionInterface.Setup(LinkedMeshEditor);
            LinkedMeshController.Setup(LinkedMeshEditor, LinkedMeshSyncController, MainMeshFilter.mesh);
            LinkedMeshSyncController.Setup(LinkedMeshController, LinkedSyncSettings);

            //Settings
            LinkedToolSettings.Setup(LinkedToolController);
            LinkedSyncSettings.Setup(LinkedMeshSyncController);
            LinkedSyncedDisplaySettings.Setup(LinkedScaler);
            LinkedMeshConverterController.Setup(LinkedMeshController, LinkedMeshEditor, LinkedMeshSyncController, ReferenceMeshFilter.mesh, ReferenceMeshHolder, MirrorReferenceMeshHolder);
        }
    }
}