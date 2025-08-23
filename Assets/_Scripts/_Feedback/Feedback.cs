using UnityEngine;

public abstract class Feedback : MonoBehaviour
{
    // Sobrecarga del m�todo CreateFeedback para distintos tipos de feedback
    public virtual void CreateFeedback() { }
    public virtual void CreateFeedback(Vector2 direction) { }

    public abstract void CompletePreviousFeedback();
}
