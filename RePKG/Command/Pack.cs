using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using Newtonsoft.Json;
using RePKG.Application.Package;
using RePKG.Application.Texture;
using RePKG.Core.Package;
using RePKG.Core.Package.Enums;
using RePKG.Core.Package.Interfaces;
using RePKG.Core.Texture;

namespace RePKG.Command
{
    public static class Pack
    {
        private static PackOptions _options;

        private static readonly ITexWriter _texWriter;
        private static readonly ImageToTexConverter _imageToTexConverter;

        static Pack()
        {
            _texWriter = TexWriter.Default;
            _imageToTexConverter = new ImageToTexConverter();
        }

        public static void Action(PackOptions options)
        {
            _options = options;

            if (string.IsNullOrEmpty(options.OutputDirectory))
            {
                _options.OutputDirectory = Directory.GetCurrentDirectory();
            }

            PackFile();
            Console.WriteLine("Done Packing");
        }

        private static void PackFile()
        {
            Directory.CreateDirectory(_options.OutputDirectory);

            var inputFileInfo = new FileInfo(_options.Input);

            ITex tex = _imageToTexConverter.LoadFileToTex(inputFileInfo);

            var outputFilePath = Path.Combine(_options.OutputDirectory, Path.GetFileNameWithoutExtension(inputFileInfo.Name));
            using (var stream = File.OpenWrite($"{outputFilePath}.tex"))
            using (var writer = new BinaryWriter(stream))
            {
                _texWriter.WriteTo(writer, tex);
            }
        }
    }


    [Verb("pack", HelpText = "Pack/convert image into TEX.")]
    public class PackOptions
    {
        [Option('o', "output", Required = false, HelpText = "Output directory", Default = "./output")]
        public string OutputDirectory { get; set; }

        [Option("overwrite", HelpText = "Overwrite all existing files")]
        public bool Overwrite { get; set; }

        [Value(0, Required = true, HelpText = "Path to file/directory", MetaName = "Input")]
        public string Input { get; set; }
    }
}