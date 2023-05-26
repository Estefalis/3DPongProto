using Nova.Compat;
using Nova.InternalNamespace_0.InternalNamespace_4;
using Nova.InternalNamespace_0.InternalNamespace_5;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Nova.InternalNamespace_0.InternalNamespace_10
{
    
    internal unsafe class InternalType_177<T108> : InternalType_789 where T108 : unmanaged
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public int InternalProperty_1215 { get; private set; } = -1;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private bool InternalField_3788 = false;

        public InternalType_177(int InternalParameter_691)
        {
            InternalProperty_1215 = InternalParameter_691;

            if (InternalType_333.InternalProperty_1216)
            {
                InternalMethod_2346();
            }
            else
            {
                InternalField_3789 = new ComputeBuffer(InternalParameter_691, sizeof(T108), ComputeBufferType.Structured);
            }
        }

        private void InternalMethod_2346()
        {
            int InternalVar_1 = sizeof(T108);
            int InternalVar_2 = sizeof(float4);
            if (InternalVar_1 >= InternalVar_2)
            {
                InternalField_3788 = true;
                int InternalVar_3 = InternalProperty_1215 * InternalVar_1 / InternalVar_2;
                InternalField_3790 = new Texture2D(InternalVar_3, 1, TextureFormat.RGBAFloat, false, true);

                if (!UnityVersionUtils.Is2022OrNewer && !InternalField_3791.IsCreated)
                {
                    int InternalVar_4 = InternalVar_3 * UnsafeUtility.SizeOf<float4>();
                    InternalField_3791 = new NativeArray<float>(InternalVar_4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                }
            }
            else
            {
                InternalField_3788 = false;


                InternalField_3790 = new Texture2D(InternalProperty_1215, 1, TextureFormat.RFloat, false, true);
                if (!UnityVersionUtils.Is2022OrNewer && !InternalField_3791.IsCreated)
                {
                    InternalField_3791 = new NativeArray<float>(InternalProperty_1215, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                }
            }

            InternalField_3790.filterMode = FilterMode.Point;
        }

        
        private void InternalMethod_1531()
        {
            if (InternalField_3790 == null)
            {
                InternalMethod_2346();
            }
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InternalMethod_3795(ref NativeArray<T108> InternalParameter_2217, int InternalParameter_1416 = 0, int InternalParameter_2220 = 0, bool InternalParameter_2221 = true)
        {
            if (InternalType_333.InternalProperty_1216)
            {
                InternalMethod_1531();

                if (InternalField_3788)
                {
                    NativeArray<float4> InternalVar_1 = InternalParameter_2217.Reinterpret<float4>(sizeof(T108));

                    if (InternalField_3791.IsCreated)
                    {
                        NativeArray<float4> InternalVar_2 = InternalField_3791.Reinterpret<float4>(sizeof(float));
                        NativeArray<float4>.Copy(InternalVar_1, InternalVar_2, InternalVar_1.Length);
                    }
                    else
                    {
                        NativeArray<float4> InternalVar_2 = InternalField_3790.GetPixelData<float4>(0);
                        NativeArray<float4>.Copy(InternalVar_1, InternalVar_2, InternalVar_1.Length);
                    }

                    if (InternalParameter_2221)
                    {
                        InternalMethod_3749();
                    }

                }
                else
                {
                    NativeArray<float> InternalVar_1 = InternalParameter_2217.Reinterpret<float>(sizeof(T108));

                    if (InternalField_3791.IsCreated)
                    {
                        NativeArray<float>.Copy(InternalVar_1, InternalParameter_1416, InternalField_3791, InternalParameter_2220, InternalVar_1.Length);
                    }
                    else
                    {
                        NativeArray<float> InternalVar_2 = InternalField_3790.GetPixelData<float>(0);
                        NativeArray<float>.Copy(InternalVar_1, InternalParameter_1416, InternalVar_2, InternalParameter_2220, InternalVar_1.Length);
                    }

                    if (InternalParameter_2221)
                    {
                        InternalMethod_3749();
                    }
                }
            }
            else
            {
                InternalField_3789.SetData(InternalParameter_2217, InternalParameter_1416, InternalParameter_2220, InternalParameter_2217.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InternalMethod_3749()
        {
            if (InternalField_3790 != null)
            {
                if (InternalField_3791.IsCreated)
                {
                    if (InternalField_3788)
                    {
                        NativeArray<float4> InternalVar_1 = InternalField_3791.Reinterpret<float4>(sizeof(float));
                        InternalField_3790.SetPixelData(InternalVar_1, 0);
                    }
                    else
                    {
                        InternalField_3790.SetPixelData(InternalField_3791, 0);
                    }
                }

                InternalField_3790.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            }
        }
    }

    internal abstract class InternalType_789 : IDisposable
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        protected ComputeBuffer InternalField_3789 = null;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        protected Texture2D InternalField_3790 = null;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        protected NativeArray<float> InternalField_3791;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InternalMethod_3809(MaterialPropertyBlock InternalParameter_2222, int InternalParameter_1645)
        {
            if (InternalType_333.InternalProperty_1216)
            {
                InternalParameter_2222.SetTexture(InternalParameter_1645, InternalField_3790);
            }
            else
            {
                InternalParameter_2222.SetBuffer(InternalParameter_1645, InternalField_3789);
            }
        }

        public virtual void Dispose()
        {
            if (InternalField_3789 != null)
            {
                InternalField_3789.Dispose();
                InternalField_3789 = null;
            }

            InternalType_179.InternalMethod_848(InternalField_3790);
            InternalField_3790 = null;

            if (InternalField_3791.IsCreated)
            {
                InternalField_3791.Dispose();
            }
        }
    }

    internal static class InternalType_790
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InternalMethod_3813(this MaterialPropertyBlock InternalParameter_3608, int InternalParameter_3607, InternalType_789 InternalParameter_3537)
        {
            InternalParameter_3537.InternalMethod_3809(InternalParameter_3608, InternalParameter_3607);
        }

        public static void InternalMethod_3814(this InternalType_789 InternalParameter_3536)
        {
            if (InternalParameter_3536 != null)
            {
                InternalParameter_3536.Dispose();
            }
        }

        
        public static bool InternalMethod_3815<T>(ref InternalType_177<T> InternalParameter_3646, int InternalParameter_3647) where T : unmanaged
        {
            if (InternalParameter_3647 == 0 || (InternalParameter_3646 != null && InternalParameter_3646.InternalProperty_1215 >= InternalParameter_3647))
            {
                return false;
            }

            bool InternalVar_1 = false;
            if (InternalParameter_3646 != null)
            {
                InternalVar_1 = true;
                InternalParameter_3646.Dispose();
            }

            InternalParameter_3646 = new InternalType_177<T>(InternalParameter_3647);
            return InternalVar_1;
        }

        public static bool InternalMethod_3816<T>(ref InternalType_177<T> InternalParameter_3648, ref InternalType_164<T> InternalParameter_3649) where T : unmanaged
        {
            bool InternalVar_1 = InternalMethod_3815<T>(ref InternalParameter_3648, InternalParameter_3649.InternalProperty_216);

            if (InternalParameter_3649.InternalProperty_216 == 0 || InternalParameter_3648 == null)
            {
                return InternalVar_1;
            }

            using (var dataAsArray = InternalParameter_3649.InternalMethod_804())
            {
                NativeArray<T> InternalVar_2 = dataAsArray.Array;
                InternalParameter_3648.InternalMethod_3795(ref InternalVar_2);
            }
            return InternalVar_1;
        }
    }
}

