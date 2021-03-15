using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.5f;
    [SerializeField] private float speed = 5;
    private Camera camera;
    private float currentPitch = 0;

    private void Start()
    {
        camera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
        }

        Rotate();
        Move();
    }

    void Rotate()
    {
        Vector2 change = new Vector2(
            0.1f * Input.GetAxis("Mouse X") * camera.fieldOfView * sensitivity,
            0.1f * Input.GetAxis("Mouse Y") * camera.fieldOfView * sensitivity);
        transform.Rotate(new Vector3(currentPitch, 0, 0));
        transform.Rotate(new Vector3(0, change.x, 0));
        currentPitch += change.y;
        currentPitch = Mathf.Clamp(currentPitch, -90, 90);
        transform.Rotate(new Vector3(-currentPitch, 0, 0));
    }

    void Move()
    {
        Vector3 translation = new Vector3(0, 0, 0);
        translation.z += Input.GetKey(KeyCode.W) ? 1 : 0;
        translation.z -= Input.GetKey(KeyCode.S) ? 1 : 0;
        translation.y += Input.GetKey(KeyCode.Space) ? 1 : 0;
        translation.y -= Input.GetKey(KeyCode.LeftControl) ? 1 : 0;
        translation.x += Input.GetKey(KeyCode.D) ? 1 : 0;
        translation.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
        transform.Translate(translation * (speed * Time.deltaTime));
    }
}