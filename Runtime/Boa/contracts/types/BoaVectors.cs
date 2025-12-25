using UnityEngine;

namespace _COBRA_.Boa.contracts.types
{
    static class Cmd_Vectors
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            DevContract.AddContract(new(
                name: "vector2",
                output_type: typeof(Vector2),
                arguments: new() { typeof(float), typeof(float), },
                action: static (memstack, memscope, prms) =>
                {
                    float x = prms.arguments[0];
                    float y = prms.arguments[1];
                    Vector2 v2 = new(x, y);
                    memstack.Add(new(v2));
                }
            ));

            DevContract.AddContract(new(
                name: "vector3",
                output_type: typeof(Vector3),
                arguments: new() { typeof(float), typeof(float), typeof(float), },
                action: static (memstack, memscope, prms) =>
                {
                    float x = prms.arguments[0];
                    float y = prms.arguments[1];
                    float z = prms.arguments[2];
                    Vector3 v3 = new(x, y, z);
                    memstack.Add(new(v3));
                }
            ));

            DevContract.AddContract(new(
                name: "vector4",
                output_type: typeof(Vector4),
                arguments: new() { typeof(float), typeof(float), typeof(float), typeof(float), },
                action: static (memstack, memscope, prms) =>
                {
                    float x = prms.arguments[0];
                    float y = prms.arguments[1];
                    float z = prms.arguments[2];
                    float w = prms.arguments[2];
                    Vector4 v4 = new(x, y, z, w);
                    memstack.Add(new(v4));
                }
            ));
        }
    }
}