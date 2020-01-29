using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class PerlinExplorer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Test(8, 8, 100);
    }

    private void Test(int w, int h, int steps)
    {
        using (var samples = Sample(w * steps, h * steps, 1f / steps, Allocator.Temp))
        {
            ScanMinMax(samples, out var min, out var max);
            var rmin = Remap(min);
            var rmax = Remap(max);
            var rmin2 = Remap(min);
            var rmax2 = Remap(max);

            Debug.Log($"({w}, {h}) --> [{min}, {max}] ~~> [{rmin}, {rmax}] ~~~~> [{rmin2}, {rmax2}] ");
        }
    }

    float Remap(float input)
    {
        return math.unlerp(-1f, 1f, input);
    }
    float Remap2(float input)
    {
        return (input + 1f) / 2f;
    }


    NativeArray<float> Sample(int w, int h, float delta, Allocator allocator)
    {
        var temp = new NativeArray<float>(w * h, allocator);

        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
            temp[x * h + y] = noise.cnoise(new float2(x * delta, y * delta));
        return temp;
    }

    void ScanMinMax(NativeArray<float> samples, out float min, out float max)
    {
        min = samples[0];
        max = samples[0];
        for (var i = 1; i < samples.Length; i++)
        {
            var c = samples[i];
            if (min > c)
                min = c;
            if (max < c)
                max = c;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}