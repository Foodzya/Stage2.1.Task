﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace FileReaderWriter.WriteOptions
{
    public class FileWriter
    {
        private IFileWriter _writer;

        public FileWriter()
        {

        }

        public async Task WriteToFileAsync(string content, string targetFile)
        {
            SetWriterByFileExtension(targetFile);

            await _writer.WriteToFileAsync(content, targetFile);
        }

        private void SetWriter(IFileWriter writer)
        {
            _writer = writer;
        }

        private void SetWriterByFileExtension(string path)
        {
            string extension = Path.GetExtension(path);

            switch (extension)
            {
                case ".txt":
                    SetWriter(new TxtWriter());
                    break;
                case ".rtxt":
                    SetWriter(new RtxtWriter());
                    break;
                case ".etxt":
                    SetWriter(new EtxtWriter());
                    break;
                case ".btxt":
                    SetWriter(new BtxtWriter());
                    break;
                default:
                    Console.WriteLine("This file type is unsupported.\n" +
                        "Press anything to continue..");
                    break;
            }
        }
    }
}