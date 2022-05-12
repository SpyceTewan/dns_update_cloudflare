using System.Collections.Generic;
using NUnit.Framework;

namespace DnsUpdate.Test;

[TestFixture]
public class CacheSerializerTests
{
    [Test]
    public void TestCacheWriteThrowsNothingWhenInputCorrect()
    {
        var cache = new CacheSerializer();
        Assert.That(() => cache.Write(new List<string>(),"ip", "102.34.5.20"), Throws.Nothing);
    }
    
    [Test]
    public void TestCacheWriteThrowsArgumentWhenInputContainsTerminator()
    {
        var cache = new CacheSerializer();
        Assert.That(() => cache.Write(new List<string>(), "ip", "102.34. 5.20"), Throws.ArgumentException);
    }

    [Test]
    public void TestCacheWriteAndRead()
    {
        List<string> cacheContent = new List<string>();
        var cache = new CacheSerializer();
        cache.Write(cacheContent, "hello", "world");

        var value = cache.Read(cacheContent, "hello");
        Assert.That(value, Is.Not.Null.And.EqualTo("world"));
    }

    [Test]
    public void TestCacheWriteSameKeyUpdatesOldKey()
    {
        List<string> cacheContent = new List<string>();
        var cache = new CacheSerializer();
        cache.Write(cacheContent, "hello", "world");
        cache.Write(cacheContent, "hello", "universe");

        var value = cache.Read(cacheContent, "hello");
        Assert.That(value, Is.Not.Null.And.EqualTo("universe"));
    }
}