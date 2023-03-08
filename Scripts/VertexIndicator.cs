
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class VertexIndicator : UdonSharpBehaviour
    {
        MeshRenderer attachedRenderer;

        [SerializeField] Material defaultColor;
        [SerializeField] Material interactColor;
        [SerializeField] Material removeColor;

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

        public void SetInfo(int index, Vector3 localPosition)
        {
            this.index = index;
            transform.localPosition = localPosition;
        }

        public void Setup(int index, Transform parent, float scale)
        {
            this.Index = index;
            transform.parent = parent;
            gameObject.SetActive(true);
            transform.localScale = scale * Vector3.one;

            attachedRenderer = transform.GetComponent<MeshRenderer>();
        }

        public VertexSelectStates SelectState
        {
            set
            {
                switch (value)
                {
                    case VertexSelectStates.Normal:
                        attachedRenderer.material = defaultColor;
                        break;
                    case VertexSelectStates.ReadyToDelete:
                        attachedRenderer.material = removeColor;
                        break;
                    case VertexSelectStates.Selected:
                        attachedRenderer.material = interactColor;
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
