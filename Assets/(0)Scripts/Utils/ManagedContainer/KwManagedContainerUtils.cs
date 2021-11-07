using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using System.Runtime.CompilerServices;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
namespace KaizerWaldCode.Utils
{
    public static class KwManagedContainerUtils
    {
        public static NativeArray<T> ToNativeArray<T>(this T[] array, in Allocator a = Allocator.TempJob , in NativeArrayOptions nao = NativeArrayOptions.UninitializedMemory) 
            where T : struct
        {
            NativeArray<T> nA = new NativeArray<T>(array.Length, a, nao);
            nA.CopyFrom(array);
            return nA;
        }
        
        /// <summary>
        /// Convert HashSet To Array
        /// </summary>
        /// <param name="hashSet"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ToArray<T>(this HashSet<T> hashSet)
            where T : unmanaged
        {
            T[] arr = new T[hashSet.Count];
            hashSet.CopyTo(arr);
            return arr;
        }
        
        public static NativeArray<T> ToNativeArray<T>(this HashSet<T> hashSet)
            where T : unmanaged
        {
            T[] arr = new T[hashSet.Count];
            hashSet.CopyTo(arr);
            NativeArray<T> ntvAry = new NativeArray<T>(hashSet.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ntvAry.CopyFrom(arr);
            return ntvAry;
        }
        
        public static T[] RemoveDuplicates<T>(this T[] s) 
            where T : struct
        {
            HashSet<T> set = new HashSet<T>(s);
            T[] result = new T[set.Count];
            set.CopyTo(result);
            return result;
        }
        
        public static bool IsNullOrEmpty<T>(this T[] array)
            where T : struct
        {
            return array == null || array.Length == 0;
        }
    }
}