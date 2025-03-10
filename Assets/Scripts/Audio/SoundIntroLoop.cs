using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class SoundIntroLoop : MonoBehaviour
    {
        [SerializeField] private AudioSourceController _introAudioSource;

        /// <summary>BGM のループ部分の AudioSource。</summary>
        [SerializeField] private AudioSourceController _loopAudioSource = null;
        [SerializeField] private AudioSourceController _loopWebGLAudioSource = null;
        /// <summary>一時停止中かどうか。</summary>
        private bool _isPause;

        /// <summary>現在の再生するループ部分のインデックス。</summary>
        private int _nowPlayIndex = -1;

        private float _reservedTime = 44100;

        /// <summary>現在の再生時間 (s)。</summary>
        public float PlayingPer()
        {
            if (_introAudioSource == null)
            {
                return -1;
            }
            if (_introAudioSource.IsLoopEnded(_reservedTime))
            {
                var timeSamples = _loopAudioSource.timeSamples();
                if (timeSamples > 0)
                {
                    return timeSamples / _loopAudioSource.Clip.length;
                }
            } else
            {
                var timeSamples = _introAudioSource.timeSamples();
                if (timeSamples > 0)
                {
                    return timeSamples / _introAudioSource.Clip.length;
                }
            }
            return -1;
        }

        public float TimeStampPer(float per)
        {
            if (_introAudioSource.IsLoopEnded(_reservedTime))
            {
                if (_loopAudioSource != null)
                {
                    return _loopAudioSource.Clip.length * per;
                }
            } else
            {
                if (_introAudioSource != null)
                {
                    return _introAudioSource.Clip.length * per;
                }
            }
            return 0;
        }

        void Awake()
        {
            // AudioSource を自身に追加
            _introAudioSource.Initialize();
            _loopAudioSource.Initialize();
#if UNITY_WEBGL
            _loopWebGLAudioSource.Initialize();
#endif
        }

        public void SetSoloClip(AudioClip clip)
        {
            _introAudioSource.ResetReserveTimestamp();
            _introAudioSource.SetAudioData(clip,true);
            if (_loopWebGLAudioSource) 
            {
                _loopWebGLAudioSource.ResetReserveTimestamp();
                _loopWebGLAudioSource.SetAudioData(clip,true);
            }
        }

        public void SetClip(List<AudioClip> clips,bool isLoop)
        {
            _introAudioSource.ResetReserveTimestamp();
            _loopAudioSource.ResetReserveTimestamp();
            if (_loopWebGLAudioSource) 
            {
                _loopWebGLAudioSource.ResetReserveTimestamp();
            }
            if (clips.Count == 2)
            {
                _introAudioSource.SetAudioData(clips[0],clips[1] == null ? isLoop : false);
                _loopAudioSource.SetAudioData(clips[1],clips[1] == null ? false : isLoop);

#if UNITY_WEBGL
                _loopWebGLAudioSource.SetAudioData(clip[1]);
                if (clip[1] != null)
                {
                    _loopAudioSource.SetReserveTimestamp();
                    _loopWebGLAudioSource.SetReserveTimestamp();
                }
#endif
            } else
            {
                _introAudioSource.SetAudioData(clips[0],isLoop,false);
                _loopAudioSource.Stop();
                _loopAudioSource.SetAudioData(null);
#if UNITY_WEBGL
                _loopWebGLAudioSource.Stop();
                _loopWebGLAudioSource.SetAudioData(null);
#endif
            }
        }

        void Update()
        {
            if (_nowPlayIndex == 2 && _introAudioSource.IsLoopEnded(_reservedTime))
            {
                float reserve = _introAudioSource.ReserveTimeSample - _introAudioSource.timeSamples();
                _nowPlayIndex = 0;
                _loopAudioSource.PlayDelay(reserve / 44100);
                
            }
            // WebGL のためのループ切り替え処理
#if UNITY_WEBGL
            if (_loopAudioSource.ReserveTimeSample > -1 && _loopWebGLAudioSource.ReserveTimeSample > -1)
            {
                if (_nowPlayIndex == 0 && _loopAudioSource.IsLoopEnded(_reservedTime))
                {
                    float reserve = _loopAudioSource.ReserveTimeSample - _loopAudioSource.timeSamples();
                    _nowPlayIndex = 1;
                    _loopWebGLAudioSource.PlayDelay((reserve) / 44100);
                }
                else if (_nowPlayIndex == 1 && _loopWebGLAudioSource.IsLoopEnded(_reservedTime))
                {
                    float reserve = _loopWebGLAudioSource.ReserveTimeSample - _loopWebGLAudioSource.timeSamples();
                    _nowPlayIndex = 0;
                    _loopAudioSource.PlayDelay((reserve) / 44100);
                }
            }
#endif
        }

        public void Play(float timeStamp = 1)
        {
            // クリップが設定されていない場合は何もしない
            if (_introAudioSource == null || _loopAudioSource == null) return;

            // Pause 中は isPlaying は false
            // 標準機能だけでは一時停止中か判別不可能
            if (_isPause)
            {
                /*
                _introAudioSource.UnPause();
                if (_introAudioSource.isPlaying)
                {
                // イントロ中ならループ開始時間を残り時間で再設定
                _loopAudioSource.Stop();
                _loopAudioSource.PlayScheduled(AudioSettings.dspTime + _introAudioSource.clip.length - _introAudioSource.timeSamples);
                }
                else
                {
                #if UNITY_WEBGL
                    // WebGL の場合は切り替え処理を実行
                    if (_loopAudioSource.timeSamples > 0)
                    {
                    _loopAudioSource.UnPause();
                    if (_loopAudioSource.timeSamples >= _loopTimeSample)
                    {
                        _loopWebGLAudioSource.Stop();
                        _loopWebGLAudioSource.PlayScheduled(AudioSettings.dspTime + (_loopTimeSample - _loopAudioSource.timeSamples));
                        _nowPlayIndex = 1;
                    }
                    }
                    else
                    {
                    _loopWebGLAudioSource.UnPause();
                    if (_loopWebGLAudioSource.timeSamples >= _loopTimeSample - 1)
                    {
                        _loopAudioSource.Stop();
                        _loopAudioSource.PlayScheduled(AudioSettings.dspTime + (_loopTimeSample - _loopAudioSource.timeSamples));
                        _nowPlayIndex = 0;
                    }
                    }
                #else
                    // WebGL 以外は UnPause するだけ
                    _loopAudioSource.UnPause();
                #endif
                }
                */
            }
            else if (true)
            {
                Stop();
                if (_introAudioSource.Clip != null)
                {
                    _introAudioSource.SetReserveTimestamp();
                    if (_introAudioSource.IsLoopEnded(_reservedTime))
                    {
                        float reserve = _introAudioSource.ReserveTimeSample - _introAudioSource.timeSamples();
                        _nowPlayIndex = 0;
                        _loopAudioSource.Play((int)timeStamp);
                    } else
                    {
                        _introAudioSource.Play((int)timeStamp);
                        _nowPlayIndex = 2;
                    }
                    //_loopAudioSource.PlayScheduled(AudioSettings.dspTime + _introAudioSource.clip.length);
                } else
                {
                    _loopAudioSource.Play((int)timeStamp);
                }
            }

            _isPause = false;
        }

        /// <summary>BGM を一時停止します。</summary>
        public void Pause()
        {
            if (_introAudioSource == null || _loopAudioSource == null) return;

            _introAudioSource.Pause();
            _loopAudioSource.Pause();
            if (_loopWebGLAudioSource != null) _loopWebGLAudioSource.Pause();

            _isPause = true;
        }

        /// <summary>BGM を停止します。</summary>
        public void Stop()
        {
            if (_introAudioSource == null || _loopAudioSource == null) return;

            _introAudioSource.Stop();
            _loopAudioSource.Stop();
            if (_loopWebGLAudioSource != null) _loopWebGLAudioSource.Stop();
            _isPause = false;
        }

        public void ChangeVolume(float volume)
        {
            if (_introAudioSource == null || _loopAudioSource == null) return;
            _introAudioSource.ChangeVolume(volume);
            _loopAudioSource.ChangeVolume(volume);
            if (_loopWebGLAudioSource != null) _loopWebGLAudioSource.ChangeVolume(volume);
        }

        public void FadeVolume(float targetVolume,int duration)
        {
            if (_introAudioSource == null || _loopAudioSource == null) return;
            if (_introAudioSource.isPlaying())
            {
                _introAudioSource.FadeVolume(targetVolume,duration);
            } else
            {
                _introAudioSource.ChangeVolume(targetVolume);
            }
            if (_loopAudioSource.isPlaying())
            {
                _loopAudioSource.FadeVolume(targetVolume,duration);
            } else
            {   
                _loopAudioSource.ChangeVolume(targetVolume);
            }
        }
    }
}
