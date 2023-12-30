using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NokiaIBConverter
{
    public class VcfWriter : IWriter
    {
        private readonly string _contactsFolderPath;
        private readonly StreamWriter _streamWriter;

        public VcfWriter(string contactsFolderPath)
        {
            _contactsFolderPath = contactsFolderPath;
            CreateTargetFolder(contactsFolderPath);
        }

        public VcfWriter(string contactsFolderPath, string vcfFileName)
        {
            _contactsFolderPath = contactsFolderPath;
            CreateTargetFolder(contactsFolderPath);
            _streamWriter = new StreamWriter($"{contactsFolderPath}\\{vcfFileName}", false, Encoding.UTF8);
        }

        public void Write(ContactEntry contact)
        {
            StreamWriterScope localWriterScope = null;
            StreamWriter streamWriter = _streamWriter;
            var firstName = contact.FirstName ?? string.Empty;
            var lastName = contact.LastName ?? string.Empty;
            var phone = contact.PhoneNumber ?? string.Empty;
            
            if (streamWriter == null)
            {
                var uniqueId = $"{_contactsFolderPath}\\{CleanString(firstName + lastName)}.vcf";
                streamWriter = new StreamWriter(uniqueId, false, Encoding.UTF8);
                localWriterScope = new StreamWriterScope(streamWriter);
            }
            
            streamWriter.WriteLine("BEGIN:VCARD");
            streamWriter.WriteLine("VERSION:2.1");
            streamWriter.WriteLine($"FN;ENCODING=QUOTED-PRINTABLE;CHARSET=utf-8:{firstName} {lastName}");
            streamWriter.WriteLine($"N;ENCODING=QUOTED-PRINTABLE;CHARSET=utf-8:{firstName};{lastName}");
            streamWriter.WriteLine($"TEL;CELL:{phone.Replace("F", string.Empty)}");
            streamWriter.WriteLine("END:VCARD");
            localWriterScope?.Dispose();
        }

        private string CleanString(string str)
        {
            return new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars())))).Replace(str, string.Empty);
        }

        private void CreateTargetFolder(string targetFolder)
        {
            try
            {
                Directory.CreateDirectory(targetFolder);
            }
            catch
            {
                // do nothing
            }
        }
    }

    public class StreamWriterScope : IDisposable
    {
        private readonly StreamWriter _writer;

        public StreamWriterScope(StreamWriter writer)
        {
            _writer = writer;
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
