using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuneTestScript : MonoBehaviour
{
   [SerializeField] CarController_ CarController_;
    // Start is called before the first frame update
    void Start()
    {
        CarController_.InitializeIngame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializedSettint()
    {
        
        CarController_.InitializeIngame();
    }
}
