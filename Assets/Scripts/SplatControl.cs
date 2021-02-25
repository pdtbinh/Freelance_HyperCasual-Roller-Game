using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is used to remove unused Particle System effects.
public class SplatControl : MonoBehaviour
{
    // After the effects take place for long enough, remove it.
    private void Awake()
    {
        Destroy(gameObject, 0.5f);
    }
}
