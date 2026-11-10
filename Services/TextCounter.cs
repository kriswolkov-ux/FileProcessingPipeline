namespace FileProcessingPipeline.Services;

public static class TextCounter
{
    public static (int Lines, int Words, long Characters)
        Count(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int lines = 0;

        if (!string.IsNullOrEmpty(text))
        {
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    lines++;
                }
            }

            if (text[^1] != '\n')
            {
                lines++;
            }
        }

        int words = text
            .Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries)
            .Length;

        long characters = text.Length;

        return (lines, words, characters);
    }
}
