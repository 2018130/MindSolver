using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class TakePicture : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Camera previewCamera;
    [SerializeField]
    private Camera resultCamera;

    [SerializeField]
    private Transform origin;

    private PuzzleMissonListener puzzleMissonListener;

    private bool isFirstInteract = true;

    public void EndInteract()
    {
        Vector3 mouseToWorldPos = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);
        
        foreach (var col in Physics2D.OverlapPointAll(mouseToWorldPos))
        {
            if(col.transform == origin)
            {
                GameUIManager.Singleton.SetPictureText(false);
                puzzleMissonListener.EndPuzzle(true);
                return;
            }
        }
        GameUIManager.Singleton.SetPictureText(false);
        puzzleMissonListener.EndPuzzle(false);
        isFirstInteract = false;
    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
        if(isFirstInteract)
        {
            Vector3 camPos = worldPosFromMousePosition;
            camPos.z = -10;
            previewCamera.transform.position = camPos;
        }
    }

    private void Awake()
    {
        puzzleMissonListener = GetComponent<PuzzleMissonListener>();
    }

    private void OnEnable()
    {
        int x = Random.Range(Screen.width * 3 / 10, Screen.width * 7 / 10);
        int y = Random.Range(Screen.height * 3 / 10, Screen.height * 7 / 10);
        Vector2 screenPos = new Vector2(x, y);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        origin.transform.position = worldPos;
        resultCamera.transform.position = worldPos;
        GameUIManager.Singleton.SetPictureText(true);
        isFirstInteract = true;
    }

}
