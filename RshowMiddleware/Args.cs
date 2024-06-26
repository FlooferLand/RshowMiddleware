namespace RshowMiddleware;

public static class Args {
    private static Dictionary<string, Arg> args = new();
    public static void Add(string key, string value) {
        args.Add(key.ToLower(), new Arg(value.ToLower()));
    }
        
    public static Arg? Get(string key) {
        return args.ContainsKey(key) ? args[key] : null;
    }
    public static bool GetBool(string key) {
        return args.ContainsKey(key) ? args[key].IsTrue() : false;
    }

    public static bool IsFeatureEnabled(string name) {
        return (GetBool($"{name}") || GetBool($"enable_{name}")) && !GetBool($"disable_{name}");
    }
}

public class Arg {
    private readonly string value;
    public Arg(string value) {
        this.value = value;
    }
    public bool IsTrue() {
        return new[] { "1", "true" }.Contains(value.ToLower());
    }
    public bool IsFalse() {
        return new[] { "0", "false" }.Contains(value.ToLower());
    }
}
