using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace regexFinder
{
    public class PatternLoader
    {
        public List<PatternDefinition> LoadPatterns(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"YAML file not found: {path}");

            using var reader = new StreamReader(path);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var root = deserializer.Deserialize<YamlRoot>(reader);
            return root?.Patterns ?? new List<PatternDefinition>();
        }
    }
}