using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class MeshConverterController : UdonSharpBehaviour
    {
        [Header("Unity assingments for UI")]
        [SerializeField] InputField LinkedInputField;
        [SerializeField] Dropdown LinkedControllerSelectorDropdown;
        [SerializeField] Toggle ShowReferenceMeshTroggle;
        [SerializeField] Toggle ReferenceMeshSymmmetryTroggle;

        [Header("Other Unity assingments")]
        [SerializeField] ObjConterter LinkedObjConverter;
        [SerializeField] BaseMeshConverter[] LinkedImporters;

        MeshController linkedMeshController;
        MeshEditor linkedMeshEditor;
        MeshSyncController linkedSyncController;
        Mesh referenceMesh;
        GameObject referenceMeshHolder;
        GameObject symmetryReferenceMeshHolder;

        bool skipUpdate = false;

        public void Setup(MeshController linkedMeshController, MeshEditor linkedMeshEditor, MeshSyncController linkedSyncController, Mesh referenceMesh, GameObject referenceMeshHolder, GameObject symmetryReferenceHolder)
        {
            this.linkedMeshController = linkedMeshController;
            this.linkedMeshEditor = linkedMeshEditor;
            this.linkedSyncController = linkedSyncController;
            this.referenceMesh = referenceMesh;
            this.referenceMeshHolder = referenceMeshHolder;
            this.symmetryReferenceMeshHolder = symmetryReferenceHolder;

            //Unable to set input field options at this time
            /*
            LinkedControllerSelectorDropdown.ClearOptions();

            string[] options = new string[LinkedImporters.Length];

            for(int i = 0; i < options.Length; i++)
            {
                options[i] = LinkedImporters[i].Title;
            }

            LinkedControllerSelectorDropdown.AddOptions(options);
            */
        }

        public void ImportData(string objText, BaseMeshConverter converter) //For default mesh loader
        {
            if (!linkedSyncController.IsOwner) return;

            bool worked = converter.ImportMeshIfValidAndSaveData(objText);

            if (!worked) return;

            linkedMeshController.SetData(converter.VerticesFromLastImport, converter.TrianglesFromLastImport, this);
        }

        BaseMeshConverter CurrentConverter
        {
            get
            {
                return LinkedImporters[LinkedControllerSelectorDropdown.value];
            }
        }

        //VRChat funcitons
        public void ClearInputField()
        {
            LinkedInputField.text = "";
        }

        public void ExportData()
        {
            string exportText = CurrentConverter.ExportMesh(linkedMeshController.Vertices, linkedMeshController.Triangles);

            skipUpdate = true;
            LinkedInputField.text = exportText;
            skipUpdate = false;

            //LinkedInputField.SetTextWithoutNotify(exportText); //Not exposed in U#
        }

        public void ImportData()
        {
            ImportData(LinkedInputField.text, CurrentConverter);
        }

        public void MergeOverlappingVertices()
        {
            linkedMeshEditor.MergeOverlappingVertices(0.001f);
        }

        public void ImportDataForReference()
        {
            BaseMeshConverter currentConverter = CurrentConverter;

            bool worked = currentConverter.ImportMeshIfValidAndSaveData(LinkedInputField.text);

            if (!worked) return;

            referenceMesh.triangles = new int[0];

            referenceMesh.vertices = currentConverter.VerticesFromLastImport;
            referenceMesh.triangles = currentConverter.TrianglesFromLastImport;

            referenceMesh.RecalculateNormals();
            referenceMesh.RecalculateTangents();
            referenceMesh.RecalculateBounds();
        }

        public void ToggleUpdate()
        {
            referenceMeshHolder.SetActive(ShowReferenceMeshTroggle.isOn);
            symmetryReferenceMeshHolder.SetActive(ReferenceMeshSymmmetryTroggle.isOn);
        }

        public void DropdownUpdate()
        {
            foreach (BaseMeshConverter current in LinkedImporters)
            {
                current.gameObject.SetActive(false);
            }

            CurrentConverter.gameObject.SetActive(true);
        }

        public void InputFieldUpdated()
        {
            if(skipUpdate) return;

            CurrentConverter.InputFieldUpdated(LinkedInputField.text);
        }
    }
}