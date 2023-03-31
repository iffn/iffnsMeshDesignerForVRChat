
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class Scaler : UdonSharpBehaviour
    {
        [SerializeField] Transform scaleObject;
        [SerializeField] GameObject indicator;

        float referenceDistance;

        bool isScaling = false;
        bool isInVR;

        Vector3 originalLocalPosition;
        Vector3 originalLocalScale;

        public ScalerLockStates currentLockState = ScalerLockStates.LockRotationOnly;

        public void ResetScale()
        {
            if (isScaling)
            {
                StopScaling();
            }

            transform.localScale = Vector3.one;

            scaleObject.transform.localPosition = originalLocalPosition;
            scaleObject.transform.localRotation = Quaternion.identity;
            scaleObject.transform.localScale = originalLocalScale;
        }

        private void Start()
        {
            originalLocalPosition = scaleObject.transform.localPosition;
            originalLocalScale = scaleObject.transform.localScale;

            isInVR = Networking.LocalPlayer.IsUserInVR();
        }

        private void Update()
        {
            if (!Networking.IsOwner(gameObject)) return;

            if (isInVR)
            {
                if (Input.GetAxis("Oculus_CrossPlatform_PrimaryHandTrigger") > 0.9f && Input.GetAxis("Oculus_CrossPlatform_SecondaryHandTrigger") > 0.9f)
                {
                    if (!isScaling)
                    {
                        SetupScaling();
                        isScaling = true;
                    }

                    Scale();
                }
                else
                {
                    if (isScaling)
                    {
                        StopScaling();
                        isScaling = false;
                    }
                }
            }
            else
            {
                float movementSpeed = 0.3f * Time.deltaTime;
                float rotationSpeed = 40f * Time.deltaTime;
                float scalingSpeed = 1f + 0.3f * Time.deltaTime;

                if (Input.GetKey(KeyCode.Keypad8)) transform.Translate(movementSpeed * Vector3.forward);
                if (Input.GetKey(KeyCode.Keypad2)) transform.Translate(movementSpeed * Vector3.back);
                if (Input.GetKey(KeyCode.Keypad4)) transform.Translate(movementSpeed * Vector3.left);
                if (Input.GetKey(KeyCode.Keypad6)) transform.Translate(movementSpeed * Vector3.right);
                if (Input.GetKey(KeyCode.PageUp)) transform.Translate(movementSpeed * Vector3.up);
                if (Input.GetKey(KeyCode.PageDown)) transform.Translate(movementSpeed * Vector3.down);
                
                if (Input.GetKey(KeyCode.Keypad9)) transform.Rotate(rotationSpeed * Vector3.up);
                if (Input.GetKey(KeyCode.Keypad7)) transform.Rotate(rotationSpeed * Vector3.down);
                
                if (Input.GetKey(KeyCode.KeypadPlus)) transform.localScale *= scalingSpeed;
                if (Input.GetKey(KeyCode.KeypadMinus)) transform.localScale *= 1f/scalingSpeed;

                if(Input.GetKeyDown(KeyCode.KeypadEnter)) ResetScale();
            }
        }

        void SetupScaling()
        {
            Vector3 rightHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            Vector3 leftHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

            Vector3 rightToLeft = leftHand - rightHand;

            referenceDistance = rightToLeft.magnitude;

            transform.position = rightHand + referenceDistance * 0.5f * rightToLeft.normalized;

            transform.parent = scaleObject.parent;
            scaleObject.parent = transform;

            indicator.SetActive(true);
        }

        void Scale()
        {
            Vector3 rightHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            Vector3 leftHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

            Vector3 rightToLeft = leftHand - rightHand;

            float currentDistance = rightToLeft.magnitude;

            transform.position = rightHand + currentDistance * 0.5f * rightToLeft.normalized;

            transform.localScale = currentDistance / referenceDistance * Vector3.one;
        }

        void StopScaling()
        {
            scaleObject.parent = transform.parent;

            transform.localScale = Vector3.one;

            transform.parent = scaleObject;
            indicator.SetActive(false);

            MeshCollider mymesh = transform.GetComponent<MeshCollider>();

            mymesh.sharedMesh = null;
        }
    }

    public enum ScalerLockStates
    {
        LockMovementAndRotation,
        LockRotationOnly,
        AllowHeadingRotation,
        AllowFullRotation
    }
}


