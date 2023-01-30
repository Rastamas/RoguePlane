using UnityEngine;

public class CarrierShieldController : MonoBehaviour
{
    private DissolveTrigger _dissolveTrigger;

    public void Awake()
    {
        _dissolveTrigger = GetComponent<DissolveTrigger>();
    }

    public void Enable()
    {
        _dissolveTrigger.Appear();
    }

    public void Disable()
    {
        _dissolveTrigger.Dissolve();
    }
}
