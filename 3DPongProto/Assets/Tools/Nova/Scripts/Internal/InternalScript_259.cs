using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Nova.InternalNamespace_0.InternalNamespace_5
{
    internal unsafe static class InternalType_188
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InternalMethod_952<T>(T* InternalParameter_910, T* InternalParameter_911, int InternalParameter_912 = 1) where T : unmanaged
        {
            UnsafeUtility.MemCpy(InternalParameter_910, InternalParameter_911, InternalParameter_912 * sizeof(T));
        }

        public static void InternalMethod_3817<T>(ref NativeArray<T> InternalParameter_3650, ref NativeArray<T> InternalParameter_3651, int InternalParameter_3652) where T : unmanaged
        {
            T* InternalVar_1 = (T*)InternalParameter_3650.GetUnsafePtr();
            T* InternalVar_2 = (T*)InternalParameter_3651.GetUnsafeReadOnlyPtr();
            InternalMethod_952(InternalVar_1, InternalVar_2, InternalParameter_3652);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo InternalMethod_953<TFrom, TTo>(this ref TFrom InternalParameter_913) where TFrom : unmanaged where TTo : unmanaged
        {
            return ref UnsafeUtility.As<TFrom, TTo>(ref InternalParameter_913);
        }
    }
}

