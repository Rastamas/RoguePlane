using UnityEngine;

public class VolumeAdjuster : MonoBehaviour
{
    public void Awake()
    {
        GetComponent<AudioSource>().volume *= PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
    }
}
