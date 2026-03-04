using System.Threading.Tasks;
using UnityEngine;

namespace Utilities
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager s_Instance;

        private void Awake()
        {
            s_Instance = this;
        }

        public void PlaySFX(AudioClip clip, float volume = 1)
        {
            Play(clip, volume);
        }

        private async void Play(AudioClip clip, float volume = 1)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.Play();
        
            await Task.Delay((int)(clip.length * 1000));
        
            if(Application.isPlaying)
                Destroy(source);
        }
    }
}
