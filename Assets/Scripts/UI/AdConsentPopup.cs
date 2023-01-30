using System;
using UnityEngine;
using UnityEngine.UI;

public class AdConsentPopup : MonoBehaviour
{
    private static AdConsentPopup _instance;
    public static AdConsentPopup instance
    {
        get
        {
            if (_instance == null)
            {
                var go = Instantiate(Resources.Load<GameObject>("Prefabs/UI/AdConfirmationPopup"),
                    parent: UIController.instance.canvas.transform);
                _instance = go.GetComponent<AdConsentPopup>();
                go.SetActive(false);
            }
            return _instance;
        }
    }

    public Button randomButton;
    public Button personalButton;

    public Action onChoose;

    public void Choose(bool storeUserContent)
    {
        PlayerPrefs.SetInt(Constants.AdConsentPlayerPrefKey, (int)(storeUserContent ? UserConsent.Accept : UserConsent.Deny));
        gameObject.SetActive(false);
        onChoose?.Invoke();
    }

    public enum UserConsent
    {
        Unset = 0,
        Accept = 1,
        Deny = 2
    }
}
