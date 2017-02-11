﻿using System;

static class DisposableExtensions
{
    public static void SafeDispose(this IDisposable disposable)
    {
        if (disposable != null)
            disposable.Dispose();
    }
}