using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TRex.CommandLine;
using Xunit;
using Xunit.Abstractions;

namespace TRexLib.Tests
{
    public class CommandLineTests
    {
        private readonly ITestOutputHelper output;

        private readonly IConsole console = new TestConsole();

        private readonly JsonConverter[] converters =
        {
            new FileInfoJsonConverter(),
            new DirectoryInfoJsonConverter()
        };

        public CommandLineTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task When_no_arguments_are_given_then_files_are_discovered_recursively()
        {
            await CommandLine.Parser.InvokeAsync("--format json", console);

            var results = JsonConvert.DeserializeObject<TestResultSet>(console.Out.ToString(), converters);

            var directories = results.Select(e => e.TestOutputFile).Select(f => f.Directory).ToArray();
            directories.Should().Contain(d => d.Name == "TRXs");
            directories.Should().Contain(d => d.Name == "1" && d.Parent.Name == "TRXs");
        }

        [Fact]
        public async Task When_one_argument_is_given_and_it_is_a_file_path_then_it_is_interpreted_as_a_file_path()
        {
            var filePath = new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                .FullName;

            await CommandLine.Parser.InvokeAsync($"--file \"{filePath}\" --format json", console);
            
            output.WriteLine(console.Out.ToString());

            var results = JsonConvert.DeserializeObject<TestResultSet>(console.Out.ToString(), converters);
            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task When_multiple_TRX_files_exist_in_the_directory_only_the_latest_is_read()
        {
            var directoryPath = new DirectoryInfo(Path.Combine("TRXs", "2")).FullName;

            await CommandLine.Parser.InvokeAsync($"--path \"{directoryPath}\" --format json", console);

            output.WriteLine(console.Out.ToString());

            var results = JsonConvert.DeserializeObject<TestResultSet>(console.Out.ToString(), converters);
            results.Should().HaveCount(18);
        }

        [Fact]
        public async Task A_filter_expression_can_be_used_to_match_only_specific_tests()
        {
            var directoryPath = new DirectoryInfo(Path.Combine("TRXs", "2")).FullName;

            await CommandLine.Parser.InvokeAsync($"--path \"{directoryPath}\" --filter *verbosity* --format json", console);

            output.WriteLine(console.Error.ToString());
            output.WriteLine(console.Out.ToString());

            var results = JsonConvert.DeserializeObject<TestResultSet>(console.Out.ToString(), converters);

            results.Should().HaveCount(10);

            results.Select(r => r.FullyQualifiedTestName).Should().OnlyContain(name => name.Contains("verbosity"));
        }
    }
}
