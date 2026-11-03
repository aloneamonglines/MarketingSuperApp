using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using MKS.Shared.Domain.Shared.Attributes;

namespace MKS.Shared.Infrastructure.Localization
{
    public sealed class JsonStringLocalizer<TResource> : IStringLocalizer<TResource>
    {
        private readonly Assembly _resourceAssembly;
        private readonly string _resourceNamePrefix;
        private readonly string _defaultCultureName;
        private readonly HashSet<string> _resourceNames;
        private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _translations = new(StringComparer.OrdinalIgnoreCase);

        public JsonStringLocalizer(string defaultCultureName = "en")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(defaultCultureName);

            var resourceType = typeof(TResource);
            var resourceAttribute = resourceType.GetCustomAttribute<LocalizationResourceAttribute>()
                ?? throw new ArgumentException(
                    $"Type '{resourceType.FullName}' must have a {nameof(LocalizationResourceAttribute)}.",
                    nameof(resourceType));

            if (string.IsNullOrWhiteSpace(resourceAttribute.ResourceName))
            {
                throw new ArgumentException(
                    $"The {nameof(LocalizationResourceAttribute)} on '{resourceType.FullName}' must have a resource name.",
                    nameof(resourceType));
            }

            if (string.IsNullOrWhiteSpace(resourceType.Namespace))
            {
                throw new ArgumentException("The localization resource type must have a namespace.", nameof(resourceType));
            }

            _defaultCultureName = CultureInfo.GetCultureInfo(defaultCultureName).Name;
            _resourceAssembly = resourceType.Assembly;
            _resourceNamePrefix = $"{resourceType.Namespace}.Resources.{resourceAttribute.ResourceName}.";
            _resourceNames = _resourceAssembly.GetManifestResourceNames().ToHashSet(StringComparer.Ordinal);

            if (!GetCultureFallbacks(CultureInfo.GetCultureInfo(_defaultCultureName))
                    .Any(culture => _resourceNames.Contains(GetManifestResourceName(culture.Name))))
            {
                throw new FileNotFoundException(
                    $"No localization resources were embedded for the default culture '{_defaultCultureName}'.");
            }
        }

        public LocalizedString this[string name] => GetLocalizedString(name);

        public LocalizedString this[string name, params object?[] arguments] => GetLocalizedString(name, arguments);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var cultures = includeParentCultures
                ? GetCultureFallbacks(CultureInfo.CurrentUICulture)
                : [CultureInfo.CurrentUICulture];
            var translations = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var culture in cultures.Concat(GetCultureFallbacks(CultureInfo.GetCultureInfo(_defaultCultureName))))
            {
                foreach (var translation in _translations.GetOrAdd(culture.Name, LoadTranslations))
                {
                    translations.TryAdd(translation.Key, translation.Value);
                }
            }

            return translations.Select(translation => new LocalizedString(
                translation.Key,
                translation.Value,
                resourceNotFound: false));
        }

        private LocalizedString GetLocalizedString(string name, object?[]? arguments = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var requestedCulture = CultureInfo.CurrentUICulture;
            var found = TryGetTranslation(name, out var value, requestedCulture);
            if (!found)
            {
                value = name;
            }

            if (arguments is { Length: > 0 })
            {
                value = string.Format(CultureInfo.CurrentCulture, value, arguments);
            }

            return new LocalizedString(name, value, resourceNotFound: !found);
        }

        private bool TryGetTranslation(string key, out string value, CultureInfo culture)
        {
            foreach (var candidate in GetCultureFallbacks(culture)
                         .Concat(GetCultureFallbacks(CultureInfo.GetCultureInfo(_defaultCultureName))))
            {
                var translations = _translations.GetOrAdd(candidate.Name, LoadTranslations);
                if (translations.TryGetValue(key, out value!))
                {
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        private IReadOnlyDictionary<string, string> LoadTranslations(string cultureName)
        {
            var manifestResourceName = GetManifestResourceName(cultureName);
            using var stream = _resourceAssembly.GetManifestResourceStream(manifestResourceName);
            if (stream is null)
            {
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }

            using var document = JsonDocument.Parse(stream);
            var translations = new Dictionary<string, string>(StringComparer.Ordinal);
            AddTranslations(document.RootElement, string.Empty, translations, manifestResourceName);
            return translations;
        }

        private string GetManifestResourceName(string cultureName)
        {
            return $"{_resourceNamePrefix}{cultureName}.json";
        }

        private static IEnumerable<CultureInfo> GetCultureFallbacks(CultureInfo culture)
        {
            for (var candidate = culture; candidate != CultureInfo.InvariantCulture; candidate = candidate.Parent)
            {
                yield return candidate;
            }
        }

        private static void AddTranslations(
            JsonElement element,
            string prefix,
            IDictionary<string, string> translations,
            string resourceName)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException($"Localization resource '{resourceName}' must contain a JSON object.");
            }

            foreach (var property in element.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    AddTranslations(property.Value, key, translations, resourceName);
                }
                else if (property.Value.ValueKind == JsonValueKind.String)
                {
                    if (!translations.TryAdd(key, property.Value.GetString()!))
                    {
                        throw new InvalidDataException(
                            $"Localization resource '{resourceName}' contains duplicate key '{key}'.");
                    }
                }
                else
                {
                    throw new InvalidDataException(
                        $"Localization resource '{resourceName}' contains a non-string value for key '{key}'.");
                }
            }
        }
    }
}
