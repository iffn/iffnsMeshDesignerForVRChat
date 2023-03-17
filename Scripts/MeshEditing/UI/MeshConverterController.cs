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
        [SerializeField] Toggle MirrorReferenceMeshTroggle;

        [Header("Other Unity assingments")]
        [SerializeField] ObjConterter LinkedObjConverter;
        [SerializeField] BaseMeshConverter[] LinkedImporters;
        [SerializeField] MeshController LinkedMeshController;
        [SerializeField] MeshEditor LinkedMeshEditor;
        [SerializeField] GameObject ReferenceMeshHolder;
        [SerializeField] GameObject MirrorReferenceMeshHolder;
        [SerializeField] Mesh ReferenceMesh;

        bool skipUpdate = false;

        void Setup()
        {
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

        public void ImportObj(string objText)
        {
            bool worked = LinkedObjConverter.ImportMeshIfValidAndSaveData(objText);

            if (!worked) return;

            LinkedMeshController.SetData(LinkedObjConverter.VerticesFromLastImport, LinkedObjConverter.TrianglesFromLastImport, this);
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
            string exportText = CurrentConverter.ExportMesh(LinkedMeshController.Vertices, LinkedMeshController.Triangles);

            skipUpdate = true;
            LinkedInputField.text = exportText;
            skipUpdate = false;

            //LinkedInputField.SetTextWithoutNotify(exportText); //Not exposed in U#
        }

        public void ImportData()
        {
            BaseMeshConverter currentConverter = CurrentConverter;

            bool worked = currentConverter.ImportMeshIfValidAndSaveData(LinkedInputField.text);

            if (!worked) return;

            LinkedMeshController.SetData(currentConverter.VerticesFromLastImport, currentConverter.TrianglesFromLastImport, this);
        }

        public void MergeOverlappingVertices()
        {
            LinkedMeshEditor.MergeOverlappingVertices(0.001f);
        }

        public void ImportDataForReference()
        {
            BaseMeshConverter currentConverter = CurrentConverter;

            bool worked = currentConverter.ImportMeshIfValidAndSaveData(LinkedInputField.text);

            if (!worked) return;

            ReferenceMesh.triangles = new int[0];

            ReferenceMesh.vertices = currentConverter.VerticesFromLastImport;
            ReferenceMesh.triangles = currentConverter.TrianglesFromLastImport;

            ReferenceMesh.RecalculateNormals();
            ReferenceMesh.RecalculateTangents();
            ReferenceMesh.RecalculateBounds();
        }

        public void ToggleUpdate()
        {
            ReferenceMeshHolder.SetActive(ShowReferenceMeshTroggle.isOn);
            MirrorReferenceMeshHolder.SetActive(MirrorReferenceMeshTroggle.isOn);
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