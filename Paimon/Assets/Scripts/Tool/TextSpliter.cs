using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class TextSplitter
{
    private static int Utf8Len(string text)
    {
        return Encoding.UTF8.GetByteCount(text);
    }

    private static IEnumerable<string> BreakText(IEnumerable<string> texts, int length, HashSet<char> splits)
    {
        foreach (var text in texts)
        {
            if (Utf8Len(text) <= length)
            {
                yield return text;
                continue;
            }

            var curr = new StringBuilder();
            foreach (var ch in text)
            {
                curr.Append(ch);
                if (splits.Contains(ch))
                {
                    yield return curr.ToString();
                    curr.Clear();
                }
            }

            if (curr.Length > 0)
            {
                yield return curr.ToString();
            }
        }
    }

    private static IEnumerable<string> BreakTextByLength(IEnumerable<string> texts, int length)
    {
        foreach (var text in texts)
        {
            if (Utf8Len(text) <= length)
            {
                yield return text;
                continue;
            }

            var curr = new StringBuilder();
            foreach (var ch in text)
            {
                curr.Append(ch);
                if (Utf8Len(curr.ToString()) >= length)
                {
                    yield return curr.ToString();
                    curr.Clear();
                }
            }

            if (curr.Length > 0)
            {
                yield return curr.ToString();
            }
        }
    }

    private static void AddCleaned(string curr, List<string> segments)
    {
        curr = curr.Trim();
        if (!string.IsNullOrEmpty(curr) && !curr.All(c => char.IsWhiteSpace(c) || char.IsPunctuation(c)))
        {
            segments.Add(curr);
        }
    }

    private static string ProtectFloat(string text)
    {
        // Turns 3.14 into <3_f_14> to prevent splitting
        return Regex.Replace(text, @"(\d+)\.(\d+)", @"<$1_f_$2>");
    }

    private static string UnprotectFloat(string text)
    {
        // Turns <3_f_14> into 3.14
        return Regex.Replace(text, @"<(\d+)_f_(\d+)>", @"$1.$2");
    }

    public static List<string> SplitText(string text, int length)
    {
        text = TextCleaner.CleanText(text);

        // Break the text into pieces with following rules:
        // 1. Split the text at ".", "!", "?" if text is NOT a float
        // 2. If the text is longer than length, split at ","
        // 3. If the text is still longer than length, split at " "
        // 4. If the text is still longer than length, split at any character to length

        var texts = new List<string> { text };
        texts = texts.Select(ProtectFloat).ToList();
        texts = BreakText(texts, length, new HashSet<char> { '.', '!', '?', '。', '！', '？' })
            .Select(UnprotectFloat)
            .ToList();
        texts = BreakText(texts, length, new HashSet<char> { ',', '，' }).ToList();
        texts = BreakText(texts, length, new HashSet<char> { ' ' }).ToList();
        texts = BreakTextByLength(texts, length).ToList();

        // Then, merge the texts into segments with length <= length
        var segments = new List<string>();
        var curr = new StringBuilder();

        foreach (var part in texts)
        {
            if (Utf8Len(curr.ToString()) + Utf8Len(part) <= length)
            {
                curr.Append(part);
            }
            else
            {
                AddCleaned(curr.ToString(), segments);
                curr.Clear();
                curr.Append(part);
            }
        }

        if (curr.Length > 0)
        {
            AddCleaned(curr.ToString(), segments);
        }

        return segments;
    }

    public static (bool, List<string>) NeedSplitText(string text, int length)
    {
        var segments = SplitText(text, length);
        bool needsSplit = segments.Count > 2 || (segments.Count == 2 && segments[1].Length > 5);
        return (needsSplit, segments);
    }

    public static void Test()
    {
        // Test the SplitText function

        string text = "This is a test sentence. This is another test sentence. And a third one.";

        var result = SplitText(text, 50);
        Debug.Assert(result.SequenceEqual(new List<string>
        {
            "This is a test sentence.",
            "This is another test sentence. And a third one."
        }));

        result = SplitText("a,aaaaaa3.14", 10);
        Debug.Assert(result.SequenceEqual(new List<string> { "a,", "aaaaaa3.14" }));

        result = SplitText("   ", 10);
        Debug.Assert(result.SequenceEqual(new List<string>()));

        result = SplitText("a", 10);
        Debug.Assert(result.SequenceEqual(new List<string> { "a" }));

        text =
            "This is a test sentence with only commas, and no dots, and no exclamation marks, and no question marks, and no newlines.";
        result = SplitText(text, 50);
        Debug.Assert(result.SequenceEqual(new List<string>
        {
            "This is a test sentence with only commas,",
            "and no dots, and no exclamation marks,",
            "and no question marks, and no newlines."
        }));

        text =
            "This is a test sentence This is a test sentence This is a test sentence. This is a test sentence, This is a test sentence, This is a test sentence.";
        result = SplitText(text, 50);
        Debug.Assert(result.SequenceEqual(new List<string>
        {
            "This is a test sentence This is a test sentence",
            "This is a test sentence. This is a test sentence,",
            "This is a test sentence, This is a test sentence."
        }));

        text = "这是一段很长的中文文本,而且没有句号,也没有感叹号,也没有问号,也没有换行符。";
        result = SplitText(text, 50);
        Debug.Assert(result.SequenceEqual(new List<string>
        {
            "这是一段很长的中文文本,",
            "而且没有句号,也没有感叹号,",
            "也没有问号,也没有换行符."
        }));

        Console.WriteLine("All tests passed.");
    }
}
