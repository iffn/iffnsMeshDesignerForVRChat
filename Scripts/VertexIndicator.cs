
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VertexIndicator : UdonSharpBehaviour
{
    MeshBuilder linkedMeshBuilder;
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

    public void Setup(int index, Transform parent, Vector3 localPosition, MeshBuilder linkedMeshBuilder)
    {
        this.Index = index;
        transform.parent = parent;
        transform.localPosition = localPosition;
        gameObject.SetActive(true);
        this.linkedMeshBuilder = linkedMeshBuilder;
        transform.localScale = linkedMeshBuilder.VertexInteractorScale * Vector3.one;

        attachedRenderer = transform.GetComponent<MeshRenderer>();
    }

    public VertexSelectStates SetSelectState
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
