using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

// This code is very bad i know

DateTime startTime = DateTime.Now;

const int statusChanged = 0;
const int statusFail = 10;
const int statusNoChange = 11;
const string oldIpFileName = "oldip.txt";
const string tokenFileName = "token.txt";
const string zoneFileName = "zone.txt";


string zone = RequireReadFile(zoneFileName);
string bearerToken = RequireReadFile(tokenFileName);

if (string.IsNullOrWhiteSpace(zone) || string.IsNullOrWhiteSpace(bearerToken))
{
    PrintDone(startTime);
    return statusFail;
}
string apiEndpoint = $"https://api.cloudflare.com/client/v4/zones/{zone}/dns_records";


// ============= Get my IP

HttpClient httpClient = new HttpClient();

HttpRequestMessage myIpMessage = new HttpRequestMessage
{
    RequestUri = new Uri("https://diagnostic.opendns.com/myip")
};

var response = await httpClient.SendAsync(myIpMessage);
if (response.StatusCode != HttpStatusCode.OK)
{
    Console.Error.WriteLine("Failed to get public IP. Code {0}", response.StatusCode);
    PrintDone(startTime);
    return statusFail;
}

string myIp = await response.Content.ReadAsStringAsync();
Console.WriteLine("Public IP: {0}", myIp);

// ========== Did it change?

bool changed = false;

if (File.Exists(oldIpFileName))
{
    string oldIp = File.ReadLines(oldIpFileName).LastOrDefault();
    if (oldIp != myIp)
        changed = true;
    
}
else
{
    changed = true;
    File.Create(oldIpFileName).Close();
}

if (!changed)
{
    Console.Error.WriteLine("IP didn't change. Not updating and exiting");
    PrintDone(startTime);
    return statusNoChange;
}
else
{
    File.AppendAllText(oldIpFileName, myIp);
}

// ========== Get existing dns records

HttpRequestMessage listMessage = new HttpRequestMessage
{
    Method = HttpMethod.Get,
    RequestUri = new Uri(apiEndpoint),
    Headers =
    {
        {"Authorization", $"Bearer {bearerToken}"}
    }
};

var listResponse = await httpClient.SendAsync(listMessage);
StreamWriter writer;
switch (listResponse.StatusCode)
{
    case HttpStatusCode.OK:
        Console.Error.WriteLine("Authorized!");
        break;
    case HttpStatusCode.Unauthorized:
        Console.Error.WriteLine("Authorization Error");
        writer = File.AppendText("error.log");
        writer.Write(await listResponse.Content.ReadAsStringAsync());
        writer.Close();
        
        PrintDone(startTime);
        return statusFail;
    default:
        Console.Error.WriteLine("Error fetching dns list");
        writer = File.AppendText("error.log");
        writer.Write(await listResponse.Content.ReadAsStringAsync());
        writer.Close();
        
        PrintDone(startTime);
        return statusFail;
}

var dnsList = await listResponse.Content.ReadFromJsonAsync<DnsListResponse>();
if (dnsList is not null)
{
    foreach (var dnsEntry in dnsList.result)
    {
        Console.WriteLine("------------");
        Console.WriteLine("ID:      {0}", dnsEntry.id);
        Console.WriteLine("Type:    {0}", dnsEntry.type);
        Console.WriteLine("Name:    {0}", dnsEntry.name);
        Console.WriteLine("Content: {0}", dnsEntry.content);
        Console.WriteLine("TLL:     {0}", dnsEntry.ttl);
    }
    
    Console.WriteLine("------------");
    foreach (var dnsEntry in dnsList.result)
    {
        JsonContent content = JsonContent.Create(new UpdateRequest
        {
            type = dnsEntry.type,
            name = dnsEntry.name,
            content = myIp,
            ttl = 120,
            proxied = dnsEntry.proxied
        });
        HttpRequestMessage message = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri($"{apiEndpoint}/{dnsEntry.id}"),
            Headers =
            {
                {"Authorization", $"Bearer {bearerToken}"}
            },
            Content = content
        };
        
        Console.Write("Updating {0} {1} to point to {2} ... ", dnsEntry.type, dnsEntry.name, myIp);
        
        var updateResponse = await httpClient.SendAsync(message);
        
        if (updateResponse.StatusCode == HttpStatusCode.OK)
        {
            Console.Write("OK\n");
            continue;
        }

        Console.Error.Write("Error updating");
        writer = File.AppendText("updateerror.log");
        var err = await updateResponse.Content.ReadAsStringAsync();
        writer.Write(err);
        writer.Close();
    }
}

Console.WriteLine("I'm making a note here: Huge success!");

PrintDone(startTime);

return statusChanged;

static void PrintDone(DateTime startTime)
{
    Console.WriteLine("------------");
    var timeTaken = DateTime.Now.Subtract(startTime);
    Console.WriteLine($"Done in {timeTaken.Seconds}s {timeTaken.Milliseconds}ms ({timeTaken.TotalMilliseconds}ms total)");
}

static string RequireReadFile(string fileName)
{
    string content;
    if (File.Exists(fileName))
    {
        content = File.ReadAllText(fileName).Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            Console.Error.WriteLine($"{fileName} is empty. Please add data, I can't grab it out of thin air!");
            return null;
        }
    }
    else
    {
        File.Create(fileName).Close();
        Console.Error.WriteLine($"No {fileName}. I created one, but you have to add it yourself.");
        return null;
    }

    return content;
}

abstract class Response
{
    public bool success { get; set; }
    public string[] errors { get; set; }
    public string[] messages { get; set; }
    
}
class DnsListResponse : Response
{
    public List<DnsListResult> result { get; set; }
}

class DnsListResult
{
    public Object data { get; set; }
    public Meta meta { get; set; }
    
    public string id { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public string content { get; set; }
    public bool proxiable { get; set; }
    public bool proxied { get; set; }
    public int ttl { get; set; }
    public bool locked { get; set; }
    public string zone_id { get; set; }
    public string zone_name { get; set; }
    public string created_on { get; set; }
    public string modified_on { get; set; }
}

class Meta
{
    public bool auto_added { get; set; }
    public string source { get; set; }
}

class UpdateRequest {
    public string type { get; set; }
    public string name { get; set; }
    public string content { get; set; }
    public bool proxied { get; set; }
    public int ttl { get; set; }
}