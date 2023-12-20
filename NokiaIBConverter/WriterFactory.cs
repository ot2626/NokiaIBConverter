using System;

namespace NokiaIBConverter
{
    public enum WriterType
    {
        VCF,
        CSV
    }

    public enum OutputType
    {
        Single,
        Multi
    }

    public class WriterFactory
    {
        public IWriter CreateSingleFileWriter(WriterType writeType,string folderPath,string fileName)
        {
            switch (writeType)
            {
                case WriterType.VCF:
                    return new VcfWriter(folderPath, $"{fileName}.vcf");

                case WriterType.CSV:
                    throw new NotImplementedException("פורמט זה עדיין לא נתמך");

                default: throw new NotImplementedException("פורמט לא נתמך");
            }
        }

        public IWriter CreateMultiFileWriter(WriterType writeType, string folderPath, string fileName = null)
        {
            switch (writeType)
            {
                case WriterType.VCF:
                    return new VcfWriter(folderPath);

                case WriterType.CSV:
                    throw new NotImplementedException("פורמט זה עדיין לא נתמך");

                default: throw new NotImplementedException("פורמט לא נתמך");
            }
        }
    }
}
