// Assets/Scripts/Audio/SoundLibrarySO.cs
using UnityEngine;

/// <summary>
/// Agrupa todos los SoundEventSO de tu proyecto.
/// </summary>
[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrarySO : ScriptableObject
{
    [Tooltip("Arrastra aquí TODOS tus SoundEventSO")]
    public SoundEventSO[] soundEvents;

    [Header("Bus de láser (FMOD)")]
    [Tooltip("Ruta FMOD del bus que agrupa LaserCharge/Fire (no Release)")]
    public string laserBusPath = "bus:/SFX/Laser";
}
