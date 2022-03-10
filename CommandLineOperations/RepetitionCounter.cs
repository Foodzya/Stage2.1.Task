﻿using FileReaderWriter.Extensions;
using FileReaderWriter.ReadOptions;
using FileReaderWriter.TextManipulations;
using FileReaderWriter.WriteOptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static FileReaderWriter.Enums.ArgumentEnum;
using static FileReaderWriter.Enums.FileFormatEnum;

namespace FileReaderWriter.CommandLineOperations
{
    public class RepetitionCounter
    {
        public async void WriteWordsToJsonInDescending(string[] args)
        {
            Task<string> sourceFilePathTask = null;

            Task<string> targetJsonFileTask = null;

            Task[] allTasks;

            List<Argument> allowedArguments = Enum.GetValues(typeof(Argument)).Cast<Argument>().ToList();

            string sourceFilePath;

            string targetJsonPath = null;

            string textFromSourceFile;

            for (var i = 0; i < args.Length; i++)
            {
                string argument = args[i];

                switch (argument)
                {
                    case string sourceArg when sourceArg.Contains(Argument.source.ToValidArgument()):
                        sourceFilePathTask = Task.Factory.StartNew(() => GetSourceFilePath(sourceArg));
                        break;
                    case string targetArg when targetArg.Contains(Argument.target.ToValidArgument()):
                        targetJsonFileTask = Task.Factory.StartNew(() => GetTargetJsonFile(targetArg));
                        break;
                    case string repetitionArg when repetitionArg.Contains(Argument.repetitions.ToValidArgument()):
                        break;
                    default:
                        if (!allowedArguments.Any(arg => argument.Contains(arg.ToValidArgument())))
                            throw new ArgumentException($"Wrong {argument} argument");
                        break;
                }
            }

            if (targetJsonFileTask != null)
            {
                allTasks = new Task[] { sourceFilePathTask, targetJsonFileTask };

                Task.WaitAll(allTasks);

                targetJsonPath = targetJsonFileTask.Result;
            }
            else
            {
                sourceFilePathTask.Wait();
            }

            sourceFilePath = sourceFilePathTask.Result;

            textFromSourceFile = await GetTextFromSourceFile(sourceFilePath, args);

            Dictionary<string, int> keyValueOfRepetitiveWords = GetDictionaryOfRepetativeWords(textFromSourceFile);

            WriteDictionaryToJsonFileInDescending(keyValueOfRepetitiveWords, targetJsonPath, args);
        }

        private Dictionary<string, int> GetDictionaryOfRepetativeWords(string textFromSourceFile)
        {
            char[] delimeterChars = { ' ', ',', '.', ':', ';', '\t', '\r', '\n', '—', '-', '"' };

            Dictionary<string, int> dictionaryWithRepetitiveWords = new Dictionary<string, int>();

            List<string> listOfAllWords = textFromSourceFile.ToLower().Split(delimeterChars, StringSplitOptions.RemoveEmptyEntries).ToList();

            listOfAllWords.ForEach(word =>
            {
                if (dictionaryWithRepetitiveWords.ContainsKey(word))
                {
                    dictionaryWithRepetitiveWords[word]++;
                }
                else
                {
                    dictionaryWithRepetitiveWords.Add(word, 1);
                }
            });

            return dictionaryWithRepetitiveWords;
        }

        private void WriteDictionaryToJsonFileInDescending(Dictionary<string, int> dictionaryWithRepetitiveWords, string targetJsonPath, string[] args)
        {
            List<string> orderedTextInJsonFormat = dictionaryWithRepetitiveWords.OrderByDescending(word => word.Value).Select(keyValuePair => string.Format("{0}:{1}", keyValuePair.Key, keyValuePair.Value)).ToList();

            TxtWriter txtWriter = new TxtWriter();

            if (args.Contains(Argument.json.ToValidArgument()))
            {
                using (TextWriter tw = new StreamWriter(targetJsonPath))
                {
                    orderedTextInJsonFormat.ForEach(str =>
                    {
                        if (str == orderedTextInJsonFormat.First())
                        {
                            tw.Write("[ {" + str + "}, ");
                        }
                        else if (str == orderedTextInJsonFormat.Last())
                        {
                            tw.Write("{" + str + "} ]");

                        }
                        else
                        {
                            tw.Write("{" + str + "}, ");
                        }
                    });
                }
            }
            else if (args.Contains(Argument.console.ToValidArgument()))
            {
                orderedTextInJsonFormat.ForEach(str =>
                {
                    if (str == orderedTextInJsonFormat.First())
                    {
                        Console.Write("[ {" + str + "}, ");
                    }
                    else if (str == orderedTextInJsonFormat.Last())
                    {
                        Console.Write("{" + str + "} ]");

                    }
                    else
                    {
                        Console.Write("{" + str + "}, ");
                    }
                });
            }
            else
            {
                throw new ArgumentException($"Command line must contain --json (along with --target= argument) or --console (--target argument not necessary)");
            }
        }

        private async Task<string> GetTextFromSourceFile(string sourceFilePath, string[] args)
        {
            string format = Path.GetExtension(sourceFilePath);

            string formattedTextFromSourceFile;

            FileReader fileReader = new FileReader();

            CaesarEncryptor caesarDecryptor = new CaesarEncryptor();

            if (format != FileFormat.etxt.ToValidFileFormat())
            {
                formattedTextFromSourceFile = await fileReader.ReadContentFromFileAsync(sourceFilePath);

                return formattedTextFromSourceFile;
            }
            else
            {
                string shiftArgument = args.FirstOrDefault(arg => arg.Contains(Argument.shift.ToValidArgument()));

                string directionArgument = args.FirstOrDefault(arg => arg.Contains(Argument.direction.ToValidArgument()));

                int shift = caesarDecryptor.GetShiftFromCommandLine(shiftArgument);

                string direction = caesarDecryptor.GetDirectionFromCommandLine(directionArgument);

                string content = await File.ReadAllTextAsync(sourceFilePath);

                if (direction == "left")
                {
                    formattedTextFromSourceFile = caesarDecryptor.LeftShiftCipher(content, shift);

                    return formattedTextFromSourceFile;
                }
                else if (direction == "right")
                {
                    formattedTextFromSourceFile = caesarDecryptor.RightShiftCipher(content, shift);

                    return formattedTextFromSourceFile;
                }
            }

            throw new FormatException($"You have specified wrong file format {format}");
        }

        private string GetTargetJsonFile(string targetArg)
        {
            int pathIndex = targetArg.IndexOf('=') + 1;

            string targetJsonPath = targetArg.Substring(pathIndex);

            string format = Path.GetExtension(targetJsonPath);

            if (format == FileFormat.json.ToValidFileFormat() && File.Exists(targetJsonPath))
            {
                return targetJsonPath;
            }

            throw new FormatException($"Only json format is valid. Wrong {format} format");
        }

        private string GetSourceFilePath(string sourceArg)
        {
            int pathIndex = sourceArg.IndexOf('=') + 1;

            string path = sourceArg.Substring(pathIndex).Trim();

            if (File.Exists(path))
            {
                return path;
            }

            throw new DirectoryNotFoundException($"Directory {path} doesn't exist");
        }
    }
}