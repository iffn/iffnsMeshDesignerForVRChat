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
        [SerializeField] InteractorController LinkedInteractionController;
        [SerializeField] GameObject Highlight;

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


            LinkedInteractionController.CurrentInteractionType = InteractionType;
        }

        public bool IsValid()
        {
            if (Highlight == null) return false;
            if (LinkedInteractionController == null) return false;

            return true;
        }

        public bool Highlighted
        {
            set
            {
                if (!IsValid()) return;

                Highlight.SetActive(value);
            }
        }

        private void Start()
        {
            if (!IsValid())
            {
                Debug.LogWarning($"Error: {nameof(InteractionTypeSelectorButton)} called {gameObject.name} is not correctly set up");
            }
        }
    }
}