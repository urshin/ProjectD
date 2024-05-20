using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessingController : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
