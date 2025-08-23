using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ProjectileImpact : MonoBehaviour
{



    public void DestroyAfterAnimation()
    {
        Destroy(gameObject);
    }



}
