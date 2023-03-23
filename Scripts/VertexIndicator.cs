
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class VertexIndicator : UdonSharpBehaviour
    {
        MeshRenderer attachedRenderer;

        [SerializeField] Material defaultMaterialDesktop;
        [SerializeField] Material interactMaterialDesktop;
        [SerializeField] Material removeMaterialDesktop;

        [SerializeField] Material defaultMaterialVR;
        [SerializeField] Material interactMaterialVR;
        [SerializeField] Material removeMaterialVR;

        Material defaultMaterial;
        Material interactMaterial;
        Material removeMaterial;

        int index;
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                InteractionText = value.ToString();
            }
        }

        public float Radius
        {
            set
            {
                transform.localScale = value * Vector3.one;
            }
        }

        public void SetInfo(int index, Vector3 localPosition)
        {
            this.index = index;
            transform.localPosition = localPosition;
        }

        public void Setup(int index, Transform parent, float scale)
        {
            this.index = index;
            transform.parent = parent;
            gameObject.SetActive(true);
            transform.localScale = scale * Vector3.one;

            attachedRenderer = transform.GetComponent<MeshRenderer>();

            if (Networking.LocalPlayer.IsUserInVR())
            {
                defaultMaterial = defaultMaterialVR;
                interactMaterial = interactMaterialVR;
                removeMaterial = removeMaterialVR;
            }
            else
            {
                defaultMaterial = defaultMaterialDesktop;
                interactMaterial = interactMaterialDesktop;
                removeMaterial = removeMaterialDesktop;
            }

            SelectState = selectState;
        }

        VertexSelectStates selectState = VertexSelectStates.Normal;
        public VertexSelectStates SelectState
        {
            set
            {
                selectState = value;

                switch (value)
                {
                    case VertexSelectStates.Normal:
                        attachedRenderer.material = defaultMaterial;
                        break;
                    case VertexSelectStates.ReadyToDelete:
                        attachedRenderer.material = removeMaterial;
                        break;
                    case VertexSelectStates.Selected:
                        attachedRenderer.material = interactMaterial;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public enum VertexSelectStates
    {
        Normal,
        ReadyToDelete,
        Selected
    }

}
