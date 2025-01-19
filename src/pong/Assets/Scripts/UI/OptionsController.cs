using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour
{
    [SerializeField]
    private Slider masterVolumeSlider;

    [SerializeField]
    private AudioMixer audioMixer;

    private AudioSource optionsControllerAudioSource;

    private void Start()
    {
        this.optionsControllerAudioSource = this.GetComponent<AudioSource>();
        var eventTrigger = this.masterVolumeSlider.GetComponent<EventTrigger>();
        
        this.RegisterEventTriggerEntries(eventTrigger);

        this.masterVolumeSlider.onValueChanged.AddListener((float volume) =>
        {
            this.SetVolume(volume);
        });
    }

    private void RegisterEventTriggerEntries(EventTrigger eventTrigger)
    {
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.EndDrag,
        };

        endDragEntry.callback.AddListener((_) =>
        {
            this.AudioCheck();
        });

        EventTrigger.Entry clickEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick,
        };

        clickEntry.callback.AddListener((_) =>
        {
            this.AudioCheck();
        });

        eventTrigger.triggers.AddRange(new List<EventTrigger.Entry>
        {
            clickEntry,
            endDragEntry,
        });
    }

    private void SetVolume(float volume)
    {
        this.audioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
    }

    private void AudioCheck()
    {
        this.optionsControllerAudioSource.Play();
    }
}
