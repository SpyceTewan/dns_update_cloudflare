using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DnsUpdate;

internal class CacheManager
{

    private const string CacheFileName = "cache0";

    private readonly CacheSerializer _serializer;

    public CacheManager()
    {
        _serializer = new CacheSerializer();
    }

    public void Write(string key, string value)
    {
        var content = GetCacheContent();
        _serializer.Write(content, key, value);
        File.WriteAllLines(GetCacheFilePath(), content);
    }

    public string? Read(string key)
    {
        return _serializer.Read(GetCacheContent(), key);
    }

    public void EnsureCacheCreated()
    {
        var cacheDir = GetCacheDirectory();
        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }

        var cacheFile = GetCacheFilePath();
        if (!File.Exists(cacheFile))
        {
            File.CreateText(cacheFile).Close();
        }
    }

    public string GetCacheDirectory()
    {
        if (OperatingSystem.IsLinux())
        {
            return "/var/cache/cloudflare_dns_updater";
        }
        return "./";
    }

    public string GetCacheFilePath()
    {
        return Path.GetFullPath(Path.Join(GetCacheDirectory(), CacheFileName));
    }

    private List<string> GetCacheContent()
    {
        return File.ReadAllLines(GetCacheFilePath()).ToList();
    }
}