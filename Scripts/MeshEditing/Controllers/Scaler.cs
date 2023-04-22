
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class Scaler : UdonSharpBehaviour
    {
        [SerializeField] Transform helperTransform;

        [UdonSynced(UdonSyncMode.Smooth)] Vector3 position;
        [UdonSynced(UdonSyncMode.Smooth)] Quaternion rotation;
        [UdonSynced(UdonSyncMode.Smooth)] Vector3 localScale;

        bool isScalingInVR = false;
        bool isInVR;

        Vector3 originalLocalPosition;
        Vector3 originalLocalScale;

        public ScalerLockStates currentLockState = ScalerLockStates.LockRotationOnly;

        VRCPlayerApi localPlayer;

        public string MultiLineDebugState()
        {
            VRCPlayerApi owner = Networking.GetOwner(gameObject);

            string returnString = $"Debug output of {nameof(Scaler)} at {Time.time:0.000}:\n"
                + $"Owner: {owner.playerId} - {owner.displayName}\n"
                + $"Left and right input states:  {Input.GetAxis("Oculus_CrossPlatform_PrimaryHandTrigger")} / {Input.GetAxis("Oculus_CrossPlatform_SecondaryHandTrigger")}\n"
                + $"{nameof(isScalingInVR)}: {isScalingInVR}\n";

            return returnString;
        }

        public void ResetScalerToEyeHeight()
        {
            ResetScale(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position.y);
        }

        public void ResetScalerToFloor()
        {
            ResetScale(localPlayer.GetPosition().y);
        }

        void ResetScale(float originHeight)
        {
            if (isScalingInVR)
            {
                StopScaling();
            }

            transform.localScale = Vector3.one;

            transform.localPosition = originalLocalPosition;

            Vector3 pos = transform.position;

            pos.y = originHeight;

            transform.position = pos;

            transform.localRotation = Quaternion.identity;
            transform.localScale = originalLocalScale;
        }

        private void Start()
        {
            originalLocalPosition = transform.localPosition;
            originalLocalScale = transform.localScale;

            localPlayer = Networking.LocalPlayer;
            isInVR = localPlayer.IsUserInVR();

            helperTransform.gameObject.SetActive(false);

        }

        private void Update()
        {
            if (!Networking.IsOwner(gameObject))
            {
                transform.SetPositionAndRotation(position, rotation);
                transform.localScale = localScale;
                return;
            }
            

            localScale = transform.localScale;

            if (isInVR)
            {
                if (Input.GetAxis("Oculus_CrossPlatform_PrimaryHandTrigger") > 0.9f && Input.GetAxis("Oculus_CrossPlatform_SecondaryHandTrigger") > 0.9f)
                {
                    if (!isScalingInVR)
                    {
                        SetupScaling();
                        isScalingInVR = true;
                    }

                    ScaleInVR();
                    UpdatePositionsForSync();
                }
                else
                {
                    if (isScalingInVR)
                    {
                        StopScaling();
                        isScalingInVR = false;
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
                if (Input.GetKey(KeyCode.KeypadMinus)) transform.localScale *= 1f / scalingSpeed;

                if(Input.GetKeyDown(KeyCode.KeypadEnter)) ResetScalerToEyeHeight();

                UpdatePositionsForSync();
            }
        }

        void UpdatePositionsForSync()
        {
            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;
        }

        void SetupScaling()
        {
            Vector3 rightHand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            Vector3 leftHand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

            Vector3 rightToLeft = leftHand - rightHand;

            helperTransform.gameObject.SetActive(true);

            helperTransform.localScale = rightToLeft.magnitude * Vector3.one;

            helperTransform.position = 0.5f * (rightHand + leftHand);

            helperTransform.parent = transform.parent;

            transform.parent = helperTransform;

        }

        void ScaleInVR()
        {
            Vector3 rightHand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            Vector3 leftHand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

            Vector3 rightToLeft = leftHand - rightHand;

            float currentDistance = rightToLeft.magnitude;

            helperTransform.position = 0.5f * (rightHand + leftHand);

            helperTransform.localScale = currentDistance * Vector3.one;
        }

        void StopScaling()
        {
            transform.parent = helperTransform.parent;

            helperTransform.gameObject.SetActive(false);
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


