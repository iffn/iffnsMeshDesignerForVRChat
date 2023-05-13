using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    public class VRToolController : UdonSharpBehaviour
    {
        
        [SerializeField] GameObject buttonHolder;
        [SerializeField] Text currentToolText;
        [SerializeField] Text currentStateIndicator;
        [SerializeField] RectTransform canvasTransformVR;
        [SerializeField] RectTransform editButtonHolder;
        [SerializeField] Collider linkedCollider;

        ToolController linkedToolController;

        bool overUIElement = false;
        public bool OverUIElement
        {
            get
            {
                return overUIElement && linkedCollider.enabled;
            }
            private set
            {
                overUIElement = value;
            }
        }

        Quaternion leftHandUIHandRotation = Quaternion.Euler(0, 90, 90);
        Quaternion rightHandUIHandRotation = Quaternion.Euler(0, 90, 90);

        HandType primaryHand;

        public GameObject ButtonHolder
        {
            get
            {
                return buttonHolder;
            }
        }

        public string CurrentToolTextVR
        {
            set
            {
                currentToolText.text = value;
            }
        }

        public void Setup(int numberOfEditTools, ToolController linkedToolController)
        {
            this.linkedToolController = linkedToolController;

            float buttonHeight = 1f / 3f;
            float buttonHolderHeight = Mathf.Ceil(numberOfEditTools / 3f) * buttonHeight;
            float baseHeight = canvasTransformVR.sizeDelta.y;
            float totalHeight = buttonHolderHeight + baseHeight;

            RectTransform buttonHolderTransform = buttonHolder.GetComponent<RectTransform>();

            buttonHolderTransform.sizeDelta = new Vector2(buttonHolderTransform.sizeDelta.x, buttonHolderHeight);
            canvasTransformVR.sizeDelta = new Vector2(canvasTransformVR.sizeDelta.x, totalHeight);
            editButtonHolder.sizeDelta = canvasTransformVR.sizeDelta;

            ColliderEnabled = true;
        }

        void Start()
        {
            //Use Setup instead
        }

        public void UpdatePosition(VRCPlayerApi localPlayer, HandType primaryHand, Vector3 handPosition, float armLengthInVR)
        {
            this.primaryHand = primaryHand;

            Quaternion playerRotation = localPlayer.GetRotation();

            if (primaryHand == HandType.RIGHT) //The other one
            {
                transform.SetPositionAndRotation(
                    handPosition + playerRotation * (armLengthInVR * 0.08f * Vector3.up),
                    localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation * leftHandUIHandRotation);
            }
            else
            {
                transform.SetPositionAndRotation(
                    handPosition + playerRotation * (armLengthInVR * 0.08f * Vector3.up),
                    localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation * rightHandUIHandRotation);
            }

            transform.localScale = armLengthInVR * 0.5f * Vector3.one;

            //Interaction position
        }

        public bool ColliderEnabled
        {
            set
            {
                linkedCollider.enabled = value;

                if (value)
                {
                    currentStateIndicator.text = "<color=orange>Interactions enabled:\nPress trigger to avoid edit input fails</color>";
                    linkedToolController.UIFocusOnSecondaryHand = true;
                }
                else
                {
                    currentStateIndicator.text = "<color=red>Interaction disabled.\nPress trigger to reenable</color>";
                }
            }
        }

        //VRChat functions
        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (!value) return;

            if (args.handType == primaryHand) return;

            ColliderEnabled = !linkedCollider.enabled;
        }

        //VRChat UI function calls>

        public void CursorNowOverToolUI()
        {
            OverUIElement = true;
        }

        public void CursorNoLongerOverToolUI()
        {
            OverUIElement = false;
        }
    }
}