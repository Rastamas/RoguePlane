using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlashController : MonoBehaviour
{
    private const string URPShaderName = "Universal Render Pipeline/Lit";
    private Dictionary<Material, Color> _originalColors;
    private List<Material> _tintedMaterials;
    private Color _damageTakenColorStart;
    private Color _damageTakenColorEnd;
    private Material _flashingMaterial;
    private MeshRenderer[] _renderers;

    public void Awake()
    {
        _flashingMaterial = new Material(Shader.Find(URPShaderName));
        _damageTakenColorStart = Color.red;
        _damageTakenColorEnd = Color.green;
        _renderers = GetComponentsInChildren<MeshRenderer>();

        _originalColors = _renderers.SelectMany(r => r.materials).Where(m => m.HasProperty("_Color")).ToDictionary(x => x, x => x.color);
        _tintedMaterials = _renderers.Where(r => r.material.shader.FindPropertyIndex("_Tint") > 0).Select(r => r.material).ToList();

        foreach (var renderer in _renderers)
        {
            renderer.materials = renderer.materials.Concat(Enumerable.Repeat(_flashingMaterial, 1)).ToArray();
        }
    }


    public void Flash(float ratio)
    {
        var damageTakenColor = Color.Lerp(_damageTakenColorStart, _damageTakenColorEnd, ratio);

        foreach (var material in _renderers.SelectMany(r => r.materials))
        {
            material.color = damageTakenColor;
        }

        foreach (var material in _tintedMaterials)
        {
            material.SetColor("_Tint", damageTakenColor);
        }

        Invoke(nameof(ResetColor), 0.08f);
    }

    private void ResetColor()
    {
        foreach (var color in _originalColors)
        {
            color.Key.color = color.Value;
        }

        foreach (var material in _tintedMaterials)
        {
            material.SetColor("_Tint", Color.white);
        }
    }
}
