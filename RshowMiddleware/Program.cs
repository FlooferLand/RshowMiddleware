const string PATH = "C:/Users/FlooferLand/Desktop/Old_Time_Rock_and_Roll.rshw";

rshwFormat? show = rshwFormat.ReadFromFile(PATH);
if (show != null) {
    foreach (int bit in show.signalData) {
        Console.Write($"{bit} ");
    }
} else {
    Console.WriteLine($"Show file at \"{PATH}\" is null");
}
