using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float timeScale;
    public Vector3 rotationAxis;
    public bool DayTime;
    // Start is called before the first frame update
    void Start()
    {
        timeScale = 100f;
        rotationAxis = new Vector3(1, 0, 0);
        transform.SetPositionAndRotation(rotationAxis, Quaternion.Euler(90, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationAxis, Time.deltaTime / timeScale);
        if (transform.eulerAngles.x < 170 && transform.eulerAngles.x > 10)
            DayTime = true;
        else
            DayTime = false;
    }
}
