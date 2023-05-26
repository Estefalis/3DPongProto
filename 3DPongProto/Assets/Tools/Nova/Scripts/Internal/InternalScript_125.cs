using Nova.Compat;
using Nova.InternalNamespace_0.InternalNamespace_4;
using Nova.InternalNamespace_0.InternalNamespace_2;
using Nova.InternalNamespace_0.InternalNamespace_9;
using Nova.InternalNamespace_0.InternalNamespace_5;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Nova.InternalNamespace_0.InternalNamespace_12
{
    internal partial class InternalType_460
    {
        
        [BurstCompile]
        internal struct InternalType_468
        {
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalField_2048;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NovaHashMap<InternalType_131, InternalType_478> InternalField_2049;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_53> InternalField_2050;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_53.InternalType_54> InternalField_2051;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_53.InternalType_55> InternalField_2052;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<bool> InternalField_2053;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<float3> InternalField_2054;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_99> InternalField_2055;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_98> InternalField_2056;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<float3> InternalField_2057;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_70> InternalField_2058;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_48.InternalType_50> InternalField_2059;

            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_222> InternalField_2060;
            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NovaHashMap<InternalType_131, InternalType_133> InternalField_2061;

            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_142> InternalField_2063;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_220> InternalField_2064;

            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<quaternion> InternalField_2065;
            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<float3> InternalField_2066;
            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<bool> InternalField_2067;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public InternalType_133 InternalMethod_3793(InternalType_133 InternalParameter_3599, bool InternalParameter_3600, out bool InternalParameter_3601, out bool InternalParameter_3602, bool InternalParameter_3603 = false)
            {
                InternalType_222 InternalVar_1 = InternalField_2060[InternalParameter_3599];
                InternalType_131 InternalVar_2 = InternalVar_1.InternalField_586;

                bool InternalVar_3 = InternalVar_2.InternalProperty_192;
                InternalType_133 InternalVar_4 = InternalVar_3 ? InternalField_2061[InternalVar_2] : InternalType_133.InternalField_418;

                bool InternalVar_5 = false;

                InternalParameter_3601 = false;
                InternalParameter_3602 = false;

                if (!InternalParameter_3603)
                {
                    bool InternalVar_6 = InternalField_2064[InternalParameter_3599].InternalProperty_250;

                    InternalType_220 InternalVar_7 = !InternalVar_3 ? InternalType_220.InternalField_3625 : InternalField_2064[InternalVar_4];
                    InternalVar_5 = InternalVar_7 == InternalType_220.InternalField_580;

                    if (!InternalVar_6 && !InternalVar_5)
                    {
                        if (InternalVar_7.InternalProperty_249)
                        {
                            InternalField_2064[InternalParameter_3599] = InternalType_220.InternalField_3626;
                        }

                        return InternalVar_4;
                    }
                }

                InternalType_448.InternalType_449 InternalVar_8 = InternalType_448.InternalMethod_1749(InternalParameter_3599, ref InternalField_2050, ref InternalField_2052);
                InternalVar_8.InternalMethod_1807(ref InternalField_2051);
                InternalVar_8.InternalMethod_1796(ref InternalField_2055);
                InternalVar_8.InternalMethod_1800(ref InternalField_2054);
                InternalVar_8.InternalMethod_1803(ref InternalField_2053);
                InternalVar_8.InternalMethod_1798(ref InternalField_2056);
                InternalVar_8.InternalMethod_1802(ref InternalField_2057);

                float3 InternalVar_9 = InternalVar_3 ? InternalMethod_1897(InternalVar_4) : InternalType_187.InternalField_530;
                bool3 InternalVar_10 = InternalType_187.InternalField_548;

                bool3 InternalVar_11 = InternalVar_3 ? InternalMethod_3822(ref InternalVar_8, InternalVar_4, out InternalParameter_3602, out InternalVar_10) : false;

                InternalParameter_3601 = math.any(InternalVar_11);

                InternalType_448.InternalType_450 InternalVar_12 = InternalVar_8.InternalMethod_1793();
                InternalVar_8.InternalMethod_3821(InternalVar_9, InternalParameter_3654: !(InternalParameter_3600 & InternalVar_11));

                if (InternalField_2048 && !InternalVar_3)
                {
                    InternalMethod_1040(ref InternalVar_8, InternalVar_1.InternalField_585);
                }

                if (InternalField_2067[InternalParameter_3599])
                {
                    float3 InternalVar_13 = InternalType_187.InternalField_530;

                    if (InternalVar_3)
                    {
                        InternalType_448.InternalType_449 InternalVar_14 = InternalType_448.InternalMethod_1749(InternalVar_4, ref InternalField_2050, ref InternalField_2052);
                        InternalVar_14.InternalMethod_1807(ref InternalField_2051);

                        InternalVar_13 = InternalVar_14.InternalProperty_378.InternalProperty_139;
                    }

                    float3 InternalVar_15 = InternalVar_8.InternalMethod_1784(ref InternalField_2065, ref InternalField_2053) + InternalVar_8.InternalProperty_379.InternalProperty_137;
                    float3 InternalVar_16 = InternalVar_8.InternalProperty_379.InternalProperty_139;

                    float3 InternalVar_17 = InternalType_182.InternalMethod_852(InternalField_2066[InternalParameter_3599], InternalVar_15, InternalVar_16, InternalVar_9, InternalVar_13, InternalField_2054[InternalParameter_3599]);
                    float3 InternalVar_18 = InternalType_53.InternalMethod_388(InternalVar_17, ref InternalVar_8.InternalProperty_361, ref InternalVar_8.InternalProperty_365, InternalVar_9);

                    InternalVar_8.InternalProperty_361.InternalProperty_115 = math.select(InternalVar_18, InternalVar_8.InternalProperty_361.InternalProperty_115, InternalVar_10);

                    InternalVar_8.InternalMethod_1787(InternalVar_9);
                }

                ref InternalType_70 InternalVar_19 = ref InternalField_2058.ElementAt(InternalParameter_3599);
                ref InternalType_48.InternalType_50 InternalVar_20 = ref InternalField_2059.ElementAt(InternalParameter_3599);
                float2 InternalVar_21 = InternalVar_20.InternalProperty_113;

                if (InternalVar_19.InternalProperty_155)
                {
                    InternalMethod_3794(ref InternalVar_19, ref InternalVar_20, InternalParameter_3599);
                }

                if (!InternalParameter_3603)
                {
                    InternalType_220 InternalVar_22 = InternalParameter_3601 ? InternalType_220.InternalField_580 : InternalVar_8.InternalMethod_1791(ref InternalVar_12);

                    InternalType_220 InternalVar_23 = math.any(InternalVar_21 != InternalVar_20.InternalProperty_113) ? InternalType_220.InternalField_580 : InternalType_220.InternalField_3625;
                    InternalType_220 InternalVar_24 = InternalType_220.InternalMethod_1052(InternalVar_22, InternalVar_23);
                    InternalVar_24 = InternalVar_5 ? InternalType_220.InternalMethod_1052(InternalVar_24, InternalType_220.InternalField_579) : InternalVar_24;

                    InternalField_2064[InternalParameter_3599] = InternalType_220.InternalMethod_1052(InternalVar_24, InternalField_2064[InternalParameter_3599]);
                }

                return InternalVar_4;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float3 InternalMethod_1897(InternalType_133 InternalParameter_715)
            {
                InternalType_448.InternalType_453 InternalVar_1 = InternalType_448.InternalMethod_1751(InternalParameter_715, ref InternalField_2052);
                return InternalVar_1.InternalProperty_402;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void InternalMethod_3794(ref InternalType_70 InternalParameter_3604, ref InternalType_48.InternalType_50 InternalParameter_3605, InternalType_133 InternalParameter_3606)
            {
                InternalType_448.InternalType_453 InternalVar_1 = InternalType_448.InternalMethod_1751(InternalParameter_3606, ref InternalField_2052);

                float3 InternalVar_2 = InternalVar_1.InternalProperty_402;

                int InternalVar_3 = InternalParameter_3604.InternalField_225.InternalMethod_508();
                float InternalVar_4 = InternalVar_2[InternalVar_3];

                if (InternalParameter_3604.InternalField_228)
                {
                    InternalType_45 InternalVar_5 = default;
                    InternalParameter_3605.InternalField_3739 = new InternalType_45.InternalType_47(InternalVar_5, InternalParameter_3604.InternalField_227, InternalVar_4);
                }
                else
                {
                    InternalParameter_3605.InternalField_3739 = InternalParameter_3604.InternalMethod_451(InternalVar_4);
                }

                if (InternalParameter_3604.InternalProperty_1185)
                {
                    int InternalVar_5 = InternalParameter_3604.InternalField_3738.InternalField_3741.InternalMethod_508();
                    float InternalVar_6 = InternalVar_2[InternalVar_5];

                    if (InternalParameter_3604.InternalField_3738.InternalField_3744)
                    {
                        InternalType_45 InternalVar_7 = default;
                        InternalParameter_3605.InternalField_3740 = new InternalType_45.InternalType_47(InternalVar_7, InternalParameter_3604.InternalField_3738.InternalField_3743, InternalVar_6);
                    }
                    else
                    {
                        InternalParameter_3605.InternalField_3740 = InternalParameter_3604.InternalField_3738.InternalMethod_3739(InternalVar_6);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void InternalMethod_1040(ref InternalType_448.InternalType_449 InternalParameter_1765, InternalType_131 InternalParameter_1632)
            {
                if (!InternalField_2049.TryGetValue(InternalParameter_1632, out InternalType_478 InternalVar_1) || !InternalVar_1.InternalField_2146)
                {
                    return;
                }

                InternalType_98 InternalVar_2 = InternalParameter_1765.InternalProperty_357;
                if (InternalVar_2.InternalProperty_173)
                {
                    int InternalVar_3 = InternalVar_2.InternalField_309.InternalMethod_508();
                    float3 InternalVar_4 = InternalVar_1.InternalField_2145;
                    InternalVar_4[InternalVar_3] = InternalParameter_1765.InternalProperty_360[InternalVar_3].InternalProperty_102 || InternalParameter_1765.InternalProperty_354[InternalVar_3] == InternalType_83.InternalField_281 ? InternalVar_1.InternalField_2145[InternalVar_3] / InternalParameter_1765.InternalProperty_360[InternalVar_3].InternalField_145 : InternalVar_4[InternalVar_3];

                    InternalParameter_1765.InternalMethod_1788(InternalVar_4, true);
                }
                else
                {
                    InternalParameter_1765.InternalProperty_376 = new InternalType_53.InternalType_55()
                    {
                        InternalField_177 = new InternalType_45.InternalType_47(InternalVar_1.InternalField_2145.x, InternalParameter_1765.InternalProperty_360.InternalField_167.InternalProperty_102 ? InternalParameter_1765.InternalProperty_360.InternalField_167.InternalField_145 : 1),
                        InternalField_178 = new InternalType_45.InternalType_47(InternalVar_1.InternalField_2145.y, InternalParameter_1765.InternalProperty_360.InternalField_168.InternalProperty_102 ? InternalParameter_1765.InternalProperty_360.InternalField_168.InternalField_145 : 1),
                        InternalField_179 = new InternalType_45.InternalType_47(InternalVar_1.InternalField_2145.z, InternalParameter_1765.InternalProperty_360.InternalField_169.InternalProperty_102 ? InternalParameter_1765.InternalProperty_360.InternalField_169.InternalField_145 : 1),
                    };
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool3 InternalMethod_3822(ref InternalType_448.InternalType_449 InternalParameter_3655, InternalType_133 InternalParameter_3656, out bool InternalParameter_3657, out bool3 InternalParameter_3658)
            {
                bool3 InternalVar_1 = InternalField_2055.ElementAt(InternalParameter_3656).InternalProperty_177;

                ref InternalType_70 InternalVar_2 = ref InternalField_2058.ElementAt(InternalParameter_3656);
                InternalParameter_3658 = InternalVar_2.InternalMethod_449() | InternalVar_2.InternalField_3738.InternalMethod_3737();

                bool3 InternalVar_3 = InternalParameter_3655.InternalProperty_381;
                bool3 InternalVar_4 = InternalParameter_3655.InternalProperty_354.InternalProperty_178;

                InternalParameter_3657 = math.any(InternalParameter_3655.InternalProperty_360.InternalProperty_118 | InternalVar_4);

                return (InternalVar_1 & InternalVar_3) | (InternalParameter_3658 & InternalVar_4);
            }
        }
    }
}
