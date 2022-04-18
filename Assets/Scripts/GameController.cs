using UnityEngine;

public class GameController : MonoBehaviour
{
    private Vector2 lastTouch;

    private int pressedMouseButton = -1;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void LateUpdate()
    {
        if (pressedMouseButton < 0)
        {
            // Test 0 and 1 mouse buttons.
            for (int i = 0; i < 2; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    pressedMouseButton = i;
                    OnTouchDown(Input.mousePosition);
                }
            }
        }
        if (pressedMouseButton >= 0 && Input.GetMouseButton(pressedMouseButton))
        {
            OnTouchDragged(Input.mousePosition);
        }
        if (pressedMouseButton >= 0 && !Input.GetMouseButton(pressedMouseButton))
        {
            OnTouchUp(Input.mousePosition);
            pressedMouseButton = -1;
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

        if (pressedMouseButton == 0)
        {
            Camera.main.transform.position += (Vector3)(lastWorldPos - worldPos);
        }
        else if (pressedMouseButton == 1)
        {
            Camera.main.orthographicSize += lastWorldPos.y - worldPos.y;
        }
    }

    private void OnTouchUp(Vector2 screenPos)
    {

    }

    private Vector2 ScreenToWorld(Vector2 screenPos) =>
        Camera.main.ScreenToWorldPoint(screenPos);
}