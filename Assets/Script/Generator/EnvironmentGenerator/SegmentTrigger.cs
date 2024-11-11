using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentTrigger : MonoBehaviour
{
    private EnvironmentStaticGenerator generator;

    void Start()
    {
        generator = FindObjectOfType<EnvironmentStaticGenerator>();
        if (generator == null)
        {
            Debug.LogError("EnvironmentGenerator не найден на сцене!");
        }
    }

        public void SetGenerator(EnvironmentStaticGenerator gen)
        {
            generator = gen;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && generator != null)
            {
                generator.OnTriggerEntered();
            }
        }
    }
