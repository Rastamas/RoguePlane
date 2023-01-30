using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveTrigger : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private const string DoDissolveProperty = "_DoDissolve";
    private const string TriggerTimeProperty = "_TriggerTime";

    public void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Dissolve()
    {
        meshRenderer.material.SetFloat(DoDissolveProperty, 1);
        meshRenderer.material.SetFloat(TriggerTimeProperty, Time.time);
    }

    public void Appear()
    {
        meshRenderer.material.SetFloat(DoDissolveProperty, -1);
        meshRenderer.material.SetFloat(TriggerTimeProperty, Time.time);
    }
}
