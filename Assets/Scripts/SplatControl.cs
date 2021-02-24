using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatControl : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject, 0.5f);
    }
}
