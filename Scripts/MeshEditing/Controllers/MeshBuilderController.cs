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
            Debug.Log("Start called");

            //Controllers
            LinkedToolController.Setup(LinkedToolSettings, LinkedMeshEditor, LinkedMeshInteractionInterface, MeshTransform);
            LinkedMeshEditor.Setup(LinkedMeshController, LinkedToolController, MeshTransform);
            LinkedMeshInteractionInterface.Setup(LinkedMeshEditor);
            LinkedMeshController.Setup(LinkedMeshEditor, LinkedMeshSyncController, MainMeshFilter.mesh);
            LinkedMeshSyncController.Setup(LinkedMeshController, LinkedSyncSettings);
            LinkedDebugController.Setup(LinkedToolController, LinkedMeshSyncController);

            //Settings
            LinkedToolSettings.Setup(LinkedToolController);
            LinkedSyncSettings.Setup(LinkedMeshSyncController);
            LinkedSyncedDisplaySettings.Setup(LinkedScaler, MainSymmetryMeshFilter.gameObject, LinkedToolController);
            LinkedMeshConverterController.Setup(LinkedMeshController, LinkedMeshEditor, LinkedMeshSyncController, ReferenceMeshFilter.mesh, ReferenceMeshFilter.gameObject, ReferenceSymmetryMeshFilter.gameObject);

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