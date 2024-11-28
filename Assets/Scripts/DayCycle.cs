using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayCycle : MonoBehaviour
{
    float x;
    float duration = 32.0f;

    Light light;

    void Start()
    {
        light = gameObject.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        x = 11.25f * Time.deltaTime;
        if (x <= 360)
        {x -= 360;}

        light.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time, duration));

        transform.Rotate(x, 0, 0);
    }
}
