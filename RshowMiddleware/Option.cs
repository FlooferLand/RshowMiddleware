using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RshowMiddleware;

// ReSharper disable UnusedMember.Global
public readonly struct Option<T> {
    private readonly bool hasValue;
    private readonly T? value;

    public bool HasValue => hasValue;

    #region Constructors
    private Option(T? value, bool hasValue) {
        this.hasValue = hasValue;
        this.value = value;
    }
    
    public Option() {
        hasValue = false;
        value = default;
    }
    
    public static Option<T> Some(T? value) {
        return value == null
            ? new Option<T>()
            : new Option<T?>(value, true)!;
    }
    
    public static Option<T> None() {
        return new Option<T>();
    }
    #endregion

    #region Getting the value
    public bool LetSome([NotNull] out T val) {
        val = (hasValue ? value : default)!;
        // throw new Exception($"{nameof(LetSome)} returns a bool! Use it to check if the value inside exists")
        return hasValue;
    }
    
    public delegate T MapSomeValue(T val);
    public Option<T> MapSome(MapSomeValue mapper) {
        var val = Some(value);
        return val.LetSome(out var v) ? Some(mapper(v)) : None();
    }
    
    public T Expect(string message) {
        return value ?? throw new Exception(message);
    }

    public T Unwrap() {
        Debug.Assert(value != null, nameof(value) + " != null");
        return value;
    }
    
    public T UnwrapOr(T @default) {
        return value ?? @default;
    }
    #endregion
}
