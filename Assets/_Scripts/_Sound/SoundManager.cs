using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Biblioteca de sonidos")]
    [SerializeField] private SoundLibrarySO library;

    private Dictionary<SoundType, SoundEventSO> _eventByType;
    private Dictionary<SoundType, List<EventInstance>> _oneShotInstances;
    private Dictionary<SoundType, EventInstance> _loopInstances;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _eventByType = new Dictionary<SoundType, SoundEventSO>();
        _oneShotInstances = new Dictionary<SoundType, List<EventInstance>>();
        _loopInstances = new Dictionary<SoundType, EventInstance>();

        foreach (var evSO in library.soundEvents)
        {
            _eventByType[evSO.type] = evSO;
            _oneShotInstances[evSO.type] = new List<EventInstance>();
            _loopInstances[evSO.type] = default;
        }
    }

    private void Update()
    {
        foreach (var kvp in _oneShotInstances)
        {
            var list = kvp.Value;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var inst = list[i];
                if (!inst.isValid())
                {
                    list.RemoveAt(i);
                    continue;
                }
                inst.getPlaybackState(out var state);
                if (state == PLAYBACK_STATE.STOPPED)
                {
                    inst.release();
                    list.RemoveAt(i);
                }
            }
        }
    }

    public EventInstance CreateEventInstance(SoundType type, Vector3 pos)
    {
        if (!_eventByType.TryGetValue(type, out var evSO))
        {
            Debug.LogWarning($"No se encontró configuración de sonido para {type}");
            return default;
        }

        var inst = RuntimeManager.CreateInstance(evSO.fmodEvent);
        inst.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        return inst;
    }

    public void PlaySound(SoundType type, Vector3 pos)
    {
        if (!_eventByType.TryGetValue(type, out var evSO))
            return;

        if (!evSO.ignoreHearingRange && !IsInHearingRange(pos, evSO.audibleRange))
            return;

        if (!evSO.isLoop)
            PlayOneShot(evSO, pos);
        else
            PlayLoop(evSO, pos);
    }

    private void PlayOneShot(SoundEventSO evSO, Vector3 pos)
    {
        var list = _oneShotInstances[evSO.type];
        if (evSO.maxVoices > 0 && list.Count >= evSO.maxVoices)
            return;

        var inst = RuntimeManager.CreateInstance(evSO.fmodEvent);
        inst.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        inst.start();
        list.Add(inst);
    }

    private void PlayLoop(SoundEventSO evSO, Vector3 pos)
    {
        if (_loopInstances.TryGetValue(evSO.type, out var existing) && existing.isValid())
        {
            existing.getPlaybackState(out var state);
            if (state == PLAYBACK_STATE.PLAYING)
                return;
        }

        var inst = RuntimeManager.CreateInstance(evSO.fmodEvent);
        inst.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        inst.start();
        _loopInstances[evSO.type] = inst;
    }

    public void StopLoop(SoundType type)
    {
        if (_loopInstances.TryGetValue(type, out var inst) && inst.isValid())
        {
            inst.stop(STOP_MODE.IMMEDIATE);
            inst.release();
            _loopInstances[type] = default;
        }
    }

    public void StopAllLaserLoops()
    {
        StopLoop(SoundType.LaserCharge);
        StopLoop(SoundType.LaserFire);
    }

    private bool IsInHearingRange(Vector3 soundPosition, float maxDistance)
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("Camera.main is null — permitiendo sonido por seguridad.");
            return true;
        }

        float distance = Vector3.Distance(Camera.main.transform.position, soundPosition);
        return distance <= maxDistance;
    }
}
