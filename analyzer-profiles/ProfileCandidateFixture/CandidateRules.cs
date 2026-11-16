using System;

// CA1050: top-level type without namespace.
public sealed class CandidateRules
{
    // CA2208: invalid argument parameter name.
    public void InvalidArgument(string value)
    {
        if (value is null)
            throw new ArgumentNullException("missingParameter");
    }

    // CA1508: expression is always true after the preceding guard.
    public bool RedundantCondition(object value)
    {
        if (value is null)
            return false;
        return value != null;
    }
}
