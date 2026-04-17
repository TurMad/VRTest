using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Autohand.Demo {
    public class InputButtonEvent : MonoBehaviour
    {
        [SerializeField] private InputActionReference reference; 
        public UnityEvent Pressed;
        public UnityEvent Released;
        bool pressed = false;
        private void Update()
        {
            InputRef();
        }

        private void InputRef()
        {
            if (reference.action.IsPressed() && !pressed)
            {
                Pressed?.Invoke();
                pressed = true;
            }
            else if (!reference.action.IsPressed() && pressed)
            {
                Released?.Invoke();
                pressed = false;
            }
        }
    }
}