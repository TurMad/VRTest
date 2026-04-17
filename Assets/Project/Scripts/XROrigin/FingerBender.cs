using UnityEngine;
using UnityEngine.InputSystem;
using Autohand;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BigDreamLab
{
    public class FingerBender : MonoBehaviour{
        [SerializeField] private XRBaseInteractor interactor; 

        [SerializeField] private InputActionReference indexValue;
        [SerializeField] private InputActionReference grabValue;
        [SerializeField] private InputActionReference thumbTouch; 
        [SerializeField] private InputActionReference thumbPress; 

        [SerializeField] private Finger finger_index;
        [SerializeField] private Finger finger_middle;
        [SerializeField] private Finger finger_ring;
        [SerializeField] private Finger finger_pinky;
        [SerializeField] private Finger finger_thumb;

        private float speed = 10f;

        void LateUpdate(){
            if (interactor.hasSelection)
            {
                BendFull();
            }
            else
            {
                BendIndex();
                BendGrab();
                BendThumb();
            }
        }

        private void BendIndex()
        {
            var value = indexValue.action.ReadValue<float>();
            finger_index.SetFingerBend(value);
        }

        private void BendGrab()
        {
            var value = grabValue.action.ReadValue<float>();
            finger_middle.SetFingerBend(value);
            finger_ring.SetFingerBend(value);
            finger_pinky.SetFingerBend(value);
        }

        private void BendThumb()
        {
            var target = 0f;
            if (thumbPress.action.IsPressed())
                target = 1f;
            else if (thumbTouch.action.IsPressed()) 
                target = 0.5f;
            var value = Mathf.MoveTowards(finger_thumb.GetCurrentBend(), target, speed * Time.deltaTime);

            finger_thumb.SetFingerBend(value);
        }

        private void BendFull()
        {
            finger_index.SetFingerBend(1f);
            finger_middle.SetFingerBend(1f);
            finger_ring.SetFingerBend(1f);
            finger_pinky.SetFingerBend(1f);
            finger_thumb.SetFingerBend(1f);
        }
    }
}
