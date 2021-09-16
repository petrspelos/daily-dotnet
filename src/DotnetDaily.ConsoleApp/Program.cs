using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotnetDaily;
using Newtonsoft.Json;

var executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

if(executablePath is null)
    throw new ArgumentNullException("Could not find the executing assembly location.");

var seenFilePath = Path.Combine(executablePath, "seen.json");
var personalityFilePath = Path.Combine(executablePath, "personality.json");
var docsPagesFilePath = Path.Combine(executablePath, "docs-pages.json");
var configFilePath = Path.Combine(executablePath, "config.json");

var config = ReadRequiredJson<Configuration>(configFilePath);

var client = new HttpClient();
var rng = new Random();


if(ShouldNotRun())
    return;

var msdnUrl = GetArticleOfTheDay();
var message = ConstructMessageOfTheDay(msdnUrl);
await SendMessageAsync(message);

static bool ShouldNotRun() => DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday;

string GetArticleOfTheDay()
{
    EnsureSeenFileExists();

    var seen = ReadRequiredJson<ICollection<string>>(seenFilePath);
    var pages = ReadRequiredJson<IEnumerable<string>>(docsPagesFilePath).Where(IsNotCompilerErrorPage);
    var pool = pages.Except(seen);

    if(!pool.Any())
    {
        pool = pages;
        seen = new List<string>();
    }

    var selection = pool.Random(rng);

    seen.Add(selection);
    File.WriteAllText(seenFilePath, JsonConvert.SerializeObject(seen));

    return selection;
}

void EnsureSeenFileExists()
{
    if(!File.Exists(seenFilePath))
        File.WriteAllText(seenFilePath, "[]");
}

static T ReadRequiredJson<T>(string filePath)
{
    var result = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));

    if(result is null)
        throw new ArgumentNullException($"Failed to read and parse required JSON file. File path: '{filePath}'.");
    
    return result;
}

static bool IsNotCompilerErrorPage(string p) => !p.ToLower().Contains("/misc/cs") && !p.ToLower().Contains("compiler-messages");

string ConstructMessageOfTheDay(string msdnUrl)
{
    var pers = ReadRequiredJson<Personality>(personalityFilePath);

    var message = $"*{pers.Greetings.Random(rng)} {pers.Nicknames.Random(rng)}!*\n\n It's {DateTime.Today.DayOfWeek}\n\n{pers.PreMessageRemarks.Random(rng)}\n\n :link: {msdnUrl}\n\n{pers.Goodbyes[DateTime.Now.DayOfWeek].Random(rng)}";

    return message;
}

async Task SendMessageAsync(string message)
{
    var json = JsonConvert.SerializeObject(new { text = message });
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    await client.PostAsync(config.SlackWebhookUrl, content);
}
