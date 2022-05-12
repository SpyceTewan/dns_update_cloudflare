using System;
using System.Collections.Generic;

namespace DnsUpdate;

public class CacheSerializer
{
    private const string Terminator = " ";

    public void Write(List<string> cache, string key, string value)
    {
        if (key.Contains(Terminator) || value.Contains(Terminator))
        {
            throw new ArgumentException($"Cannot store terminator char '{Terminator}' in cache");
        }

        var existingValue = Read(cache, key);
        if (existingValue is not null)
        {
            cache.Remove($"{key}{Terminator}{existingValue}");
        }
        cache.Add($"{key}{Terminator}{value}");
    }

    public string? Read(List<string> cache, string key)
    {
        foreach (var line in cache)
        {
            var splits = line.Split(Terminator);
            if (splits.Length != 2)
            {
                continue;
            }

            if (splits[0] == key)
            {
                return splits[1];
            }
        }

        return null;
    }
}