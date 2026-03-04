using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [field: Header("Event Instances")]
    [field: SerializeField] private EventInstance musicEventInstance;

    [Header("Event Instances Management")]
    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    private void Start()
    {
        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();
    }

    #region AudioManagerFunctions
    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        eventInstances.Clear();

        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }

        eventEmitters.Clear();
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    #endregion

    #region PlaySoundFunctions

    public void StopMusic()
    {
        musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicEventInstance.release();
    }

    public void PlayMainMenuMusic()
    {
        StopMusic();
        musicEventInstance = CreateInstance(FMODEvents.Instance.musicMainMenu);
        musicEventInstance.start();
    }

    public void PlayTypingMusic()
    {
        StopMusic();
        musicEventInstance = CreateInstance(FMODEvents.Instance.musicTyping);
        musicEventInstance.start();
    }

    public void PlayWaitingMusic()
    {
        StopMusic();
        musicEventInstance = CreateInstance(FMODEvents.Instance.musicWaiting);
        musicEventInstance.start();
    }

    #endregion
}
