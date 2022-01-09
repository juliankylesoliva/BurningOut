using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintTextScript : MonoBehaviour
{
    [SerializeField] GameObject[] textObjects;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            foreach (GameObject g in textObjects)
            {
                g.SetActive(!g.activeInHierarchy);
            }
        }
    }
}
