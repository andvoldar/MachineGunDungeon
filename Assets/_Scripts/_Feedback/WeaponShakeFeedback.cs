using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShakeFeedback : Feedback
{

    [SerializeField]
    private GameObject objectToShake;
    [SerializeField]
    private float duration = 0.2f, strength = 1, randomness = 90;
    [SerializeField]
    private int vibrato = 10;
    [SerializeField]
    private bool snapping = false, fadeOut = true;


    public override void CompletePreviousFeedback()
    {
        objectToShake.transform.DOComplete();
    }

    public override void CreateFeedback()
    {
        CompletePreviousFeedback();
        objectToShake.transform.DOShakePosition(duration,strength,vibrato,randomness,snapping,fadeOut);
    }
}
