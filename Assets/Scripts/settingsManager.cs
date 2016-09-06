using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class settingsManager : MonoBehaviour {

    private bool soundOn;

    public Sprite[] soundIcons;

    // Use this for initialization
    void Start () {
        soundOn = PlayerPrefs.GetInt("sound", 1) > 0;
        updateSoundButton();

        if (!soundOn)
        {
            AudioListener.pause = true;
        }        
    }

    public void ToggleSound()
    {
        soundOn = !soundOn;
        AudioListener.pause = !soundOn;

        updateSoundButton();
    }

    private void updateSoundButton()
    {
        int soundVal = soundOn ? 1 : 0;

        PlayerPrefs.SetInt("sound", soundVal);

        GameObject.Find("SoundIcon").GetComponent<Image>().sprite = soundIcons[soundVal];
    }

}
