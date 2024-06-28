namespace RshowMiddleware;

public static class Args {
    private static Dictionary<string, RegisteredArg> registeredArgs = new();
    public static Dictionary<string, Arg> UserProvidedArgs = new();
    
    /** Adds an argument that was passed into the program */
    public static void AddFromUser(string key, string value) {
        key = key.ToLower();
        UserProvidedArgs.Add(key, new Arg(key, Option<string>.Some(value)));
    }

    #region Registry
    public delegate T RegistryGetter<out T>(string key);
    
    // ReSharper disable RedundantIfElseBlock
    /** Registers an argument; Adds stuff to the help menu, etc. */
    public static Arg RegisterFeature(string key, string description, bool @default) {
        key = key.ToLower();
        string[] keys = { key, $"enable_{key}", $"disable_{key}" };

        foreach (string k in keys) {
            registeredArgs.Add(k, new RegisteredArg(key, Option<string>.Some(@default.ToString()), description, false));
        }

        string? yuh = UserProvidedArgs.ContainsKey(key) ? UserProvidedArgs[key].Get() : null;
        return new Arg(key, Option<string>.Some(yuh), Option<string>.Some(@default.ToString()));
    }
    
    // ReSharper disable RedundantIfElseBlock
    /** Registers an argument; Adds stuff to the help menu, etc. */
    public static Arg Register<T>(string key, string description, RegistryGetter<T> getter, bool hidden = false) {
        return Register(key, description, getter, Option<string>.None(), hidden);
    }

    // ReSharper disable RedundantIfElseBlock
    /** Registers an argument; Adds stuff to the help menu, etc. */
    public static Arg Register<T>(string key, string description, RegistryGetter<T> getter, Option<string> @default, bool hidden = false) {
        key = key.ToLower();
        registeredArgs.Add(key, new RegisteredArg(key, @default, description, hidden));
        
        string? value = Convert.ToString(getter(key));
        if (value == null) {
            if (@default.LetSome(out string some)) {
                return new Arg(key, Option<string>.Some(some), @default);
            } else {
                throw new Exception($"Argument '{key}' must be specified!\nInfo about '{key}': \"{description}\"");
            }
        }
        return new Arg(key, Option<string>.Some(value), @default);;
    }
    
    /** Will tell the developer (me) if I forgot to register an argument */
    public static void CheckRegistered(string key) {
        if (!registeredArgs.ContainsKey(key.ToLower())) {
            throw new Exception($"Argument \"{key}\" was not registered as an argument!\nIt won't show up in the help menu!");
        }
    }
    #endregion
    
    /** The main get method. This should be used EVERYWHERE. */
    public static Arg? GetArg(string key) {
        CheckRegistered(key);
        return UserProvidedArgs.ContainsKey(key) ? UserProvidedArgs[key] : null;
    }
    
    public static string? GetString(string key) {
        return GetArg(key)?.GetString();
    }
    public static bool GetBool(string key) {
        return GetArg(key)?.GetBoolean() ?? false;
    }
    public static bool? GetBoolUnsafe(string key) {
        return GetArg(key)?.GetBoolean();
    }

    public static bool IsFeatureEnabled(string name) {
        return (
            GetBool($"{name}") || GetBool($"enable_{name}")
        ) && !GetBool($"disable_{name}");
    }
}

/** Used user-side */
public class Arg {
    private readonly string key;
    private readonly Option<string> value;
    public readonly Option<string> Default;

    public Arg(string key, Option<string> value)
        : this(key, value, Option<string>.None()) {}

    public Arg(string key, Option<string> value, Option<string> @default) {
        this.key = key;
        this.value = value.MapSome(v => v.Trim());
        Default = @default;
    }

    public override bool Equals(object? obj) {
        if (obj is Arg arg) {
            return key == arg.key;
        }
        return base.Equals(obj);
    }

    public string GetName() {
        return key;
    }
    
    public Option<string> GetOption() {
        if (value.HasValue) {
            return value;
        } else if (Default.HasValue) {
            return Default;
        }
        
        return Option<string>.None();
    }
    
    public string Get() {
        if (value.LetSome(out string val)) {
            return val.Trim();
        } else if (Default.LetSome(out string defaultVal)) {
            return defaultVal.Trim();
        } else {
            throw new Exception($"Neither the value or the default value exist in argument {key}");
        }
    }
    
    public string GetString() {
        return Convert.ToString(Get());
    }

    public bool IsEnabled() {
        return GetBoolean();
    }

    public bool GetBoolean() {
        string val = Get().ToLower();
        
        if (new[] { "1", "true" }.Contains(val)) {
            return true;
        } else if (new[] { "0", "false" }.Contains(val)) {
            return false;
        }
        
        return false;
    }
}

/** Simpler container */
public class RegisteredArg {
    public readonly string Key;
    public readonly Option<string> DefaultValue;
    public readonly string Description;
    public readonly bool Hidden;

    public RegisteredArg(string key, Option<string> defaultValue, string description, bool hidden) {
        Key = key;
        DefaultValue = defaultValue;
        Description = description;
        Hidden = hidden;
    }
}
