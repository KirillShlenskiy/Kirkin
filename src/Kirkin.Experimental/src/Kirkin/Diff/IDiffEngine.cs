﻿namespace Kirkin.Diff
{
    internal interface IDiffEngine<T>
    {
        IDiffResult Compare(string name, T x, T y);
    }
}