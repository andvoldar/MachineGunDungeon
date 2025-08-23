// EnemyPerception.cs
using UnityEngine;
using System;

public class EnemyPerception
{
    private Transform enemyTransform;
    private Transform playerTransform;
    private float detectionRange;
    private float disengageRange;

    public float furiousDetectionRange { get; set; }
    public float furiousChaseSpeed { get; set; }
    public Transform CurrentTarget { get; private set; }

    public event Action<Transform> OnTargetDetected;

    public EnemyPerception(
        Transform enemyTransform,
        Transform playerTransform,
        float detectionRange,
        float disengageRange,
        float furiousChaseSpeed,
        float furiousDetectionRange)
    {
        this.enemyTransform = enemyTransform;
        this.playerTransform = playerTransform;
        this.detectionRange = detectionRange;
        this.disengageRange = disengageRange;
        this.furiousChaseSpeed = furiousChaseSpeed;
        this.furiousDetectionRange = furiousDetectionRange;
    }

    public void SetDetectionRange(float range)
    {
        detectionRange = range;
    }

    public void SetDisengageRange(float range)
    {
        disengageRange = range;
    }

    public bool IsPlayerInDetectionRange()
    {
        if (playerTransform == null) return false;
        float sqrDistance = (playerTransform.position - enemyTransform.position).sqrMagnitude;
        return sqrDistance <= detectionRange * detectionRange;
    }

    public bool IsPlayerOutOfDisengageRange()
    {
        if (playerTransform == null) return true;
        float sqrDistance = (playerTransform.position - enemyTransform.position).sqrMagnitude;
        return sqrDistance > disengageRange * disengageRange;
    }

    public void UpdatePerception()
    {
        if (playerTransform == null)
        {
            if (CurrentTarget != null)
            {
                CurrentTarget = null;
                OnTargetDetected?.Invoke(null);
            }
            return;
        }

        float sqrDistance = (playerTransform.position - enemyTransform.position).sqrMagnitude;

        if (sqrDistance <= detectionRange * detectionRange)
        {
            if (CurrentTarget != playerTransform)
            {
                CurrentTarget = playerTransform;
                OnTargetDetected?.Invoke(CurrentTarget);
            }
        }
        else if (sqrDistance > disengageRange * disengageRange)
        {
            if (CurrentTarget != null)
            {
                CurrentTarget = null;
                OnTargetDetected?.Invoke(null);
            }
        }
    }

    public bool HasTarget()
    {
        return CurrentTarget != null;
    }

    public float GetDetectionRange()
    {
        return detectionRange;
    }

    public float GetDisengageRange()
    {
        return disengageRange;
    }
}
