#define debugLog

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    public class MeshBuilderController : UdonSharpBehaviour
    {
        [Header("Unity assingments: General objects")]
        [SerializeField] Transform MeshTransform;
        [SerializeField] MeshFilter MainMeshFilter;
        [SerializeField] MeshFilter MainSymmetryMeshFilter;
        [SerializeField] MeshFilter ReferenceMeshFilter;
        [SerializeField] MeshFilter ReferenceSymmetryMeshFilter;

        [Header("Unity assingments: Controllers")]
        [SerializeField] ToolController LinkedToolController;
        [SerializeField] MeshEditor LinkedMeshEditor;
        [SerializeField] MeshInteractionInterface LinkedMeshInteractionInterface;
        [SerializeField] MeshController LinkedMeshController;
        [SerializeField] MeshSyncController LinkedMeshSyncController;
        [SerializeField] Scaler LinkedScaler;
        [SerializeField] DebugController LinkedDebugController;

        [Header("Unity assingments: Settings")]
        [SerializeField] ToolSettings LinkedToolSettings;
        [SerializeField] SyncSettings LinkedSyncSettings;
        [SerializeField] SyncedDisplaySettings LinkedSyncedDisplaySettings;
        [SerializeField] MeshConverterController LinkedMeshConverterController;

        private void Start()
        {
            //Controllers
            LinkedToolController.Setup(LinkedToolSettings, LinkedMeshEditor, LinkedMeshInteractionInterface, MeshTransform);
            LinkedMeshController.Setup(LinkedMeshEditor, LinkedMeshSyncController, MainMeshFilter.mesh);
            LinkedMeshEditor.Setup(LinkedMeshController, LinkedToolController, MeshTransform);
            LinkedMeshInteractionInterface.Setup(LinkedMeshEditor);
            LinkedMeshSyncController.Setup(LinkedMeshController, LinkedSyncSettings, LinkedScaler, LinkedSyncedDisplaySettings, LinkedToolSettings);
            LinkedDebugController.Setup(LinkedToolController, LinkedMeshSyncController);

            //Settings
            LinkedToolSettings.Setup(LinkedToolController, LinkedMeshSyncController);
            LinkedSyncSettings.Setup(LinkedMeshSyncController);
            LinkedSyncedDisplaySettings.Setup(LinkedScaler, MainSymmetryMeshFilter.gameObject, LinkedToolController, LinkedMeshSyncController);
            LinkedMeshConverterController.Setup(LinkedMeshController, LinkedMeshEditor, LinkedMeshSyncController, ReferenceMeshFilter.mesh, ReferenceMeshFilter.gameObject, ReferenceSymmetryMeshFilter.gameObject, LinkedScaler);

            SendCustomEventDelayedFrames(nameof(SetSymmetryMeshes), 1);
        }

        public void SetSymmetryMeshes()
        {
            //Somehow always on start
            MainSymmetryMeshFilter.sharedMesh = MainMeshFilter.sharedMesh;
            ReferenceSymmetryMeshFilter.sharedMesh = ReferenceMeshFilter.sharedMesh;
        }
    }
}