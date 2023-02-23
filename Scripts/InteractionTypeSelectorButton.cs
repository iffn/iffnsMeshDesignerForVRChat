using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class InteractionTypeSelectorButton : UdonSharpBehaviour
    {
        [Header("Unity assingments")]
        [SerializeField] InteractionTypes interactionType;
        [SerializeField] GameObject Highlight;

        InteractorController linkedInteractionController;

        public void Setup(InteractorController linkedInteractionController)
        {
            this.linkedInteractionController = linkedInteractionController;

            if (!IsValid())
            {
                Debug.LogWarning($"Error: {nameof(InteractionTypeSelectorButton)} called {gameObject.name} is not correctly set up");
            }
        }

        public InteractionTypes InteractionType
        {
            get
            {
                return interactionType;
            }
        }

        public void Use()
        {
            Debug.Log($"Setting mode {interactionType} from button {transform.parent.name}");

            if (!IsValid()) return;


            linkedInteractionController.CurrentInteractionType = InteractionType;
        }

        public bool IsValid()
        {
            if (Highlight == null) return false;

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