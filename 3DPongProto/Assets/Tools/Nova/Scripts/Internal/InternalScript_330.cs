using Nova.InternalNamespace_0.InternalNamespace_9;
using Nova.InternalNamespace_0.InternalNamespace_5;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace Nova.InternalNamespace_0.InternalNamespace_12
{
    internal partial class InternalType_460
    {
        public static class InternalType_784
        {
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            private const int InternalField_3752 = 10;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            private const float InternalField_3753 = 5e-4f;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void InternalMethod_3754(InternalType_448.InternalType_449 InternalParameter_3543, ref InternalType_785 InternalParameter_3544, float InternalParameter_3545, float InternalParameter_3546, ref NativeList<InternalType_786> InternalParameter_3547, out float InternalParameter_3548)
            {
                int InternalVar_1 = InternalParameter_3544.InternalProperty_1195.InternalMethod_508();
                float InternalVar_2 = InternalParameter_3544.InternalField_3754.InternalField_231;
                float InternalVar_3 = InternalParameter_3544.InternalProperty_1197;

                bool InternalVar_4 = InternalVar_3 == 0;

                bool InternalVar_5 = InternalParameter_3544.InternalField_3754.InternalProperty_1185;
                float InternalVar_6 = InternalVar_5 ? 0 : InternalVar_2;
                int InternalVar_7 = InternalParameter_3544.InternalProperty_1196.InternalMethod_508();

                bool InternalVar_8 = InternalVar_5 ? false : InternalParameter_3544.InternalProperty_1203;
                bool InternalVar_9 = InternalParameter_3544.InternalProperty_1203;

                bool InternalVar_10 = math.any(InternalParameter_3544.InternalField_3755);
                float3 InternalVar_11 = InternalVar_10 ? InternalType_187.InternalMethod_887(!InternalParameter_3544.InternalField_3755) : InternalType_187.InternalField_531;

                int InternalVar_12 = math.select(1, -1, InternalVar_8);
                int InternalVar_13 = math.select(0, InternalParameter_3544.InternalField_3756.InternalField_587.InternalProperty_216 - 1, InternalVar_8);
                int InternalVar_14 = math.select(InternalParameter_3544.InternalField_3756.InternalField_587.InternalProperty_216, -1, InternalVar_8);

                float InternalVar_15 = InternalParameter_3544.InternalField_3758[InternalVar_1];

                InternalParameter_3547.Clear();

                if (InternalVar_5)
                {
                    InternalParameter_3547.Add(new InternalType_786() { InternalField_3760 = InternalVar_13, InternalField_3766 = InternalVar_1, InternalField_3767 = InternalVar_15 });
                }

                float InternalVar_16 = 0;

                float InternalVar_17 = InternalParameter_3544.InternalField_3754.InternalProperty_1185 && InternalParameter_3544.InternalProperty_1193 ? InternalParameter_3544.InternalProperty_1209.InternalField_148 : InternalParameter_3544.InternalProperty_1205.InternalField_153;

                bool InternalVar_18 = InternalParameter_3544.InternalField_3754.InternalField_3738.InternalField_3747;

                for (int InternalVar_19 = InternalVar_13; InternalVar_19 != InternalVar_14; InternalVar_19 += InternalVar_12)
                {
                    InternalParameter_3543.InternalProperty_353 = InternalParameter_3544.InternalField_3756.InternalField_587[InternalVar_19];

                    bool InternalVar_20 = InternalVar_5 && InternalParameter_3543.InternalProperty_354[InternalVar_1] == InternalType_83.InternalField_281;

                    float InternalVar_21 = InternalParameter_3546;
                    int InternalVar_22 = 1;

                    if (InternalVar_20)
                    {
                        float InternalVar_23 = InternalParameter_3543.InternalProperty_364[InternalVar_1].InternalField_148;

                        InternalVar_21 = InternalVar_18 ? InternalParameter_3544.InternalMethod_3788(InternalVar_23, out InternalVar_22) : InternalVar_23;

                        ref InternalType_786 InternalVar_24 = ref InternalParameter_3547.ElementAt(InternalParameter_3547.Length - 1);
                        InternalVar_24.InternalField_3764 += InternalVar_21;
                        InternalVar_24.InternalField_3761 += InternalVar_22;
                        InternalVar_24.InternalField_3762++;
                    }

                    InternalMethod_3756(InternalParameter_3543, ref InternalParameter_3544, InternalVar_1, InternalVar_21, InternalVar_7, InternalVar_5 ? InternalParameter_3544.InternalField_3758[InternalVar_7] : 0, ref InternalParameter_3544.InternalField_3758);

                    float InternalVar_25 = InternalParameter_3543.InternalProperty_1191[InternalVar_1];

                    InternalVar_25 += InternalParameter_3543.InternalProperty_379.InternalProperty_137[InternalVar_1];

                    float InternalVar_26 = math.select(0, 0.5f * InternalVar_25, InternalVar_4);

                    InternalParameter_3543.InternalProperty_355[InternalVar_1] = InternalVar_3;

                    ref InternalType_53 InternalVar_27 = ref InternalParameter_3543.InternalProperty_361;

                    if (InternalVar_5 && InternalVar_6 + InternalVar_25 > InternalVar_15 && InternalVar_6 != 0)
                    {
                        ref InternalType_786 InternalVar_28 = ref InternalParameter_3547.ElementAt(InternalParameter_3547.Length - 1);
                        InternalVar_28.InternalField_3764 -= InternalVar_20 ? InternalVar_21 : 0;
                        InternalVar_28.InternalField_3761 -= InternalVar_20 ? InternalVar_22 : 0;
                        InternalVar_28.InternalField_3762 -= InternalVar_20 ? 1 : 0;
                        InternalVar_28.InternalField_3763 = InternalVar_6 - InternalVar_17;
                        InternalVar_28.InternalField_3765 = InternalVar_9 ? InternalVar_28.InternalField_3760 - InternalVar_12 : InternalVar_19;
                        InternalVar_28.InternalField_3760 = InternalVar_9 ? InternalVar_19 - InternalVar_12 : InternalVar_28.InternalField_3760;

                        InternalVar_16 = math.max(InternalVar_28.InternalField_3763, InternalVar_16);

                        InternalVar_6 = 0;
                        InternalParameter_3547.Add(new InternalType_786()
                        {
                            InternalField_3760 = InternalVar_19,
                            InternalField_3761 = InternalVar_20 ? InternalVar_22 : 0,
                            InternalField_3762 = InternalVar_20 ? 1 : 0,
                            InternalField_3764 = InternalVar_20 ? InternalVar_21 : 0,
                            InternalField_3766 = InternalVar_1,
                            InternalField_3767 = InternalVar_15,
                        });
                    }

                    InternalVar_27[InternalVar_1] = new InternalType_45() { InternalField_146 = InternalType_59.InternalField_201, InternalField_145 = InternalVar_6 + InternalVar_26 + InternalParameter_3545 };

                    if (InternalVar_10)
                    {
                        InternalVar_27.InternalProperty_115 *= InternalVar_11;

                        InternalParameter_3543.InternalMethod_1787(InternalParameter_3543.InternalProperty_383);
                    }
                    else
                    {
                        InternalParameter_3543.InternalMethod_3750(InternalVar_15, InternalVar_1);
                    }

                    InternalVar_6 += InternalVar_25 + InternalVar_17;
                }

                if (InternalVar_5)
                {
                    ref InternalType_786 InternalVar_19 = ref InternalParameter_3547.ElementAt(InternalParameter_3547.Length - 1);
                    InternalVar_19.InternalField_3763 = InternalVar_6 - InternalVar_17;
                    InternalVar_19.InternalField_3765 = InternalVar_9 ? InternalVar_19.InternalField_3760 - InternalVar_12 : InternalVar_14;
                    InternalVar_19.InternalField_3760 = InternalVar_9 ? InternalVar_14 - InternalVar_12 : InternalVar_19.InternalField_3760;

                    InternalVar_16 = math.max(InternalVar_19.InternalField_3763, InternalVar_16);
                }

                InternalParameter_3548 = InternalVar_5 ? InternalVar_16 : InternalVar_6 - InternalVar_17 - InternalVar_2;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void InternalMethod_3755(InternalType_448.InternalType_449 InternalParameter_3549, ref InternalType_785 InternalParameter_3550, ref NativeList<InternalType_786> InternalParameter_3551, ref NativeList<InternalType_787> InternalParameter_3552, out float InternalParameter_3553)
            {
                InternalMethod_3759(ref InternalParameter_3549, ref InternalParameter_3550, ref InternalParameter_3551, ref InternalParameter_3552, out float InternalVar_1);
                InternalMethod_3760(ref InternalParameter_3549, ref InternalParameter_3550, ref InternalParameter_3551, ref InternalParameter_3552, InternalVar_1, out InternalParameter_3553);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void InternalMethod_3756(InternalType_448.InternalType_449 InternalParameter_3554, ref InternalType_785 InternalParameter_3555, int InternalParameter_3556, float InternalParameter_3557, int InternalParameter_3558, float InternalParameter_3559, ref float3 InternalParameter_3560)
            {
                bool InternalVar_1 = InternalParameter_3556 >= 0 && InternalParameter_3554.InternalProperty_354[InternalParameter_3556] == InternalType_83.InternalField_281;
                bool InternalVar_2 = InternalParameter_3558 >= 0 && InternalParameter_3554.InternalProperty_354[InternalParameter_3558] == InternalType_83.InternalField_281;

                if (!InternalVar_2 && !InternalVar_1)
                {
                    return;
                }

                float3 InternalVar_3 = InternalParameter_3554.InternalProperty_364.InternalProperty_120;

                bool3 InternalVar_4 = InternalType_187.InternalField_548;

                if (InternalVar_1)
                {
                    InternalParameter_3554.InternalProperty_354[InternalParameter_3556] = InternalType_83.InternalField_280;
                    InternalType_45 InternalVar_5 = InternalParameter_3554.InternalProperty_360[InternalParameter_3556];
                    InternalVar_5.InternalField_146 = InternalType_59.InternalField_202;
                    InternalVar_5.InternalField_145 = InternalParameter_3557 / InternalParameter_3560[InternalParameter_3556];
                    InternalParameter_3554.InternalProperty_360[InternalParameter_3556] = InternalVar_5;

                    if (InternalParameter_3555.InternalField_3754.InternalProperty_1185 && InternalParameter_3555.InternalField_3754.InternalField_3738.InternalField_3747)
                    {
                        float3 InternalVar_6 = InternalVar_3;
                        InternalVar_6[InternalParameter_3556] = math.max(InternalVar_3[InternalParameter_3556], InternalParameter_3555.InternalField_3759);
                        InternalParameter_3554.InternalProperty_364.InternalProperty_120 = InternalVar_6;
                    }

                    InternalVar_4[InternalParameter_3556] = true;
                }

                if (InternalVar_2)
                {
                    InternalParameter_3554.InternalProperty_354[InternalParameter_3558] = InternalType_83.InternalField_280;
                    InternalType_45 InternalVar_5 = InternalParameter_3554.InternalProperty_360[InternalParameter_3558];
                    InternalVar_5.InternalField_146 = InternalType_59.InternalField_202;
                    InternalVar_5.InternalField_145 = InternalParameter_3559 / InternalParameter_3560[InternalParameter_3558];
                    InternalParameter_3554.InternalProperty_360[InternalParameter_3558] = InternalVar_5;

                    InternalVar_4[InternalParameter_3558] = true;
                }

                InternalParameter_3554.InternalMethod_1788(InternalParameter_3560, InternalParameter_3554.InternalProperty_357.InternalProperty_173 ? true : InternalVar_4);

                if (InternalVar_1)
                {
                    InternalParameter_3554.InternalProperty_354[InternalParameter_3556] = InternalType_83.InternalField_281;

                    InternalParameter_3554.InternalProperty_364.InternalProperty_120 = InternalVar_3;
                }

                if (InternalVar_2)
                {
                    InternalParameter_3554.InternalProperty_354[InternalParameter_3558] = InternalType_83.InternalField_281;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float InternalMethod_3757(InternalType_448.InternalType_449 InternalParameter_3561, ref InternalType_785 InternalParameter_3562, ref InternalType_786 InternalParameter_3563, out float InternalParameter_3564)
            {
                int InternalVar_1 = InternalParameter_3563.InternalField_3760;
                float InternalVar_2 = InternalParameter_3563.InternalField_3763;
                float InternalVar_3 = InternalParameter_3563.InternalField_3764;
                float InternalVar_4 = InternalParameter_3563.InternalField_3761;
                int InternalVar_5 = InternalParameter_3563.InternalField_3765;
                int InternalVar_6 = InternalParameter_3563.InternalField_3766;
                float InternalVar_7 = InternalParameter_3563.InternalField_3767;

                float InternalVar_8 = InternalVar_7 - InternalVar_2;

                InternalParameter_3564 = InternalVar_8;

                if (InternalVar_4 == 0 || InternalVar_8 <= 0)
                {
                    return 0;
                }

                InternalParameter_3564 = 0;

                int InternalVar_9 = 0;
                float InternalVar_10 = 0;

                float InternalVar_11 = 0;
                float InternalVar_12 = InternalVar_8 / InternalVar_4;
                float2 InternalVar_13 = InternalVar_12;

                int InternalVar_14 = InternalVar_5 > InternalVar_1 ? 1 : -1;

                for (int InternalVar_15 = 0; InternalVar_15 < InternalField_3752; ++InternalVar_15)
                {
                    float InternalVar_16 = InternalVar_10 - InternalVar_3 - InternalVar_8;

                    InternalVar_11 = 0;
                    InternalVar_10 = 0;

                    InternalVar_9 = InternalVar_15 + 1;

                    float InternalVar_17 = 0;

                    float InternalVar_18 = float.MaxValue;
                    float InternalVar_19 = float.MinValue;

                    for (int InternalVar_20 = InternalVar_1; InternalVar_20 != InternalVar_5; InternalVar_20 += InternalVar_14)
                    {
                        InternalParameter_3561.InternalProperty_353 = InternalParameter_3562.InternalField_3756.InternalField_587[InternalVar_20];

                        if (InternalParameter_3561.InternalProperty_354[InternalVar_6] != InternalType_83.InternalField_281)
                        {
                            continue;
                        }

                        InternalType_45.InternalType_46 InternalVar_21 = InternalParameter_3561.InternalProperty_364[InternalVar_6];

                        float InternalVar_22 = 1;

                        if (InternalVar_12 < InternalVar_21.InternalField_148)
                        {
                            InternalVar_18 = math.min(InternalVar_18, InternalVar_21.InternalField_148);
                        }
                        else if (InternalVar_12 > InternalVar_21.InternalField_149)
                        {
                            InternalVar_19 = math.max(InternalVar_19, InternalVar_21.InternalField_149);
                        }

                        float InternalVar_23 = InternalVar_21.InternalMethod_349(InternalVar_12);

                        if (InternalType_187.InternalMethod_923(InternalVar_23, InternalVar_12, InternalField_3753))
                        {
                            InternalVar_11 += InternalVar_22;
                        }
                        else if (InternalType_187.InternalMethod_922(InternalVar_23, InternalVar_21.InternalField_149))
                        {
                            InternalVar_17 += InternalVar_22;
                        }

                        InternalVar_10 += InternalVar_23;
                    }

                    float InternalVar_24 = InternalVar_10 - InternalVar_3 - InternalVar_8;

                    if (InternalVar_24 <= 0 && InternalVar_17 == InternalVar_4)
                    {
                        InternalParameter_3564 = -InternalVar_24;
                        break;
                    }

                    float InternalVar_25 = InternalVar_11 == 0 ? InternalVar_4 : InternalVar_11;

                    InternalVar_13[0] = InternalVar_13[1];
                    InternalVar_13[1] = InternalVar_12;

                    if (InternalType_187.InternalMethod_922(InternalVar_16, InternalVar_24))
                    {

                        InternalVar_12 = InternalVar_24 < 0 ? InternalVar_18 - (InternalVar_24 / InternalVar_25) : InternalVar_19 - (InternalVar_24 / InternalVar_25);
                    }
                    else
                    {
                        InternalVar_12 -= InternalVar_24 / InternalVar_25;
                    }

                    if (InternalType_187.InternalMethod_923(InternalVar_12, InternalVar_13[0], InternalField_3753))
                    {
                        InternalVar_12 = (InternalVar_12 + InternalVar_13[1]) * 0.5f;
                    }

                    InternalVar_12 = math.max(InternalVar_12, 0);


                    if (InternalType_187.InternalMethod_914(InternalVar_24, InternalField_3753))
                    {
                        break;
                    }
                }

                return InternalVar_12;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static float InternalMethod_3758(ref NativeList<InternalType_787> InternalParameter_3565, int InternalParameter_3566, float InternalParameter_3567, float InternalParameter_3568, float InternalParameter_3569, out float InternalParameter_3570)
            {
                int InternalVar_1 = InternalParameter_3566;

                float InternalVar_2 = InternalParameter_3569 - InternalParameter_3567;

                InternalParameter_3570 = InternalVar_2;

                if (InternalVar_1 == 0 || InternalVar_2 <= 0)
                {
                    return 0;
                }

                int InternalVar_3 = 0;
                float InternalVar_4 = 0;
                InternalParameter_3570 = 0;

                int InternalVar_5 = 0;
                float InternalVar_6 = InternalVar_2 / InternalVar_1;
                float2 InternalVar_7 = InternalVar_6;

                int InternalVar_8 = 0;
                int InternalVar_9 = InternalParameter_3565.Length;

                for (int InternalVar_10 = 0; InternalVar_10 < InternalField_3752; ++InternalVar_10)
                {
                    float InternalVar_11 = InternalVar_4 - InternalParameter_3568 - InternalVar_2;

                    InternalVar_5 = 0;
                    InternalVar_4 = 0;

                    InternalVar_3 = InternalVar_10 + 1;

                    int InternalVar_12 = 0;

                    float InternalVar_13 = float.MaxValue;
                    float InternalVar_14 = float.MinValue;

                    for (int InternalVar_15 = InternalVar_8; InternalVar_15 != InternalVar_9; ++InternalVar_15)
                    {
                        InternalType_787 InternalVar_16 = InternalParameter_3565[InternalVar_15];

                        if (!InternalVar_16.InternalField_3770)
                        {
                            continue;
                        }

                        InternalType_45.InternalType_46 InternalVar_17 = InternalVar_16.InternalField_3768;

                        if (InternalVar_6 < InternalVar_17.InternalField_148)
                        {
                            InternalVar_13 = math.min(InternalVar_13, InternalVar_17.InternalField_148);
                        }
                        else if (InternalVar_6 > InternalVar_17.InternalField_149)
                        {
                            InternalVar_14 = math.max(InternalVar_14, InternalVar_17.InternalField_149);
                        }

                        float InternalVar_18 = InternalVar_17.InternalMethod_349(InternalVar_6);

                        if (InternalType_187.InternalMethod_923(InternalVar_18, InternalVar_6, InternalField_3753))
                        {
                            InternalVar_5++;
                        }
                        else if (InternalType_187.InternalMethod_922(InternalVar_18, InternalVar_17.InternalField_149))
                        {
                            InternalVar_12++;
                        }

                        InternalVar_4 += InternalVar_18;
                    }

                    float InternalVar_19 = InternalVar_4 - InternalParameter_3568 - InternalVar_2;

                    if (InternalVar_19 <= 0 && InternalVar_12 == InternalVar_1)
                    {
                        InternalParameter_3570 = -InternalVar_19;
                        break;
                    }

                    int InternalVar_20 = InternalVar_5 == 0 ? InternalVar_1 : InternalVar_5;

                    InternalVar_7[0] = InternalVar_7[1];
                    InternalVar_7[1] = InternalVar_6;

                    if (InternalType_187.InternalMethod_922(InternalVar_11, InternalVar_19))
                    {

                        InternalVar_6 = InternalVar_19 < 0 ? InternalVar_13 - (InternalVar_19 / InternalVar_20) : InternalVar_14 - (InternalVar_19 / InternalVar_20);
                    }
                    else
                    {
                        InternalVar_6 -= InternalVar_19 / InternalVar_20;
                    }

                    if (InternalType_187.InternalMethod_923(InternalVar_6, InternalVar_7[0], InternalField_3753))
                    {
                        InternalVar_6 = (InternalVar_6 + InternalVar_7[1]) * 0.5f;
                    }

                    InternalVar_6 = math.max(InternalVar_6, 0);


                    if (InternalType_187.InternalMethod_914(InternalVar_19, InternalField_3753))
                    {
                        break;
                    }
                }

                return InternalVar_6;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InternalMethod_3759(ref InternalType_448.InternalType_449 InternalParameter_3571, ref InternalType_785 InternalParameter_3572, ref NativeList<InternalType_786> InternalParameter_3573, ref NativeList<InternalType_787> InternalParameter_3574, out float InternalParameter_3575)
            {
                int InternalVar_1 = InternalParameter_3573.Length;
                int InternalVar_2 = InternalParameter_3572.InternalProperty_1196.InternalMethod_508();

                float InternalVar_3 = 0;
                float InternalVar_4 = 0;
                int InternalVar_5 = 0;

                InternalParameter_3574.Clear();

                float InternalVar_6 = InternalParameter_3572.InternalProperty_1194 ? InternalParameter_3572.InternalProperty_1210.InternalField_148 : InternalParameter_3572.InternalProperty_1206.InternalField_153;

                int InternalVar_7 = InternalParameter_3573[0].InternalField_3760 <= InternalParameter_3573[0].InternalField_3765 ? 1 : -1;

                bool InternalVar_8 = false;

                for (int InternalVar_9 = 0; InternalVar_9 < InternalVar_1; ++InternalVar_9)
                {
                    ref InternalType_786 InternalVar_10 = ref InternalParameter_3573.ElementAt(InternalVar_9);

                    float InternalVar_11 = 0;
                    bool InternalVar_12 = false;
                    bool InternalVar_13 = false;

                    InternalType_45.InternalType_46 InternalVar_14 = new InternalType_45.InternalType_46();

                    for (int InternalVar_15 = InternalVar_10.InternalField_3760; InternalVar_15 != InternalVar_10.InternalField_3765; InternalVar_15 += InternalVar_7)
                    {
                        InternalParameter_3571.InternalProperty_353 = InternalParameter_3572.InternalField_3756.InternalField_587[InternalVar_15];

                        bool InternalVar_16 = InternalParameter_3571.InternalProperty_354[InternalVar_2] == InternalType_83.InternalField_281;

                        InternalVar_12 |= InternalVar_16;

                        InternalVar_13 |= InternalVar_16 && InternalParameter_3571.InternalProperty_357.InternalField_309.InternalMethod_508() == InternalVar_2;

                        InternalType_45.InternalType_46 InternalVar_17 = InternalVar_16 ? InternalParameter_3571.InternalProperty_364[InternalVar_2] : (InternalType_45.InternalType_46)InternalParameter_3571.InternalProperty_1191[InternalVar_2];

                        InternalVar_14.InternalField_148 = math.max(InternalVar_14.InternalField_148, InternalVar_17.InternalField_148);
                        InternalVar_14.InternalField_149 = math.max(InternalVar_14.InternalField_149, InternalVar_17.InternalField_149);
                        InternalVar_11 = math.max(InternalVar_11, math.max(InternalVar_17.InternalField_148, 0) + InternalParameter_3571.InternalProperty_379.InternalProperty_137[InternalVar_2]);
                    }

                    if (InternalVar_12)
                    {
                        InternalVar_5++;
                        InternalVar_3 += InternalVar_14.InternalField_148;
                        InternalVar_8 |= InternalVar_13;
                    }

                    InternalVar_4 += InternalVar_11 + InternalVar_6;

                    InternalParameter_3574.Add(new InternalType_787() { InternalField_3768 = InternalVar_14, InternalField_3770 = InternalVar_12, InternalField_3769 = InternalVar_11, InternalField_3771 = InternalVar_13 });
                }

                InternalVar_4 -= InternalVar_6;

                InternalParameter_3575 = InternalMethod_3758(ref InternalParameter_3574, InternalVar_5, InternalVar_4, InternalVar_3, InternalParameter_3572.InternalField_3758[InternalVar_2], out float InternalVar_18);

                if (InternalParameter_3572.InternalProperty_1194)
                {
                    float InternalVar_19 = math.max(InternalParameter_3572.InternalProperty_1210.InternalField_148, 0) * (InternalVar_1 - 1);
                    InternalParameter_3572.InternalProperty_1208 = new InternalType_45() { InternalField_145 = (InternalVar_18 + InternalVar_19) / math.max(InternalVar_1 - 1, 1) };
                    InternalParameter_3572.InternalProperty_1206 = InternalParameter_3572.InternalMethod_3787(InternalParameter_3572.InternalField_3758[InternalVar_2]);
                }

                if (!InternalVar_8)
                {
                    return;
                }


                for (int InternalVar_19 = 0; InternalVar_19 < InternalVar_1; ++InternalVar_19)
                {
                    ref InternalType_787 InternalVar_20 = ref InternalParameter_3574.ElementAt(InternalVar_19);

                    if (!InternalVar_20.InternalField_3771)
                    {
                        continue;
                    }

                    ref InternalType_786 InternalVar_21 = ref InternalParameter_3573.ElementAt(InternalVar_19);

                    for (int InternalVar_22 = InternalVar_21.InternalField_3760; InternalVar_22 != InternalVar_21.InternalField_3765; InternalVar_22 += InternalVar_7)
                    {
                        InternalParameter_3571.InternalProperty_353 = InternalParameter_3572.InternalField_3756.InternalField_587[InternalVar_22];

                        if (InternalParameter_3571.InternalProperty_354[InternalVar_2] != InternalType_83.InternalField_281 ||
                            InternalParameter_3571.InternalProperty_357.InternalField_309.InternalMethod_508() != InternalVar_2)
                        {
                            continue;
                        }

                        InternalVar_21.InternalField_3763 -= InternalParameter_3571.InternalProperty_1191[InternalVar_21.InternalField_3766];
                        InternalMethod_3756(InternalParameter_3571, ref InternalParameter_3572, -1, 0, InternalVar_2, InternalVar_20.InternalField_3768.InternalMethod_349(InternalParameter_3575), ref InternalParameter_3572.InternalField_3758);
                        InternalVar_21.InternalField_3763 += InternalParameter_3571.InternalProperty_1191[InternalVar_21.InternalField_3766];
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InternalMethod_3760(ref InternalType_448.InternalType_449 InternalParameter_3576, ref InternalType_785 InternalParameter_3577, ref NativeList<InternalType_786> InternalParameter_3578, ref NativeList<InternalType_787> InternalParameter_3579, float InternalParameter_3580, out float InternalParameter_3581)
            {
                int InternalVar_1 = InternalParameter_3577.InternalProperty_1195.InternalMethod_508();
                int InternalVar_2 = InternalParameter_3577.InternalProperty_1196.InternalMethod_508();
                float InternalVar_3 = InternalParameter_3577.InternalField_3754.InternalField_231;
                float InternalVar_4 = InternalParameter_3577.InternalProperty_1198;

                int InternalVar_5 = InternalParameter_3578.Length;

                bool InternalVar_6 = InternalParameter_3577.InternalField_3754.InternalField_3738.InternalField_3747;
                float InternalVar_7 = 0;

                if (InternalVar_6)
                {
                    InternalVar_7 = InternalMethod_3761(ref InternalParameter_3577, ref InternalParameter_3578);

                    if (InternalVar_5 > 1)
                    {
                        InternalMethod_3762(ref InternalParameter_3576, ref InternalParameter_3577, ref InternalParameter_3578, InternalVar_7);
                    }
                }

                float2 InternalVar_8 = new float2(0, InternalVar_3);

                int InternalVar_9 = math.select(1, -1, InternalParameter_3577.InternalProperty_1204);
                int InternalVar_10 = math.select(0, InternalVar_5 - 1, InternalParameter_3577.InternalProperty_1204);
                int InternalVar_11 = math.select(InternalVar_5, -1, InternalParameter_3577.InternalProperty_1204);

                bool InternalVar_12 = InternalParameter_3577.InternalProperty_1199;
                bool InternalVar_13 = InternalParameter_3577.InternalProperty_1200;

                for (int InternalVar_14 = InternalVar_10; InternalVar_14 != InternalVar_11; InternalVar_14 += InternalVar_9)
                {
                    ref InternalType_786 InternalVar_15 = ref InternalParameter_3578.ElementAt(InternalVar_14);

                    float InternalVar_16 = 0;
                    float InternalVar_17 = 0;

                    if (InternalVar_6 && InternalVar_5 > 1)
                    {
                        float InternalVar_18 = InternalParameter_3577.InternalProperty_1193 ? (InternalParameter_3577.InternalProperty_1205.InternalField_153 - InternalParameter_3577.InternalProperty_1209.InternalField_148) * (InternalVar_15.InternalProperty_1211 - 1) : 0;
                        float InternalVar_19 = (InternalVar_15.InternalField_3761 * (InternalVar_7 + InternalParameter_3577.InternalProperty_1205.InternalField_153)) - (InternalVar_15.InternalField_3762 * InternalParameter_3577.InternalProperty_1205.InternalField_153);
                        InternalVar_16 = InternalVar_15.InternalField_3767 - (InternalVar_15.InternalField_3763 - InternalVar_15.InternalField_3764 + InternalVar_19) - InternalVar_18;
                    }
                    else
                    {
                        InternalVar_17 = InternalMethod_3757(InternalParameter_3576, ref InternalParameter_3577, ref InternalVar_15, out InternalVar_16);
                        InternalParameter_3577.InternalMethod_3790(ref InternalVar_15, ref InternalVar_16);
                    }

                    float InternalVar_20 = InternalParameter_3577.InternalProperty_1199 ? -0.5f * (InternalVar_15.InternalField_3767 - InternalVar_16) : 0;

                    float InternalVar_21 = 0;

                    int InternalVar_22 = InternalVar_15.InternalField_3760 <= InternalVar_15.InternalField_3765 ? 1 : -1;

                    ref InternalType_787 InternalVar_23 = ref InternalParameter_3579.ElementAt(InternalVar_14);

                    for (int InternalVar_24 = InternalVar_15.InternalField_3760; InternalVar_24 != InternalVar_15.InternalField_3765; InternalVar_24 += InternalVar_22)
                    {
                        InternalParameter_3576.InternalProperty_353 = InternalParameter_3577.InternalField_3756.InternalField_587[InternalVar_24];

                        float InternalVar_25 = InternalVar_6 ? InternalParameter_3577.InternalMethod_3789(InternalParameter_3576.InternalProperty_364[InternalVar_1].InternalField_148, InternalVar_7, out _) : InternalVar_17;

                        InternalMethod_3756(InternalParameter_3576, ref InternalParameter_3577, InternalVar_1, InternalVar_25, InternalVar_2, InternalVar_23.InternalField_3768.InternalMethod_349(InternalParameter_3580), ref InternalParameter_3577.InternalField_3758);

                        float3 InternalVar_26 = InternalParameter_3576.InternalProperty_1191;
                        InternalVar_26 += InternalParameter_3576.InternalProperty_379.InternalProperty_137;

                        float InternalVar_27 = InternalVar_12 ? 0.5f * InternalVar_26[InternalVar_1] : 0;

                        InternalVar_21 = math.max(InternalVar_26[InternalVar_2], InternalVar_21);

                        InternalParameter_3576.InternalProperty_355[InternalVar_2] = InternalVar_4;

                        ref InternalType_53 InternalVar_28 = ref InternalParameter_3576.InternalProperty_361;

                        float InternalVar_29 = InternalVar_8[0] + InternalVar_27 + InternalVar_20;

                        InternalVar_28[InternalVar_1] = new InternalType_45() { InternalField_146 = InternalType_59.InternalField_201, InternalField_145 = InternalVar_29 };

                        InternalParameter_3576.InternalMethod_3750(InternalParameter_3577.InternalField_3758[InternalVar_1], InternalVar_1);

                        InternalVar_28[InternalVar_2] = new InternalType_45() { InternalField_146 = InternalType_59.InternalField_201, InternalField_145 = InternalVar_8[1] };

                        if (!InternalVar_13)
                        {
                            InternalParameter_3576.InternalMethod_3750(InternalParameter_3577.InternalField_3758[InternalVar_2], InternalVar_2);
                        }

                        InternalVar_8[0] += InternalVar_26[InternalVar_1] + InternalParameter_3577.InternalProperty_1205.InternalField_153;
                    }

                    InternalVar_23.InternalField_3769 = InternalVar_21;

                    InternalVar_8[1] += InternalVar_21 + InternalParameter_3577.InternalProperty_1206.InternalField_153;
                    InternalVar_8[0] = 0;
                }

                InternalParameter_3581 = InternalVar_8[1] - InternalParameter_3577.InternalProperty_1206.InternalField_153 - InternalVar_3;

                if (!InternalVar_13)
                {
                    return;
                }

                float InternalVar_30 = -0.5f * InternalParameter_3581;

                for (int InternalVar_31 = InternalVar_10; InternalVar_31 != InternalVar_11; InternalVar_31 += InternalVar_9)
                {
                    ref InternalType_786 InternalVar_32 = ref InternalParameter_3578.ElementAt(InternalVar_31);

                    int InternalVar_33 = InternalVar_32.InternalField_3760 <= InternalVar_32.InternalField_3765 ? 1 : -1;

                    float InternalVar_34 = 0.5f * InternalParameter_3579[InternalVar_31].InternalField_3769;

                    for (int InternalVar_35 = InternalVar_32.InternalField_3760; InternalVar_35 != InternalVar_32.InternalField_3765; InternalVar_35 += InternalVar_33)
                    {
                        InternalParameter_3576.InternalProperty_353 = InternalParameter_3577.InternalField_3756.InternalField_587[InternalVar_35];

                        ref InternalType_53 InternalVar_36 = ref InternalParameter_3576.InternalProperty_361;

                        InternalType_45 InternalVar_37 = InternalVar_36[InternalVar_2];
                        InternalVar_37.InternalField_145 += InternalVar_30 + InternalVar_34;
                        InternalVar_36[InternalVar_2] = InternalVar_37;

                        InternalParameter_3576.InternalMethod_3750(InternalParameter_3577.InternalField_3758[InternalVar_2], InternalVar_2);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static float InternalMethod_3761(ref InternalType_785 InternalParameter_3582, ref NativeList<InternalType_786> InternalParameter_3583)
            {
                float InternalVar_1 = float.MaxValue;
                int InternalVar_2 = InternalParameter_3582.InternalProperty_1195.InternalMethod_508();
                int InternalVar_3 = InternalParameter_3583.Length;

                float InternalVar_4 = InternalParameter_3582.InternalProperty_1193 ? InternalParameter_3582.InternalProperty_1209.InternalField_148 : InternalParameter_3582.InternalProperty_1205.InternalField_153;

                for (int InternalVar_5 = 0; InternalVar_5 < InternalVar_3; ++InternalVar_5)
                {
                    ref InternalType_786 InternalVar_6 = ref InternalParameter_3583.ElementAt(InternalVar_5);

                    if (InternalVar_6.InternalField_3761 == 0)
                    {
                        continue;
                    }

                    float InternalVar_7 = InternalVar_6.InternalMethod_3792(InternalParameter_3582.InternalField_3758[InternalVar_2], InternalVar_4);

                    InternalVar_1 = math.min(InternalVar_1, InternalVar_7);

                    if (InternalVar_1 < InternalParameter_3582.InternalField_3759)
                    {
                        break;
                    }
                }

                return math.max(InternalParameter_3582.InternalField_3759, InternalVar_1);
            }

            private static void InternalMethod_3762(ref InternalType_448.InternalType_449 InternalParameter_3584, ref InternalType_785 InternalParameter_3585, ref NativeList<InternalType_786> InternalParameter_3586, float InternalParameter_3587)
            {
                if (!InternalParameter_3585.InternalProperty_1193)
                {
                    return;
                }

                int InternalVar_1 = InternalParameter_3586.Length;
                int InternalVar_2 = InternalParameter_3586[0].InternalField_3760 < InternalParameter_3586[0].InternalField_3765 ? 1 : -1;
                float InternalVar_3 = float.MaxValue;
                bool InternalVar_4 = true;

                for (int InternalVar_5 = 0; InternalVar_5 < InternalVar_1; ++InternalVar_5)
                {
                    ref InternalType_786 InternalVar_6 = ref InternalParameter_3586.ElementAt(InternalVar_5);

                    InternalVar_4 &= InternalVar_6.InternalProperty_1211 == 1;

                    float InternalVar_7 = 0;

                    for (int InternalVar_8 = InternalVar_6.InternalField_3760; InternalVar_8 != InternalVar_6.InternalField_3765; InternalVar_8 += InternalVar_2)
                    {
                        InternalParameter_3584.InternalProperty_353 = InternalParameter_3585.InternalField_3756.InternalField_587[InternalVar_8];

                        float InternalVar_9 = InternalParameter_3584.InternalProperty_376[InternalVar_6.InternalField_3766].InternalField_153;

                        if (InternalParameter_3584.InternalProperty_354[InternalVar_6.InternalField_3766] == InternalType_83.InternalField_281)
                        {
                            InternalType_45.InternalType_46 InternalVar_10 = InternalParameter_3584.InternalProperty_364[InternalVar_6.InternalField_3766];
                            InternalVar_9 = InternalVar_10.InternalMethod_349(InternalParameter_3585.InternalMethod_3789(InternalVar_10.InternalField_148, InternalParameter_3587, out _));
                        }

                        InternalVar_9 += InternalParameter_3584.InternalProperty_379.InternalProperty_137[InternalVar_6.InternalField_3766];

                        InternalVar_7 += InternalVar_9;
                    }

                    InternalVar_3 = math.min((InternalVar_6.InternalField_3767 - InternalVar_7) / math.max(InternalVar_6.InternalProperty_1211 - 1, 1), InternalVar_3);
                }

                InternalParameter_3585.InternalProperty_1207 = new InternalType_45() { InternalField_145 = InternalVar_4 ? 0 : InternalVar_3 };
                InternalParameter_3585.InternalProperty_1205 = InternalParameter_3585.InternalMethod_3786(InternalParameter_3586[0].InternalField_3767);
            }
        }

        internal struct InternalType_785
        {
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_70 InternalField_3754;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool3 InternalField_3755;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_222 InternalField_3756;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_48.InternalType_50 InternalField_3757;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public float3 InternalField_3758;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public float InternalField_3759;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            private bool InternalProperty_1192 => InternalField_3754.InternalField_3738.InternalProperty_1188 && InternalField_3754.InternalField_3738.InternalField_3741 != InternalField_3754.InternalField_225;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalProperty_1193 => InternalProperty_1192 ? InternalField_3754.InternalField_3738.InternalField_3744 : InternalField_3754.InternalField_228;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalProperty_1194 => InternalProperty_1192 ? InternalField_3754.InternalField_228 : false;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_96 InternalProperty_1195 => InternalProperty_1192 ? InternalField_3754.InternalField_3738.InternalField_3741 : InternalField_3754.InternalField_225;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_96 InternalProperty_1196 => InternalProperty_1192 ? InternalField_3754.InternalField_225 : InternalType_96.InternalField_304;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public int InternalProperty_1197 => InternalProperty_1192 ? InternalField_3754.InternalField_3738.InternalField_3746 : InternalField_3754.InternalField_230;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public int InternalProperty_1198 => InternalField_3754.InternalField_230;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalProperty_1199 => InternalProperty_1197 == 0;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalProperty_1200 => InternalProperty_1198 == 0;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalProperty_1201 => InternalProperty_1192 ? InternalField_3754.InternalField_3738.InternalField_3745 : InternalField_3754.InternalField_229;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalProperty_1202 => InternalProperty_1192 ? InternalField_3754.InternalField_229 : false;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalProperty_1203 => (InternalProperty_1195 == InternalType_96.InternalField_306) ^ (InternalProperty_1197 == 1) ^ InternalProperty_1201;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalProperty_1204 => (InternalProperty_1196 == InternalType_96.InternalField_306) ^ (InternalProperty_1198 == 1) ^ InternalProperty_1202;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_45.InternalType_47 InternalProperty_1205
            {
                get
                {
                    return InternalProperty_1192 ? InternalField_3757.InternalField_3740 : InternalField_3757.InternalField_3739;
                }
                set
                {
                    if (InternalProperty_1192)
                    {
                        InternalField_3757.InternalField_3740 = value;
                    }
                    else
                    {
                        InternalField_3757.InternalField_3739 = value;
                    }
                }
            }

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_45.InternalType_47 InternalProperty_1206
            {
                get
                {
                    return InternalField_3754.InternalProperty_1185 ? InternalField_3757.InternalField_3739 : default;
                }
                set
                {
                    if (InternalProperty_1192)
                    {
                        InternalField_3757.InternalField_3739 = value;
                    }
                }
            }

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_45 InternalProperty_1207
            {
                get
                {
                    return InternalProperty_1192 ? InternalField_3754.InternalField_3738.InternalField_3742 : InternalField_3754.InternalField_226;
                }
                set
                {
                    if (InternalProperty_1192)
                    {
                        InternalField_3754.InternalField_3738.InternalField_3742 = value;
                    }
                    else
                    {
                        InternalField_3754.InternalField_226 = value;
                    }
                }
            }

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_45 InternalProperty_1208
            {
                get
                {
                    return InternalProperty_1192 ? InternalField_3754.InternalField_226 : default;
                }
                set
                {
                    if (InternalProperty_1192)
                    {
                        InternalField_3754.InternalField_226 = value;
                    }
                }
            }

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_45.InternalType_46 InternalProperty_1209 => InternalProperty_1192 ? InternalField_3754.InternalField_3738.InternalField_3743 : InternalField_3754.InternalField_227;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_45.InternalType_46 InternalProperty_1210 => InternalProperty_1192 ? InternalField_3754.InternalField_227 : default;

            public InternalType_45.InternalType_47 InternalMethod_3786(float InternalParameter_3588) => InternalProperty_1192 ? InternalField_3754.InternalField_3738.InternalMethod_3739(InternalParameter_3588) : InternalField_3754.InternalMethod_451(InternalParameter_3588);
            public InternalType_45.InternalType_47 InternalMethod_3787(float InternalParameter_3589) => InternalProperty_1192 ? InternalField_3754.InternalMethod_451(InternalParameter_3589) : default;

            public float InternalMethod_3788(float InternalParameter_3590, out int InternalParameter_3591) => InternalMethod_3789(InternalParameter_3590, InternalField_3759, out InternalParameter_3591);

            public float InternalMethod_3789(float InternalParameter_3592, float InternalParameter_3593, out int InternalParameter_3594)
            {
                if (!InternalField_3754.InternalProperty_1185 || !InternalField_3754.InternalField_3738.InternalField_3747)
                {
                    InternalParameter_3594 = 0;
                    return InternalParameter_3592;
                }

                if (InternalType_187.InternalMethod_3644(InternalField_3759, 0))
                {
                    InternalParameter_3594 = 1;
                    return InternalParameter_3593;
                }

                float InternalVar_1 = InternalParameter_3593 + InternalProperty_1205.InternalField_153;

                InternalParameter_3594 = (int)math.max(math.ceil(InternalParameter_3592 / InternalField_3759), 1);

                return (InternalParameter_3594 * InternalVar_1) - InternalProperty_1205.InternalField_153;
            }

            public void InternalMethod_3790(ref InternalType_786 InternalParameter_3595, ref float InternalParameter_3596)
            {
                if (!InternalProperty_1193)
                {
                    return;
                }


                float InternalVar_1 = math.max(InternalProperty_1209.InternalField_148, 0);
                float InternalVar_2 = InternalVar_1 * (InternalParameter_3595.InternalProperty_1211 - 1);
                InternalProperty_1207 = new InternalType_45() { InternalField_145 = (InternalParameter_3596 + InternalVar_2) / (InternalParameter_3595.InternalProperty_1211 - 1) };
                InternalProperty_1205 = InternalMethod_3786(InternalParameter_3595.InternalField_3767);
                InternalParameter_3596 -= (InternalProperty_1205.InternalField_153 - InternalVar_1) * (InternalParameter_3595.InternalProperty_1211 - 1);
            }
        }

        internal struct InternalType_786
        {
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public int InternalField_3760;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public int InternalField_3761;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public int InternalField_3762;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public float InternalField_3763;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public float InternalField_3764;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public int InternalField_3765;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public int InternalField_3766;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public float InternalField_3767;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public int InternalProperty_1211 => math.abs(InternalField_3760 - InternalField_3765);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float InternalMethod_3792(float InternalParameter_3597, float InternalParameter_3598)
            {
                float InternalVar_1 = InternalProperty_1211 == 1 ? 0 : (InternalField_3761 - InternalField_3762) * InternalParameter_3598;

                return (InternalParameter_3597 - InternalField_3763 + InternalField_3764 - InternalVar_1) / InternalField_3761;
            }
        }

        internal struct InternalType_787
        {
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public InternalType_45.InternalType_46 InternalField_3768;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public float InternalField_3769;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalField_3770;
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
            public bool InternalField_3771;
        }
    }
}
