using System;
using System.Collections.Generic;

public class StringUtils
{
    public static List<string> ParseString(string input)
    {
        return new List<string>(
            input.Split(
                new[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries
            )
        );
    }
}
