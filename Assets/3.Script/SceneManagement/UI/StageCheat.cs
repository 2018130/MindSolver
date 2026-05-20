using UnityEngine;
using UnityEngine.UI;

public class StageCheat : MonoBehaviour
{
    private int count = 0;

    public void SetStageToZeroFirebase()
    {
        count++;
        if(count >= 5)
        {
            Image image = GetComponent<Image>();
            Color color = image.color;
            color.a = 0.5f;
            image.color = color;

            PersistentDataManager.Singleton.IsDefaultPlay = true;
        }
    }
}
