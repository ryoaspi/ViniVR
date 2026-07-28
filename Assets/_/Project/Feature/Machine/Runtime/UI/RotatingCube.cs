using UnityEngine;
using UnityEngine.UIElements;

namespace Machine.Runtime
{
    public class RotatingCube : MonoBehaviour
    {
        
        #region Publics
        #endregion
        
        #region Unity API
        #endregion
        
        #region Main Methods
        void Start()
        {
            startButton = uiDocumentReference.rootVisualElement.Q<Button>("press-start-button");
            startButton.clicked += StartRotation;
        }
        
        void Update()
        {
            if (!isRotating)
            {
                return;
            }
            float rotationSpeed = degreesPerRotation / secondsPerRotation * Time.deltaTime;
            objectReference.Rotate(Vector3.right, rotationSpeed, Space.Self);
            rotatedDegrees += rotationSpeed;
            
            float totalDegrees = degreesPerRotation * numberOfRotations;
            
            if (rotatedDegrees >= totalDegrees)
            {
                isRotating = false;
                objectReference.localRotation = initialRotation;
            }
        }
        #endregion
        
        #region Utils
        private void StartRotation()
        {
            if (isRotating)
            {
                return;
            }
            initialRotation = objectReference.localRotation;
            rotatedDegrees = 0f;
            isRotating = true;
        }
        #endregion
        
        #region Private
        [SerializeField] private Transform objectReference;
        [SerializeField] private UIDocument uiDocumentReference;
        private Button startButton;
        private bool isRotating = false;
        [SerializeField] private float rotatedDegrees = 0f;
        private Quaternion initialRotation;
        [SerializeField] private float secondsPerRotation = 3f;
        [SerializeField] private int numberOfRotations = 3;
        [SerializeField] private float degreesPerRotation = 360f;
        #endregion
       
       
        
        
     
    }
}
