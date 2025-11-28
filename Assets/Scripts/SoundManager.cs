using Unity.Netcode;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip titleBgmClip;
    public AudioClip gameBgmClip;
    public AudioClip typingClip;
    public AudioClip submitClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayTitleBgm();
    }

    public void PlayTitleBgm()
    {
        if (bgmSource.clip == titleBgmClip && bgmSource.isPlaying) return;

        bgmSource.clip = titleBgmClip;
        bgmSource.Play();
    }

    public void PlayGameBgm()
    {
        if (bgmSource.clip == gameBgmClip && bgmSource.isPlaying) return;

        bgmSource.clip = gameBgmClip;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        bgmSource.Stop();
    }

    public void PlayTypingSfx()
    {
        sfxSource.PlayOneShot(typingClip);
    }

    [Rpc(SendTo.Server)]
    public void PlaySubmitSfxServerRpc()
    {
        PlaySubmitSfxClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlaySubmitSfxClientRpc()
    {
        sfxSource.PlayOneShot(submitClip);
    }
}