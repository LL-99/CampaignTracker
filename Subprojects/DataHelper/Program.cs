using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CampaignTracker.Model.Creatures;

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36");
httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
httpClient.DefaultRequestHeaders.Referrer = new Uri("https://5e.tools/bestiary.html");

var (sourceBaseUri, index) = await LoadBestiaryIndex(httpClient);
var rawCreatures = new List<JsonElement>();
var rawCreaturesByKey = new Dictionary<CreatureKey, JsonElement>();

foreach (var pageFile in index.Values.Distinct(StringComparer.OrdinalIgnoreCase))
{
    var pageCreatures = await LoadPageData(httpClient, sourceBaseUri, pageFile, rawCreaturesByKey, rawCreatures);

    Console.WriteLine($"Loaded {pageCreatures.Count,4} creatures from {pageFile}");
}

var importedCreatures = rawCreatures
    .Select(monster => ToStaticCreature(monster, rawCreaturesByKey))
    .ToList();
var duplicateCreatureInfo = GetDuplicateCreatureInfo(importedCreatures);
var (mergedCreatures, mergedDuplicateCreatureCount) = MergeIdenticalCreatures(importedCreatures);

foreach (var creature in mergedCreatures)
{
    creature.GUID = CreateStableGuid(creature.Name, creature.Source);
}

var orderedCreatures = mergedCreatures
    .OrderBy(creature => creature.Name, StringComparer.OrdinalIgnoreCase)
    .ThenBy(creature => creature.Source, StringComparer.OrdinalIgnoreCase)
    .ToList();
var keptDuplicateCreatureInfo = GetDuplicateCreatureInfo(orderedCreatures);

var outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(Directory.GetCurrentDirectory(), "output.json");
await File.WriteAllTextAsync(
    outputPath,
    JsonSerializer.Serialize(orderedCreatures, JsonOptions));

Console.WriteLine(
    $"Duplicate creature entries by name/source before exact-match merging: {duplicateCreatureInfo.DuplicateEntryCount} across {duplicateCreatureInfo.DuplicateNameCount} names.");
Console.WriteLine($"Merged {mergedDuplicateCreatureCount} exact duplicate creature entries into shared source lists.");
Console.WriteLine(
    $"Duplicate creature entries kept in output after exact-match merging: {keptDuplicateCreatureInfo.DuplicateEntryCount} across {keptDuplicateCreatureInfo.DuplicateNameCount} names.");
Console.WriteLine($"Wrote {orderedCreatures.Count} creatures to {outputPath}");

static async Task<(Uri SourceBaseUri, Dictionary<string, string> Index)> LoadBestiaryIndex(HttpClient httpClient)
{
    foreach (var sourceBaseUri in BestiaryBaseUris)
    {
        try
        {
            await using var stream = await httpClient.GetStreamAsync(new Uri(sourceBaseUri, "index.json"));
            var index = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream, JsonOptions)
                ?? throw new InvalidOperationException($"Bestiary index at {sourceBaseUri} was empty or invalid.");

            Console.WriteLine($"Using bestiary data source: {sourceBaseUri}");
            return (sourceBaseUri, index);
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"Could not load {sourceBaseUri}index.json: {exception.Message}");
        }
    }

    throw new InvalidOperationException("Could not load the bestiary index from any configured source.");
}

static async Task<List<StaticCreature>> LoadPageData(
    HttpClient httpClient,
    Uri sourceBaseUri,
    string pageFile,
    Dictionary<CreatureKey, JsonElement> knownRawCreatures,
    List<JsonElement> allRawCreatures)
{
    var pageUrl = new Uri(sourceBaseUri, pageFile);
    using var document = await JsonDocument.ParseAsync(await httpClient.GetStreamAsync(pageUrl));

    if (!document.RootElement.TryGetProperty("monster", out var monsters) ||
        monsters.ValueKind != JsonValueKind.Array)
    {
        return [];
    }

    var pageCreatures = new List<JsonElement>();
    foreach (var monster in monsters.EnumerateArray())
    {
        var clone = monster.Clone();
        pageCreatures.Add(clone);
        allRawCreatures.Add(clone);

        if (TryGetString(clone, "name", out var name) &&
            TryGetString(clone, "source", out var source))
        {
            knownRawCreatures[new CreatureKey(name, source)] = clone;
        }
    }

    return pageCreatures
        .Select(monster => ToStaticCreature(monster, knownRawCreatures))
        .ToList();
}

static StaticCreature ToStaticCreature(
    JsonElement monster,
    IReadOnlyDictionary<CreatureKey, JsonElement> knownRawCreatures)
{
    var hp = ResolveProperty(monster, "hp", knownRawCreatures)
        .Match(ParseHpAverage, 0f);
    var resistances = ResolveProperty(monster, "resist", knownRawCreatures)
        .Match(ParseDamageTypes, []);
    var vulnerabilities = ResolveProperty(monster, "vulnerable", knownRawCreatures)
        .Match(ParseDamageTypes, []);

    return new StaticCreature
    {
        Name = TryGetString(monster, "name", out var name) ? name : string.Empty,
        Source = TryGetString(monster, "source", out var source) ? source : null,
        ChallengeRating = ResolveProperty(monster, "cr", knownRawCreatures)
            .Match(ParseChallengeRating, null),
        Stats = new CreatureStats
        {
            HP = hp,
            Resistances = resistances,
            Vulnerabilities = vulnerabilities
        }
    };
}

static DuplicateCreatureInfo GetDuplicateCreatureInfo(IReadOnlyCollection<StaticCreature> creatures)
{
    var duplicateGroups = creatures
        .GroupBy(creature => creature.Name, StringComparer.OrdinalIgnoreCase)
        .Where(group => group
            .Select(creature => creature.Source ?? string.Empty)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Skip(1)
            .Any())
        .ToList();

    return new DuplicateCreatureInfo(
        duplicateGroups.Count,
        duplicateGroups.Sum(group => group.Count() - 1));
}

static (List<StaticCreature> Creatures, int MergedDuplicateCreatureCount) MergeIdenticalCreatures(
    IReadOnlyCollection<StaticCreature> creatures)
{
    var mergedCreatures = new List<StaticCreature>();
    var mergedDuplicateCreatureCount = 0;

    foreach (var group in creatures.GroupBy(CreateExactCreatureKey))
    {
        var groupCreatures = group.ToList();
        var first = groupCreatures[0];
        var mergedSources = groupCreatures
            .SelectMany(creature => SplitSources(creature.Source))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(source => source, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        first.Source = mergedSources.Length == 0
            ? null
            : string.Join(", ", mergedSources);

        mergedCreatures.Add(first);
        mergedDuplicateCreatureCount += groupCreatures.Count - 1;
    }

    return (mergedCreatures, mergedDuplicateCreatureCount);
}

static ExactCreatureKey CreateExactCreatureKey(StaticCreature creature)
{
    return new ExactCreatureKey(
        creature.Name.Trim().ToUpperInvariant(),
        creature.ChallengeRating,
        creature.Stats.HP,
        string.Join(",", creature.Stats.Resistances.OrderBy(type => type).Select(type => type.ToString())),
        string.Join(",", creature.Stats.Vulnerabilities.OrderBy(type => type).Select(type => type.ToString())));
}

static IEnumerable<string> SplitSources(string? source)
{
    return (source ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(splitSource => !string.IsNullOrWhiteSpace(splitSource));
}

static Guid CreateStableGuid(string name, string? source)
{
    var stableKey = $"CampaignTracker.StaticCreature|{name.Trim().ToUpperInvariant()}|{(source ?? string.Empty).Trim().ToUpperInvariant()}";
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(stableKey));
    var guidBytes = hash[..16];

    guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x50);
    guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

    return new Guid(guidBytes);
}

static OptionalJsonElement ResolveProperty(
    JsonElement monster,
    string propertyName,
    IReadOnlyDictionary<CreatureKey, JsonElement> knownRawCreatures,
    HashSet<CreatureKey>? visited = null)
{
    if (monster.TryGetProperty(propertyName, out var directValue) &&
        directValue.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
    {
        if (!IsCopyOnlyValue(directValue))
        {
            return new OptionalJsonElement(directValue);
        }
    }

    if (!monster.TryGetProperty("_copy", out var copy) ||
        !TryGetString(copy, "name", out var baseName) ||
        !TryGetString(copy, "source", out var baseSource))
    {
        return OptionalJsonElement.Empty;
    }

    var key = new CreatureKey(baseName, baseSource);
    visited ??= [];
    if (!visited.Add(key) || !knownRawCreatures.TryGetValue(key, out var baseMonster))
    {
        return OptionalJsonElement.Empty;
    }

    return ResolveProperty(baseMonster, propertyName, knownRawCreatures, visited);
}

static bool IsCopyOnlyValue(JsonElement value)
{
    return value.ValueKind == JsonValueKind.Object &&
        value.EnumerateObject().All(property => property.NameEquals("_copy"));
}

static float ParseHpAverage(JsonElement hp)
{
    return hp.ValueKind switch
    {
        JsonValueKind.Number => hp.GetSingle(),
        JsonValueKind.Object when hp.TryGetProperty("average", out var average) => ParseNumber(average),
        _ => 0f
    };
}

static float? ParseChallengeRating(JsonElement cr)
{
    if (cr.ValueKind == JsonValueKind.Object &&
        cr.TryGetProperty("cr", out var nestedCr))
    {
        return ParseChallengeRating(nestedCr);
    }

    if (TryGetStringValue(cr, out var crText))
    {
        return ParseChallengeRatingText(crText);
    }

    return cr.ValueKind == JsonValueKind.Number && cr.TryGetSingle(out var numericCr)
        ? numericCr
        : null;
}

static float? ParseChallengeRatingText(string crText)
{
    if (string.Equals(crText, "Unknown", StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    if (float.TryParse(crText, NumberStyles.Float, CultureInfo.InvariantCulture, out var wholeCr))
    {
        return wholeCr;
    }

    var fractionParts = crText.Split('/', StringSplitOptions.TrimEntries);
    if (fractionParts.Length == 2 &&
        float.TryParse(fractionParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var numerator) &&
        float.TryParse(fractionParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var denominator) &&
        denominator != 0)
    {
        return numerator / denominator;
    }

    return null;
}

static DamageType[] ParseDamageTypes(JsonElement damageTypes)
{
    var parsedDamageTypes = new HashSet<DamageType>();
    AddDamageTypes(damageTypes, parsedDamageTypes);

    return parsedDamageTypes
        .OrderBy(type => type)
        .ToArray();
}

static void AddDamageTypes(JsonElement value, HashSet<DamageType> damageTypes)
{
    switch (value.ValueKind)
    {
        case JsonValueKind.String:
            AddDamageType(value.GetString(), damageTypes);
            break;
        case JsonValueKind.Array:
            foreach (var item in value.EnumerateArray())
            {
                AddDamageTypes(item, damageTypes);
            }

            break;
        case JsonValueKind.Object:
            foreach (var propertyName in new[] { "resist", "vulnerable", "immune" })
            {
                if (value.TryGetProperty(propertyName, out var nestedDamageTypes))
                {
                    AddDamageTypes(nestedDamageTypes, damageTypes);
                }
            }

            if (IsAllPhysicalDamage(value))
            {
                damageTypes.Add(DamageType.BludgeoningMagic);
                damageTypes.Add(DamageType.PiercingMagic);
                damageTypes.Add(DamageType.SlashingMagic);
            }

            break;
    }
}

static void AddDamageType(string? damageType, HashSet<DamageType> damageTypes)
{
    switch (damageType?.Trim().ToLowerInvariant())
    {
        case "acid":
            damageTypes.Add(DamageType.Acid);
            break;
        case "bludgeoning":
            damageTypes.Add(DamageType.Bludgeoning);
            break;
        case "cold":
            damageTypes.Add(DamageType.Cold);
            break;
        case "fire":
            damageTypes.Add(DamageType.Fire);
            break;
        case "force":
            damageTypes.Add(DamageType.Force);
            break;
        case "lightning":
            damageTypes.Add(DamageType.Lightning);
            break;
        case "necrotic":
            damageTypes.Add(DamageType.Necrotic);
            break;
        case "piercing":
            damageTypes.Add(DamageType.Piercing);
            break;
        case "poison":
            damageTypes.Add(DamageType.Poison);
            break;
        case "psychic":
            damageTypes.Add(DamageType.Psychic);
            break;
        case "radiant":
            damageTypes.Add(DamageType.Radiant);
            break;
        case "slashing":
            damageTypes.Add(DamageType.Slashing);
            break;
        case "thunder":
            damageTypes.Add(DamageType.Thunder);
            break;
    }
}

static bool IsAllPhysicalDamage(JsonElement value)
{
    if (!value.TryGetProperty("note", out var noteElement) ||
        !TryGetStringValue(noteElement, out var note))
    {
        return false;
    }

    var normalizedNote = note.ToLowerInvariant();
    return normalizedNote.Contains("from all") ||
        normalizedNote.Contains("from magical") ||
        normalizedNote.Contains("magical and nonmagical") ||
        normalizedNote.Contains("regardless of whether");
}

static bool TryGetString(JsonElement element, string propertyName, out string value)
{
    if (element.TryGetProperty(propertyName, out var property) &&
        TryGetStringValue(property, out value))
    {
        return true;
    }

    value = string.Empty;
    return false;
}

static bool TryGetStringValue(JsonElement element, out string value)
{
    value = element.ValueKind switch
    {
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Number => element.GetRawText(),
        _ => string.Empty
    };

    return value.Length > 0;
}

static float ParseNumber(JsonElement number)
{
    return number.ValueKind == JsonValueKind.Number && number.TryGetSingle(out var value)
        ? value
        : 0f;
}

internal readonly record struct CreatureKey(string Name, string Source);

internal readonly record struct DuplicateCreatureInfo(int DuplicateNameCount, int DuplicateEntryCount);

internal readonly record struct ExactCreatureKey(
    string Name,
    float? ChallengeRating,
    float HP,
    string Resistances,
    string Vulnerabilities);

internal readonly struct OptionalJsonElement
{
    public static OptionalJsonElement Empty { get; } = new(default, false);

    public OptionalJsonElement(JsonElement value)
        : this(value, true)
    {
    }

    private OptionalJsonElement(JsonElement value, bool hasValue)
    {
        Value = value;
        HasValue = hasValue;
    }

    public JsonElement Value { get; }

    public bool HasValue { get; }

    public T Match<T>(Func<JsonElement, T> whenPresent, T whenMissing)
    {
        return HasValue ? whenPresent(Value) : whenMissing;
    }
}

internal sealed class JsonStringEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Enum.Parse<TEnum>(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

internal static partial class Program
{
    private static readonly Uri[] BestiaryBaseUris =
    [
        new("https://5e.tools/data/bestiary/"),
        new("https://raw.githubusercontent.com/5etools-mirror-3/5etools-src/main/data/bestiary/")
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter<CreatureType>(),
            new JsonStringEnumConverter<DamageType>()
        }
    };
}
