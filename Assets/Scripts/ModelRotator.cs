using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    // public float rotationSpeed = 100.0f;
    // private float lastMouseX;
    
    public float rotationSpeed = 0.005f;    // 鼠标滑动控制旋转的灵敏度
    public float rotationSpeedTouch = 0.5f;    // 触屏控制旋转的灵敏度

    public float inertiaDamping = 0.5f;   // 惯性阻尼，值越大停得越快

    private float currentVelocity = 0f;
    private float lastMouseX;
    private bool isDragging = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 触屏支持
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastMouseX = touch.position.x;
                currentVelocity = 0f;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                float deltaX = touch.position.x - lastMouseX;
                currentVelocity = deltaX * rotationSpeedTouch;
                transform.Rotate(Vector3.up, -currentVelocity, Space.Self);
                lastMouseX = touch.position.x;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMouseX = Input.mousePosition.x;
                currentVelocity = 0f;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                float mouseX = Input.mousePosition.x;
                float deltaX = mouseX - lastMouseX;
                currentVelocity = deltaX * rotationSpeed;
                transform.Rotate(Vector3.up, -currentVelocity, Space.Self);
                // lastMouseX = mouseX;
            }
            else
            {
                // 惯性旋转 + 衰减
                if (Mathf.Abs(currentVelocity) > 0.01f)
                {
                    transform.Rotate(Vector3.up, -currentVelocity, Space.Self);
                    currentVelocity = Mathf.Lerp(currentVelocity, 0f, Time.deltaTime * inertiaDamping);
                }
            }
        }
        
    }
}
