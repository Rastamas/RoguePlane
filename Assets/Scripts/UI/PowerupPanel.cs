using TMPro;
using UnityEngine;

public class PowerupPanel : MonoBehaviour
{
    public PowerupPopupController powerupController;
    public TextMeshProUGUI title;

    public void Awake()
    {
        title.text = "CHOOSE POWERUP";
    }
}
