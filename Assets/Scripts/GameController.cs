using UnityEngine;

public class GameController : MonoBehaviour
{
    private bool isMouseDown = false;
    
    private Vector2 lastTouch;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void LateUpdate()
    {
        if (!isMouseDown && Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            OnTouchDown(Input.mousePosition);
        }
        if (isMouseDown && Input.GetMouseButton(0))
        {
            OnTouchDragged(Input.mousePosition);
        }
        if (isMouseDown && !Input.GetMouseButton(0))
        {
            OnTouchUp(Input.mousePosition);
            isMouseDown = false;
        }
    }

    private void OnTouchDown(Vector2 screenPos)
    {
        this.lastTouch = screenPos;
    }

    private void OnTouchDragged(Vector2 screenPos)
    {
        Vector2 worldPos = ScreenToWorld(screenPos);
        Vector2 lastWorldPos = ScreenToWorld(lastTouch);
        lastTouch = screenPos;

        Camera.main.transform.position += (Vector3)(lastWorldPos - worldPos);
    }

    private void OnTouchUp(Vector2 screenPos)
    {

    }

    private Vector2 ScreenToWorld(Vector2 screenPos) =>
        Camera.main.ScreenToWorldPoint(screenPos);
}