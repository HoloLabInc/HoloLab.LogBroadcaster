using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interval : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IntervalProc());
    }

    IEnumerator IntervalProc()
    {
        int type = 0;

        while (true)
        {
            if (type == 0)
            {
                Debug.Log($"{DateTime.Now.ToString()}");
                type++;
            }
            else if (type == 1)
            {
                Debug.LogWarning($"{DateTime.Now.ToString()}");
                type++;
            }
            else if (type == 2)
            {
                Debug.LogError($"{DateTime.Now.ToString()}");
                type++;
            }
            else if (type == 3)
            {
                Debug.LogException(new Exception());
                type++;
            }
            else if (type == 4)
            {
                Debug.LogAssertion($"{DateTime.Now.ToString()}");
                type = 0;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }
}
