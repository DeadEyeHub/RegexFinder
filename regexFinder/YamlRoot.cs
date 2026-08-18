using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace regexFinder
{
    public static class YamlRoot
    {
        public static (List<BlockDefinition> Blocks, List<PatternDefinition> Patterns) LoadParts(string path)
        {
            var yamlText = File.ReadAllText(path);
            return LoadPartsFromString(yamlText);
        }

        public static (List<BlockDefinition> Blocks, List<PatternDefinition> Patterns) LoadPartsFromString(string yamlText)
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yamlText));
            if (stream.Documents.Count == 0)
                return (new List<BlockDefinition>(), new List<PatternDefinition>());

            if (stream.Documents[0].RootNode is not YamlMappingNode root)
                throw new InvalidDataException("YAML root must be a mapping (key: value).");

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var serializer = new SerializerBuilder().Build();

            var blocks = new List<BlockDefinition>();
            if (root.Children.TryGetValue(new YamlScalarNode("blocks"), out var blocksNode))
            {
                var blocksYaml = serializer.Serialize(blocksNode);
                blocks = deserializer.Deserialize<List<BlockDefinition>>(blocksYaml) ?? new();
            }

            var patterns = new List<PatternDefinition>();
            if (root.Children.TryGetValue(new YamlScalarNode("patterns"), out var patternsNode))
            {
                var patternsYaml = serializer.Serialize(patternsNode);
                patterns = deserializer.Deserialize<List<PatternDefinition>>(patternsYaml) ?? new();
            }

            CompileAll(blocks, patterns);

            return (blocks, patterns);
        }

        private static void CompileAll(List<BlockDefinition> blocks, List<PatternDefinition> patterns)
        {
            var errors = new StringBuilder();

            foreach (var b in blocks)
            {
                try
                {
                    b.BuildRegexes(timeout: TimeSpan.FromSeconds(2), ignoreCase: true);
                }
                catch (Exception ex)
                {
                    errors.AppendLine($"Block '{b?.Name ?? "?"}': {ex.Message}");
                }
            }

            foreach (var p in patterns)
            {
                try
                {
                    p.BuildRegex(timeout: TimeSpan.FromSeconds(2));
                }
                catch (Exception ex)
                {
                    errors.AppendLine($"Pattern '{p?.Name ?? "?"}': {ex.Message} [{p?.RegexCommand}]");
                }
            }

            var blockNames = new HashSet<string>(
                blocks.Where(b => !string.IsNullOrWhiteSpace(b?.Name))
                      .Select(b => b.Name),
                StringComparer.OrdinalIgnoreCase);

            foreach (var p in patterns)
            {
                if (p == null || string.IsNullOrWhiteSpace(p.BlockName)) continue;
                if (!blockNames.Contains(p.BlockName.Trim()))
                {
                    errors.AppendLine(
                        $"Pattern '{p.Name ?? "?"}' references unknown block '{p.BlockName}'.");
                }
            }

            if (errors.Length > 0)
                throw new InvalidDataException("Regex compilation errors:\n" + errors.ToString());
        }
    }
}
