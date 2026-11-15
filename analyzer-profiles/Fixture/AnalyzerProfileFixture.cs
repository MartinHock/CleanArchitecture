public sealed class AnalyzerProfileFixture
{
    public int Value() => 42; // Deliberately triggers CA1822.

    public void LeakStream()
    {
        var stream = new System.IO.StreamReader("unused.txt"); // Deliberately triggers CA2000.
        _ = stream.ReadLine();
    }
}

