using System;
using System.Collections;
using System.IO;
using System.Text;

namespace NokiaIBConverter
{
    public class Converter : IConverter
    {
        private const int HeaderOffset = 0x244;
        private const int SectionOffset = 0x250;
        private readonly IWriter _writer;
        private readonly string _sourceFilePath;

        public Converter(IWriter writer, string sourceFilePath)
        {
            _writer = writer;
            _sourceFilePath = sourceFilePath;
        }

        public int Convert()
        {
            using (var reader = new FileStream(_sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                SeekHeader(reader);
                return ProcessContacts(reader, _writer);
            }
        }

        private void SeekHeader(FileStream reader)
        {
            reader.Seek(HeaderOffset, SeekOrigin.Begin);
        }

        private int ProcessContacts(FileStream reader, IWriter writer)
        {
            var counter = 0;
            while (reader.Position + SectionOffset <= new FileInfo(_sourceFilePath).Length)
            {
                var contact = ParseContact(reader);
                writer.Write(contact);
                reader.Seek(SectionOffset, SeekOrigin.Current);
                counter++;
            }

            if (counter == 0)
            {
                throw new Exception("קובץ פגום או ריק");
            }

            return counter;
        }


        private ContactEntry ParseContact(FileStream reader)
        {
            const int blockSize = 2;
            var contact = new ContactEntry();

            byte[] blockHeader = new byte[blockSize];
            reader.Read(blockHeader, 0, blockSize);
            byte[] headerData = new byte[blockSize];
            Buffer.BlockCopy(blockHeader, 0, headerData, 0, blockSize);

            if (!StructuralComparisons.StructuralEqualityComparer.Equals(headerData, new byte[] { 56, blockSize }))
            {
                throw new Exception("הקובץ מכיל רשומות פגומות ולא ניתן להמירו");
            }

            reader.Seek(-blockSize, SeekOrigin.Current);

            //Find first name
            reader.Seek(0x60, SeekOrigin.Current);
            var fNameBlock = new byte[blockSize];
            reader.Read(fNameBlock, 0, blockSize);
            int fNameLength = BitConverter.ToUInt16(fNameBlock, 0);

            if (fNameBlock[0] != 0 || fNameBlock[1] != 0)
            {

                byte[] fNameBytes = new byte[fNameLength * blockSize];
                reader.Read(fNameBytes, 0, fNameLength * blockSize);
                contact.FirstName = Encoding.Unicode.GetString(fNameBytes);
            }

            var fNameRevOffset = (fNameLength * blockSize) + blockSize + 0x60;
            reader.Seek(-fNameRevOffset, SeekOrigin.Current);

            //Find last name
            reader.Seek(0xB4, SeekOrigin.Current);
            byte[] lNameBlock = new byte[blockSize];
            reader.Read(lNameBlock, 0, blockSize);
            int lNameLength = BitConverter.ToUInt16(lNameBlock, 0);

            if (lNameBlock[0] != 0 || lNameBlock[1] != 0)
            {

                byte[] lNameBytes = new byte[lNameLength * blockSize];
                reader.Read(lNameBytes, 0, lNameLength * blockSize);
                contact.LastName = Encoding.Unicode.GetString(lNameBytes);
            }

            var lNameRevOffset = (lNameLength * blockSize) + blockSize + 0xB4;
            reader.Seek(-lNameRevOffset, SeekOrigin.Current);

            //Find phone number
            reader.Seek(0x1E, SeekOrigin.Current);
            byte[] numBytes = new byte[1];
            reader.Read(numBytes, 0, 1);
            byte[] numType = new byte[1];
            reader.Read(numType, 0, 1);
            byte[] phoneBytes = new byte[numBytes[0]];
            reader.Read(phoneBytes, 0, numBytes[0]);

            if (phoneBytes.Length > 0 && phoneBytes[0] != 0x00)
            {
                string phoneNumber = string.Empty;
                string revPhoneNumber = string.Empty;
                for (int i = 0; i < phoneBytes.Length; i++)
                {
                    revPhoneNumber += phoneBytes[i].ToString("X2");
                }
                for (int i = 0; i < revPhoneNumber.Length; i += blockSize)
                {
                    phoneNumber += revPhoneNumber[i + 1];
                    phoneNumber += revPhoneNumber[i];
                }
                if (numType[0] == 0x11)
                {
                    phoneNumber = "+" + phoneNumber;
                }
                phoneNumber = phoneNumber.Replace("A", "*");

                contact.PhoneNumber = phoneNumber;
            }

            var phoneNumberOffset = numBytes[0] + 0x20;
            reader.Seek(-phoneNumberOffset, SeekOrigin.Current);

            return contact;
        }
    }
}