using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace Ryneus
{
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        private float _bgmVolume = 1.0f;
        public float BgmVolume => _bgmVolume;
        public void SetBgmVolume(float volume) => _bgmVolume = volume;
        public float SeVolume = 1.0f;
        public bool BGMMute = false;
        public bool SeMute = false;
        private List<AudioSource> _se;
        private List<AudioSource> _playingSe = new();
        private int _seAudioSourceNum = 16;
        private List<AudioSource> _staticSe = new();
        private List<SoundData> _seMaster;

        [SerializeField] private SoundIntroLoop _bgmMain;
        [SerializeField] private SoundIntroLoop _bgmSub;
        [SerializeField] private SoundIntroLoop _bgsTrack;

        private bool _crossFadeMode = false;
        public bool CrossFadeMode => _crossFadeMode;
        private bool _playingIsMain = true;

        private string _lastPlayAudio = "";
        private float _lastBgmVolume = 0f;

        public async Task Initialize()
        {
            // 全体シーンで使うサウンドを初期ロード
            await LoadStaticSe();
            // SeAudioSourceを生成
            _se = new List<AudioSource>();
            for (int i = 0; i < _seAudioSourceNum; i++)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                _se.Add(audioSource);
            }
        }

        private async Task LoadStaticSe()
        {
            _staticSe.Clear();
            _seMaster = DataSystem.SE.FindAll(a => a != null);
            foreach (var seMaster in _seMaster)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                _staticSe.Add(audioSource);
                SetSeAudio(audioSource, seMaster.FileName, seMaster.Volume, seMaster.Pitch);
            }
        }


        private async void SetSeAudio(AudioSource audioSource, string sePath, float volume, float pitch)
        {
            audioSource.clip = await ResourceSystem.LoadSeAsset(sePath);
            audioSource.pitch = pitch;
            audioSource.volume = volume;
        }

        private void LateUpdate()
        {
            //UpdateVolume(_seData, _seVolume);
        }

        public void UpdateBgmVolume()
        {
            if (BGMMute)
            {
                _bgmMain.ChangeVolume(0);
                _bgmSub.ChangeVolume(0);
            }
            else
            {
                var volume = _bgmVolume * _lastBgmVolume;
                var playingTrack = _playingIsMain ? _bgmMain : _bgmSub;
                playingTrack.ChangeVolume(volume);
            }
        }

        public void UpdateSeVolume()
        {
            foreach (var staticSe in _staticSe)
            {
                if (staticSe.clip == null)
                {
                    continue;
                }
                float baseVolume = _seMaster.Find(a => a.FileName == staticSe.clip.name).Volume;
                staticSe.rolloffMode = AudioRolloffMode.Linear;
                staticSe.volume = SeVolume * baseVolume;
            }
        }

        public void UpdateSeMute()
        {
            if (SeMute)
            {
            }
            else
            {
                UpdateSeMute();
            }
        }

        public void PlayBgm(List<AudioClip> clips, float volume = 1.0f, bool loop = true, float timeStamp = 0)
        {
            if (clips[0].name == _lastPlayAudio)
            {
                return;
            }
            _lastBgmVolume = volume;
            _lastPlayAudio = clips[0].name;
            // これから再生するTrackを停止して再生
            var playTrack = _playingIsMain ? _bgmSub : _bgmMain;
            playTrack.Stop();
            playTrack.SetClip(clips, loop);
            playTrack.ChangeVolume(volume * _bgmVolume);
            playTrack.Play(timeStamp);
            playTrack.FadeVolume(volume * _bgmVolume, 1);

            // 再生中の方をフェードアウト
            var playingTrack = _playingIsMain ? _bgmMain : _bgmSub;
            playingTrack.FadeVolume(0, 1);
            //UpdateBgmVolume();
            _crossFadeMode = false;
            _playingIsMain = !_playingIsMain;
        }

        public void PlayBgs(AudioClip clip, float volume = 1.0f, bool loop = true)
        {
            _bgsTrack.Stop();
            _bgsTrack.SetClip(new List<AudioClip>() { clip }, loop);

            UpdateBgmVolume();
            _bgsTrack.Play();
            _bgsTrack.FadeVolume(volume * _bgmVolume, 1);
        }

        public void ChangeCrossFade(List<AudioClip> clips, float volume = 1.0f, bool loop = true)
        {
            var timeStamp = CurrentTimeStamp();
            _lastBgmVolume = volume;
            _lastPlayAudio = clips[0].name;
            // これから再生するTrackを停止して再生
            var playTrack = _playingIsMain ? _bgmSub : _bgmMain;
            playTrack.Stop();
            playTrack.SetClip(clips, loop);
            playTrack.Play(timeStamp + 28000);
            playTrack.FadeVolume(volume * _bgmVolume, 4);

            // 再生中の方をフェードアウト
            var playingTrack = _playingIsMain ? _bgmMain : _bgmSub;
            playingTrack.FadeVolume(0, 4);
            //UpdateBgmVolume();
            _crossFadeMode = false;
            _playingIsMain = !_playingIsMain;
            /*
            if (!_crossFadeMode)
            {
                return;
            }
            var playingTrack = _bgmMain;
            var resumeTrack = _bgmSub;

            var playingPer = playingTrack.PlayingPer();
            var timeStamp = resumeTrack.TimeStampPer(playingPer);
            _lastBgmVolume = volume;

            _playingIsMain = !_playingIsMain;
            playingTrack.FadeVolume(0, 1);
            UpdateBgmVolume();
            resumeTrack.Play(timeStamp);
            var playVolume = _bgmVolume * _lastBgmVolume;
            if (BGMMute)
            {
                playVolume = 0;
            }
            resumeTrack.FadeVolume(playVolume,1);
            */
        }

        public void StopBgm()
        {
            _bgmSub.Stop();
            _bgmMain.Stop();
            _lastPlayAudio = null;
        }

        public void StopBgs()
        {
            _bgsTrack.Stop();
        }

        public float CurrentTimeStamp()
        {
            var playingTrack = _playingIsMain ? _bgmMain : _bgmSub;
            var playingPer = playingTrack.PlayingPer();
            return playingTrack.TimeStampPer(playingPer);
        }

        public void FadeOutBgm()
        {
            var playingTrack = _playingIsMain ? _bgmMain : _bgmSub;
            playingTrack.FadeVolume(0, 1);
            _lastPlayAudio = null;
        }

        public void FadeOutBgs()
        {
            _bgsTrack.FadeVolume(0, 1);
        }

        public async void PlaySe(AudioClip clip, float volume, float pitch = 1f, int delayFrame = 0)
        {
            int audioSourceIndex = -1;
            for (int i = 0; i < _seAudioSourceNum; i++)
            {
                if (!_se[i].isPlaying && !_playingSe.Contains(_se[i]))
                {
                    audioSourceIndex = i;
                    break;
                }
            }
            if (audioSourceIndex > -1)
            {
                _se[audioSourceIndex].clip = clip;
                _se[audioSourceIndex].volume = volume * SeVolume;
                _se[audioSourceIndex].pitch = pitch;
                _playingSe.Add(_se[audioSourceIndex]);
                if (delayFrame > 0)
                {
                    await UniTask.DelayFrame(delayFrame);
                }
                _se[audioSourceIndex].Play();
                _playingSe.Remove(_se[audioSourceIndex]);
            }
        }

        public void PlayStaticSe(SEType sEType, float volume = 1.0f)
        {
            Debug.Log(sEType);
            if (SeMute)
            {
                return;
            }
            var seIndex = DataSystem.SE.FindIndex(a => a.Id == (int)sEType);
            if (seIndex > -1)
            {
                _staticSe[seIndex].Play();
            }
        }

        public async UniTask PlayStaticSeAsync(SEType sEType, float volume = 1.0f)
        {
            var seIndex = DataSystem.SE.FindIndex(a => a.Id == (int)sEType);
            if (seIndex > -1)
            {
                _staticSe[seIndex].Play();
                await UniTask.WaitUntil(() => _staticSe[seIndex].isPlaying);
                await UniTask.WaitWhile(() => _staticSe[seIndex].isPlaying);
            }
        }

        public void AllPause()
        {
            foreach (var data in _staticSe)
            {
                if (data.isPlaying)
                {
                    data.Pause();
                }
            }
        }

        public void AllUnPause()
        {
            foreach (var data in _staticSe)
            {
                if (data.isPlaying)
                {
                    data.UnPause();
                }
            }
        }
    }

    public enum AudioTrackType
    {
        Main = 0,
        Sub = 1
    }
}