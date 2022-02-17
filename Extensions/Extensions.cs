﻿global using KiraiMod.Extensions;

using System;
using System.Collections;
using UnityEngine;

namespace KiraiMod.Extensions
{
    public static class Extensions
    {
        public static void Initialize(this Type type) => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

        public static Managers.GUIManager.KiraiSlider GetKiraiSlider(this Transform obj) => new(obj);

        public static void Destroy(this Component component) => UnityEngine.Object.Destroy(component);
        public static void Destroy(this Transform transform) => UnityEngine.Object.Destroy(transform.gameObject);

        public static Coroutine Start(this IEnumerator enumerator) => Utils.Coroutine.Start(enumerator);
        public static void Start(this Coroutine coroutine) => Utils.Coroutine.Stop(coroutine);
    }
}
