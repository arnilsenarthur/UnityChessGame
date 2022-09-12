using UnityEngine;

namespace Game
{
    public class CameraController : MonoBehaviour
    {
        public float rotSpeed = 5f;

        public float yaw = 0;
        public float pitch = 0;
        public float distance = 0;

        public GameObject target;

        public float zoomAcceleration = 0;
        public float yawAcceleration = 0;
        public float pitchAcceleration = 0;
        
        public void Start()
        {
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
            distance = Vector3.Distance(target.transform.position, transform.position);
        }

        public void Update()
        {
            if(Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            if(Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = CursorLockMode.None;
            }

            if(Input.GetMouseButton(1))
            {
                yawAcceleration = Mathf.Lerp(yawAcceleration, Input.GetAxis("Mouse X") * rotSpeed, Time.deltaTime * 5f);
                pitchAcceleration = Mathf.Lerp(pitchAcceleration, -Input.GetAxis("Mouse Y") * rotSpeed, Time.deltaTime * 5f);
            }
            else
            {
                yawAcceleration = Mathf.Lerp(yawAcceleration, 0, Time.deltaTime * 5);
                pitchAcceleration = Mathf.Lerp(pitchAcceleration, 0, Time.deltaTime * 4);
            }

            zoomAcceleration = Mathf.Lerp(zoomAcceleration, -Input.mouseScrollDelta.y, Time.deltaTime * 8f);
            
            distance += zoomAcceleration;
            distance = Mathf.Clamp(distance, 5f,15f);

            yaw += yawAcceleration;
            pitch += pitchAcceleration;
            pitch = Mathf.Clamp(pitch, 5f, 70f);

            transform.eulerAngles = new Vector3(pitch, yaw);
            transform.position = target.transform.position + transform.forward * -distance;
        }
    }
}