using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class InteractionTypeSelectorButton : UdonSharpBehaviour
    {
        [Header("Unity assingments")]
        [SerializeField] Image LinkedImage;
        [SerializeField] GameObject Highlight;

        InteractorController linkedInteractionController;
        public MeshEditTool LinkedTool { get; private set; }

        public void Setup(InteractorController linkedInteractionController, MeshEditTool linkedTool)
        {
            this.linkedInteractionController = linkedInteractionController;
            this.LinkedTool = linkedTool;

            if (!IsValid())
            {
                Debug.LogWarning($"Error: {nameof(InteractionTypeSelectorButton)} called {gameObject.name} is not correctly set up");
            }

            LinkedImage.sprite = linkedTool.LinkedSprite;
        }

        public void Use()
        {
            if (!IsValid()) return;

            linkedInteractionController.CurrentInteractorTool = LinkedTool;
        }

        public bool IsValid()
        {
            if (Highlight == null) return false;
            if (LinkedImage == null) return false;

            return true;
        }

        public bool Highlighted
        {
            set
            {
                if (!IsValid())
                {
                    Debug.LogWarning($"Error: Button {transform.parent.name} is not set up correctly");
                    return;
                }

                Highlight.SetActive(value);
            }
        }
    }
}