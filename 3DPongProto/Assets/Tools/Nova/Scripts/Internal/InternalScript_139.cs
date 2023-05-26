using Nova.Compat;
using Nova.InternalNamespace_0.InternalNamespace_2;
using Nova.InternalNamespace_0.InternalNamespace_9;
using Nova.InternalNamespace_0.InternalNamespace_5;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace Nova.InternalNamespace_0.InternalNamespace_12
{
    internal partial class InternalType_460
    {
        
        internal struct InternalType_469
        {
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_53> InternalField_2068;

            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_53.InternalType_54> InternalField_2069;

            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_53.InternalType_55> InternalField_2070;

            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<bool> InternalField_2071;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<float3> InternalField_2072;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_99> InternalField_2073;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_98> InternalField_2074;

            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<float3> InternalField_2075;

            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<quaternion> InternalField_2076;

            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_142> InternalField_2077;

            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_222> InternalField_2078;
            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NovaHashMap<InternalType_131, InternalType_133> InternalField_2079;

            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_70> InternalField_2081;
            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_48.InternalType_50> InternalField_2082;
            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NovaHashMap<InternalType_133, InternalType_783> InternalField_2083;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_786> InternalField_3774;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_787> InternalField_3775;

            [ReadOnly]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NovaHashMap<InternalType_131, InternalType_456> InternalField_2084;

            [NativeDisableParallelForRestriction]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public NativeList<InternalType_220> InternalField_2085;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            private InternalType_133 InternalField_2088;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            private InternalType_448.InternalType_450 InternalField_2092;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            private InternalType_785 InternalField_3776;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void InternalMethod_1885(InternalType_133 InternalParameter_2087, bool InternalParameter_2088 = false)
            {
                InternalField_3776 = default;

                InternalField_2088 = InternalParameter_2087;
                InternalField_3776.InternalField_3756 = InternalField_2078[InternalField_2088];

                InternalType_448.InternalType_449 InternalVar_1 = InternalType_448.InternalMethod_1749(InternalField_2088, ref InternalField_2068, ref InternalField_2070);
                InternalVar_1.InternalMethod_1807(ref InternalField_2069);
                InternalVar_1.InternalMethod_1796(ref InternalField_2073);

                InternalField_2092 = InternalVar_1.InternalMethod_1793();

                bool3 InternalVar_2 = InternalField_2092.InternalField_1815.InternalProperty_177;

                bool InternalVar_3 = math.any(InternalVar_2);

                InternalField_3776.InternalField_3754 = InternalField_2081[InternalField_2088];

                if (!InternalVar_3 && !InternalField_3776.InternalField_3754.InternalProperty_155)
                {
                    return;
                }

                if (!InternalParameter_2088 && !InternalMethod_1889())
                {
                    return;
                }

                InternalField_3776.InternalField_3758 = math.max(math.select(InternalField_2092.InternalField_1811.InternalProperty_124, InternalVar_1.InternalProperty_364.InternalProperty_120, InternalVar_2) - InternalField_2092.InternalField_1813.InternalProperty_137, 0);
                InternalField_3776.InternalField_3757 = InternalField_2082[InternalField_2088];

                int InternalVar_4 = InternalField_3776.InternalProperty_1195.InternalMethod_508();
                int InternalVar_5 = InternalField_3776.InternalProperty_1196.InternalMethod_508();

                float3 InternalVar_8 = InternalMethod_3796(InternalVar_1, InternalField_3776.InternalField_3758, out float InternalVar_6, out int InternalVar_7);

                InternalType_456 InternalVar_9 = default;

                if (InternalVar_3)
                {
                    if (InternalField_2084.TryGetValue(InternalField_3776.InternalField_3756.InternalField_585, out InternalVar_9))
                    {
                        InternalVar_8 = math.select(InternalVar_8, InternalVar_9.InternalField_1839, InternalVar_9.InternalField_1840);
                    }

                    float3 InternalVar_10 = math.max(InternalVar_1.InternalProperty_364.InternalMethod_398(InternalVar_8 + InternalField_2092.InternalField_1813.InternalProperty_137) - InternalField_2092.InternalField_1813.InternalProperty_137, 0);
                    InternalField_3776.InternalField_3758 = math.select(InternalField_3776.InternalField_3758, InternalVar_10, InternalVar_2);

                    InternalMethod_3804(ref InternalField_3776.InternalField_3758);
                }

                if (InternalField_3776.InternalField_3754.InternalProperty_155)
                {
                    InternalType_786 InternalVar_10 = new InternalType_786()
                    {
                        InternalField_3760 = 0,
                        InternalField_3761 = InternalVar_7,
                        InternalField_3762 = InternalVar_7,
                        InternalField_3763 = InternalVar_8[InternalVar_4],
                        InternalField_3764 = InternalVar_6,
                        InternalField_3766 = InternalVar_4,
                        InternalField_3767 = InternalField_3776.InternalField_3758[InternalVar_4],
                        InternalField_3765 = InternalField_3776.InternalField_3756.InternalProperty_253,
                    };

                    InternalMethod_3797(ref InternalVar_10, out float InternalVar_11, out float InternalVar_12);

                    if (!InternalVar_9.InternalField_1840[InternalVar_4])
                    {
                        InternalVar_8[InternalVar_4] = InternalVar_11;
                    }

                    if (InternalField_3776.InternalField_3754.InternalProperty_1185 && !InternalVar_9.InternalField_1840[InternalVar_5])
                    {
                        InternalVar_8[InternalVar_5] = InternalVar_12;
                    }
                }

                InternalField_2082.ElementAt(InternalField_2088) = InternalField_3776.InternalField_3757;

                bool InternalVar_13 = InternalMethod_1891(ref InternalVar_8);

                if (InternalVar_13)
                {
                    InternalField_2085[InternalField_2088] = InternalType_220.InternalField_580;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float3 InternalMethod_3796(InternalType_448.InternalType_449 InternalParameter_3611, float3 InternalParameter_3612, out float InternalParameter_3613, out int InternalParameter_3614)
            {
                int InternalVar_1 = InternalField_3776.InternalField_3756.InternalProperty_253;
                bool InternalVar_2 = InternalField_2077[InternalField_2088].InternalField_426;

                int InternalVar_3 = InternalField_3776.InternalProperty_1195.InternalMethod_508();

                if (!InternalVar_2 && InternalField_3776.InternalField_3754.InternalProperty_155 && InternalField_2083.TryGetValue(InternalField_2088, out InternalType_783 InternalVar_4))
                {
                    InternalField_3776.InternalField_3755 = InternalVar_4.InternalField_3750 == InternalType_96.InternalField_304 ? InternalVar_3 != InternalType_187.InternalField_498 : InternalVar_4.InternalField_3750.InternalMethod_508() == InternalType_187.InternalField_498;
                }
                else
                {
                    InternalField_3776.InternalField_3755 = InternalType_187.InternalField_548;
                }

                bool3 InternalVar_5 = InternalParameter_3611.InternalProperty_354.InternalProperty_177;

                InternalType_448.InternalType_449 InternalVar_6 = InternalType_448.InternalMethod_1749(InternalType_133.InternalField_418, ref InternalField_2068, ref InternalField_2070);
                InternalVar_6.InternalMethod_1796(ref InternalField_2073);
                InternalVar_6.InternalMethod_1807(ref InternalField_2069);
                InternalVar_6.InternalMethod_1798(ref InternalField_2074);

                InternalMethod_3799(0, InternalVar_1 - 1, InternalVar_6, InternalVar_5, out float3x2 InternalVar_7, out float2 InternalVar_8, out InternalParameter_3614);

                InternalParameter_3613 = InternalVar_8.y;

                float3 InternalVar_9 = math.max(InternalType_187.InternalField_530, InternalVar_7.c1 - InternalVar_7.c0);

                if (!InternalField_3776.InternalField_3754.InternalProperty_155 || InternalField_3776.InternalField_3754.InternalProperty_1185)
                {
                    return InternalVar_9;
                }

                float InternalVar_10 = InternalVar_8.x;
                bool InternalVar_11 = InternalField_3776.InternalProperty_1193;

                if (InternalVar_11)
                {
                    float InternalVar_12 = InternalParameter_3614 == 0 ? InternalVar_10 : InternalParameter_3612[InternalVar_3];
                    float InternalVar_13 = (InternalParameter_3612[InternalVar_3] - InternalVar_12) / math.max(InternalVar_1 - 1, 1);
                    InternalField_3776.InternalProperty_1207 = new InternalType_45() { InternalField_145 = InternalField_3776.InternalProperty_1209.InternalMethod_349(InternalVar_13) };
                }

                float InternalVar_14 = InternalField_3776.InternalProperty_1209.InternalMethod_349(InternalField_3776.InternalProperty_1207.InternalField_145);

                if (InternalField_3776.InternalProperty_1207.InternalProperty_102)
                {
                    InternalVar_14 = InternalVar_5[InternalVar_3] ? InternalField_3776.InternalProperty_1209.InternalMethod_349(InternalVar_10 * InternalField_3776.InternalProperty_1207.InternalField_145) : InternalField_3776.InternalProperty_1209.InternalMethod_349(InternalParameter_3612[InternalVar_3] * InternalField_3776.InternalProperty_1207.InternalField_145);
                }

                InternalVar_10 += (InternalVar_1 - 1) * InternalVar_14;

                float InternalVar_15 = InternalVar_5[InternalVar_3] ? InternalVar_10 : InternalParameter_3612[InternalVar_3];
                InternalField_3776.InternalProperty_1205 = InternalField_3776.InternalMethod_3786(InternalVar_15);

                InternalVar_9[InternalVar_3] = InternalVar_10;

                return InternalVar_9;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void InternalMethod_3797(ref InternalType_786 InternalParameter_3615, out float InternalParameter_3616, out float InternalParameter_3617)
            {
                InternalType_448.InternalType_449 InternalVar_1 = InternalType_448.InternalMethod_1749(InternalType_133.InternalField_418, ref InternalField_2068, ref InternalField_2070);
                InternalVar_1.InternalMethod_1807(ref InternalField_2069);
                InternalVar_1.InternalMethod_1796(ref InternalField_2073);
                InternalVar_1.InternalMethod_1798(ref InternalField_2074);
                InternalVar_1.InternalMethod_1802(ref InternalField_2075);
                InternalVar_1.InternalMethod_1800(ref InternalField_2072);
                InternalVar_1.InternalMethod_3751(ref InternalField_2076);
                InternalVar_1.InternalMethod_1803(ref InternalField_2071);

                bool InternalVar_2 = InternalField_3776.InternalField_3754.InternalProperty_1185;

                float InternalVar_3 = 0;
                float InternalVar_4 = 0;

                if (!InternalVar_2)
                {
                    if (InternalParameter_3615.InternalField_3762 > 0)
                    {
                        InternalVar_3 = InternalMethod_3798(ref InternalParameter_3615, out float InternalVar_5);
                        InternalField_3776.InternalMethod_3790(ref InternalParameter_3615, ref InternalVar_5);
                        InternalParameter_3615.InternalField_3763 = InternalParameter_3615.InternalField_3767 - InternalVar_5;
                    }
                    
                    InternalVar_4 = InternalField_3776.InternalProperty_1199 ? -0.5f * InternalParameter_3615.InternalField_3763 : 0;
                }

                InternalType_784.InternalMethod_3754(InternalVar_1, ref InternalField_3776, InternalVar_4, InternalVar_3, ref InternalField_3774, out InternalParameter_3616);

                InternalParameter_3617 = 0;

                if (InternalVar_2)
                {
                    InternalType_784.InternalMethod_3755(InternalVar_1, ref InternalField_3776, ref InternalField_3774, ref InternalField_3775, out InternalParameter_3617);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float InternalMethod_3798(ref InternalType_786 InternalParameter_3618, out float InternalParameter_3619)
            {
                InternalType_448.InternalType_449 InternalVar_1 = InternalType_448.InternalMethod_1749(InternalType_133.InternalField_418, ref InternalField_2068, ref InternalField_2070);
                InternalVar_1.InternalMethod_1796(ref InternalField_2073);
                InternalVar_1.InternalMethod_1807(ref InternalField_2069);

                return InternalType_784.InternalMethod_3757(InternalVar_1, ref InternalField_3776, ref InternalParameter_3618, out InternalParameter_3619);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void InternalMethod_3799(int InternalParameter_3620, int InternalParameter_3621, InternalType_448.InternalType_449 InternalParameter_3622, bool3 InternalParameter_3623, out float3x2 InternalParameter_3624, out float2 InternalParameter_3625, out int InternalParameter_3626)
            {
                int InternalVar_1 = InternalParameter_3621 - InternalParameter_3620 + 1;

                InternalParameter_3624 = new float3x2(float.MaxValue, float.MinValue);

                int InternalVar_2 = InternalField_3776.InternalProperty_1195.InternalMethod_508();
                bool InternalVar_3 = InternalVar_2 >= 0;

                InternalParameter_3625 = 0;
                InternalParameter_3626 = 0;

                InternalField_3776.InternalField_3759 = float.MaxValue;

                if (InternalVar_3 && InternalField_3776.InternalField_3754.InternalProperty_1185 && InternalField_3776.InternalField_3754.InternalField_3738.InternalField_3747)
                {
                    for (int InternalVar_4 = 0; InternalVar_4 < InternalVar_1; ++InternalVar_4)
                    {
                        InternalParameter_3622.InternalProperty_353 = InternalField_3776.InternalField_3756.InternalField_587[InternalVar_4];

                        bool InternalVar_5 = InternalParameter_3622.InternalProperty_354[InternalVar_2] == InternalType_83.InternalField_281;

                        if (!InternalVar_5)
                        {
                            continue;
                        }

                        InternalField_3776.InternalField_3759 = math.min(InternalField_3776.InternalField_3759, InternalParameter_3622.InternalProperty_364[InternalVar_2].InternalField_148);
                    }
                }

                if (math.any(InternalParameter_3623))
                {
                    for (int InternalVar_4 = 0; InternalVar_4 < InternalVar_1; ++InternalVar_4)
                    {
                        InternalParameter_3622.InternalProperty_353 = InternalField_3776.InternalField_3756.InternalField_587[InternalVar_4];

                        float3 InternalVar_6 = InternalMethod_3800(ref InternalParameter_3622, InternalParameter_3623, InternalVar_2, out bool InternalVar_5);

                        InternalParameter_3624.c0 = math.min(InternalParameter_3624.c0, InternalVar_6 * InternalType_187.InternalField_499.c0);
                        InternalParameter_3624.c1 = math.max(InternalParameter_3624.c1, InternalVar_6 * InternalType_187.InternalField_499.c1);

                        if (!InternalVar_3)
                        {
                            continue;
                        }

                        InternalParameter_3625.x += InternalVar_6[InternalVar_2];

                        if (InternalVar_5)
                        {
                            InternalParameter_3626++;
                            InternalParameter_3625.y += math.max(InternalVar_6[InternalVar_2], 0);
                        }
                    }

                    return;
                }

                for (int InternalVar_4 = 0; InternalVar_4 < InternalVar_1; ++InternalVar_4)
                {
                    InternalParameter_3622.InternalProperty_353 = InternalField_3776.InternalField_3756.InternalField_587[InternalVar_4];

                    float3 InternalVar_6 = InternalMethod_3801(ref InternalParameter_3622, InternalVar_2, out bool InternalVar_5);

                    InternalParameter_3624.c0 = math.min(InternalParameter_3624.c0, InternalVar_6 * InternalType_187.InternalField_499.c0);
                    InternalParameter_3624.c1 = math.max(InternalParameter_3624.c1, InternalVar_6 * InternalType_187.InternalField_499.c1);

                    if (!InternalVar_3)
                    {
                        continue;
                    }

                    InternalParameter_3625.x += InternalVar_6[InternalVar_2];

                    if (InternalVar_5)
                    {
                        InternalParameter_3626++;
                        InternalParameter_3625.y += math.max(InternalParameter_3622.InternalProperty_364[InternalVar_2].InternalField_148, 0);
                    }
                }
            }

            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float3 InternalMethod_3800(ref InternalType_448.InternalType_449 InternalParameter_3627, bool3 InternalParameter_3628, int InternalParameter_3629, out bool InternalParameter_3630)
            {
                bool3 InternalVar_1 = !InternalParameter_3628;

                InternalParameter_3630 = InternalParameter_3629 >= 0 && InternalParameter_3627.InternalProperty_354[InternalParameter_3629] == InternalType_83.InternalField_281;

                bool InternalVar_2 = InternalField_2071[InternalParameter_3627.InternalProperty_353];
                quaternion InternalVar_3 = InternalVar_2 ? InternalField_2076[InternalParameter_3627.InternalProperty_353] : InternalType_187.InternalField_3748;

                float3 InternalVar_4 = InternalParameter_3627.InternalProperty_360.InternalProperty_117;
                float3 InternalVar_5 = InternalParameter_3627.InternalProperty_376.InternalProperty_124 * InternalType_187.InternalMethod_887(!InternalParameter_3627.InternalProperty_380 | InternalVar_1);

                if (InternalParameter_3630)
                {
                    InternalVar_5[InternalParameter_3629] = 0;
                    InternalVar_4[InternalParameter_3629] = 0;

                    if (InternalParameter_3627.InternalProperty_357.InternalField_309.InternalMethod_508() == InternalParameter_3629)
                    {
                        InternalVar_5 = 0;
                        InternalVar_4 = 0;
                    }
                }

                float3 InternalVar_6 = InternalVar_2 ? InternalType_182.InternalMethod_859(InternalVar_5, InternalVar_3) : InternalVar_5;

                float3x2 InternalVar_7 = InternalParameter_3627.InternalProperty_379.InternalProperty_133;
                InternalVar_7.c0 *= InternalType_187.InternalMethod_887(!InternalParameter_3627.InternalProperty_363.InternalProperty_127 | InternalVar_1);
                InternalVar_7.c1 *= InternalType_187.InternalMethod_887(!InternalParameter_3627.InternalProperty_363.InternalProperty_128 | InternalVar_1);
                float3x2 InternalVar_8 = InternalParameter_3627.InternalProperty_363.InternalProperty_130;

                InternalType_56.InternalType_57 InternalVar_9 = InternalParameter_3627.InternalProperty_367;
                float3x2 InternalVar_10 = InternalVar_9.InternalProperty_131;
                InternalVar_10.c0 = math.max(InternalVar_10.c0, InternalType_187.InternalField_530);
                InternalVar_10.c1 = math.max(InternalVar_10.c1, InternalType_187.InternalField_530);
                InternalVar_9.InternalProperty_131 = InternalVar_10;

                InternalType_53.InternalType_54 InternalVar_11 = InternalParameter_3627.InternalProperty_364;
                InternalVar_11.InternalProperty_120 = math.max(InternalVar_11.InternalProperty_120, InternalType_187.InternalField_530);

                InternalType_53.InternalType_54 InternalVar_12 = InternalVar_11 + InternalVar_9.InternalProperty_1186 + InternalVar_9.InternalProperty_1187;

                float3 InternalVar_13 = InternalVar_6 + InternalVar_7.c0 + InternalVar_7.c1;
                float3 InternalVar_14 = (InternalVar_4 + InternalVar_8.c0 + InternalVar_8.c1) * InternalType_187.InternalMethod_887(InternalParameter_3628);

                return InternalVar_12.InternalMethod_398(InternalMethod_3802(ref InternalVar_13, ref InternalVar_14));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float3 InternalMethod_3801(ref InternalType_448.InternalType_449 InternalParameter_3631, int InternalParameter_3632, out bool InternalParameter_3633)
            {
                InternalParameter_3633 = InternalParameter_3632 >= 0 && InternalParameter_3631.InternalProperty_354[InternalParameter_3632] == InternalType_83.InternalField_281;

                bool InternalVar_1 = InternalField_2071[InternalParameter_3631.InternalProperty_353];
                quaternion InternalVar_2 = InternalVar_1 ? InternalField_2076[InternalParameter_3631.InternalProperty_353] : InternalType_187.InternalField_3748;
                float3 InternalVar_3 = InternalParameter_3631.InternalProperty_376.InternalProperty_124;

                if (InternalParameter_3633)
                {
                    InternalVar_3[InternalParameter_3632] = InternalParameter_3631.InternalProperty_364[InternalParameter_3632].InternalMethod_349(0);
                }

                InternalVar_3 = InternalVar_1 ? InternalType_182.InternalMethod_859(InternalVar_3, InternalVar_2) : InternalVar_3;

                return InternalVar_3 + InternalParameter_3631.InternalProperty_379.InternalProperty_137;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float3 InternalMethod_3802(ref float3 InternalParameter_3634, ref float3 InternalParameter_3635)
            {
                return new float3(InternalMethod_3803(InternalParameter_3634.x, InternalParameter_3635.x),
                                  InternalMethod_3803(InternalParameter_3634.y, InternalParameter_3635.y),
                                  InternalMethod_3803(InternalParameter_3634.z, InternalParameter_3635.z));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float InternalMethod_3803(float InternalParameter_3636, float InternalParameter_3637)
            {
                bool InternalVar_1 = !InternalType_187.InternalMethod_3644(InternalParameter_3637, 1f);

                float InternalVar_2 = InternalParameter_3636 / (1f - InternalParameter_3637);
                float InternalVar_3 = math.select(InternalVar_2, InternalParameter_3636, InternalVar_1);

                return math.max(InternalParameter_3636, InternalVar_3);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void InternalMethod_3804(ref float3 InternalParameter_3638)
            {
                InternalType_448.InternalType_449 InternalVar_1 = InternalType_448.InternalMethod_1749(InternalType_133.InternalField_418, ref InternalField_2068, ref InternalField_2070);
                InternalVar_1.InternalMethod_1807(ref InternalField_2069);
                InternalVar_1.InternalMethod_1796(ref InternalField_2073);
                InternalVar_1.InternalMethod_1798(ref InternalField_2074);
                InternalVar_1.InternalMethod_1802(ref InternalField_2075);

                int InternalVar_2 = InternalField_3776.InternalField_3756.InternalProperty_253;

                for (int InternalVar_3 = 0; InternalVar_3 < InternalVar_2; ++InternalVar_3)
                {
                    InternalVar_1.InternalProperty_353 = InternalField_3776.InternalField_3756.InternalField_587[InternalVar_3];
                    float3 InternalVar_4 = InternalVar_1.InternalProperty_383;

                    if (InternalParameter_3638.Equals(InternalVar_4))
                    {
                        continue;
                    }

                    InternalVar_1.InternalMethod_3821(InternalParameter_3638, InternalType_187.InternalField_3800);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool InternalMethod_1891(ref float3 InternalParameter_2098)
            {
                bool3 InternalVar_1 = InternalField_2073.ElementAt(InternalField_2088).InternalProperty_177;
                InternalType_448.InternalType_449 InternalVar_2 = InternalType_448.InternalMethod_1749(InternalField_2088, ref InternalField_2068, ref InternalField_2070);

                if (!math.any(InternalVar_1))
                {
                    return false;
                }

                InternalVar_2.InternalMethod_1807(ref InternalField_2069);
                InternalVar_2.InternalMethod_1796(ref InternalField_2073);
                InternalVar_2.InternalMethod_1798(ref InternalField_2074);
                InternalVar_2.InternalMethod_1802(ref InternalField_2075);
                InternalVar_2.InternalMethod_1786();

                ref InternalType_56 InternalVar_3 = ref InternalVar_2.InternalProperty_362;

                float3 InternalVar_4 = InternalVar_2.InternalProperty_378.InternalProperty_137;
                float3 InternalVar_5 = InternalParameter_2098 + InternalVar_4;

                float3 InternalVar_6 = InternalVar_2.InternalProperty_360.InternalProperty_115;
                InternalVar_2.InternalProperty_360.InternalProperty_118 = InternalVar_2.InternalProperty_360.InternalProperty_118 & !InternalVar_1;
                InternalVar_2.InternalProperty_360.InternalProperty_115 = math.select(InternalVar_6, InternalVar_5, InternalVar_1);

                float3 InternalVar_7 = InternalVar_2.InternalProperty_383;

                InternalVar_2.InternalMethod_1788(InternalVar_7, InternalVar_1);
                InternalVar_2.InternalMethod_1786();

                return math.any(InternalVar_2.InternalProperty_360.InternalProperty_115 != InternalVar_6);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool InternalMethod_1889()
            {
                if (InternalField_2085[InternalField_2088] == InternalType_220.InternalField_580)
                {
                    return true;
                }

                bool InternalVar_1 = false;
                int InternalVar_2 = InternalField_3776.InternalField_3756.InternalProperty_253;

                for (int InternalVar_3 = 0; InternalVar_3 < InternalVar_2; ++InternalVar_3)
                {
                    InternalType_133 InternalVar_4 = InternalField_3776.InternalField_3756.InternalField_587[InternalVar_3];

                    if (InternalField_2085[InternalVar_4].InternalProperty_250)
                    {
                        InternalVar_1 = true;
                        break;
                    }
                }

                return InternalVar_1;
            }
        }
    }
}
