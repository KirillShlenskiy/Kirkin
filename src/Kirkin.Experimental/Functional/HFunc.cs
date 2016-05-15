using System;

namespace Kirkin.Functional
{
    /* =========================================================================== *
     * Not declaring HFunc<TIn, TOut> as it would be identical to Func<TIn, TOut>. *
     * =========================================================================== */

    /// <summary>
    /// Haskell-style function delegate equivalent to Func{TIn1, Func{TIn2, TOut}}.
    /// </summary>
    public delegate Func<TIn2, TOut> HFunc<in TIn1, in TIn2, out TOut>(TIn1 input);

    /// <summary>
    /// Haskell-style function delegate equivalent to Func{TIn1, Func{TIn2, Func{TIn3, TOut}}}.
    /// </summary>
    public delegate HFunc<TIn2, TIn3, TOut> HFunc<in TIn1, in TIn2, in TIn3, out TOut>(TIn1 input);

    /// <summary>
    /// Haskell-style function delegate equivalent to Func{TIn1, Func{TIn2, Func{TIn3, Func{TIn4, TOut}}}}.
    /// </summary>
    public delegate HFunc<TIn2, TIn3, TIn4, TOut> HFunc<in TIn1, in TIn2, in TIn3, in TIn4, out TOut>(TIn1 input);
}