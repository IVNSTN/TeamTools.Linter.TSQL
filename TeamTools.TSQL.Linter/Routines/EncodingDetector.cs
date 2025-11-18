using System.IO;
using System.Text;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class EncodingDetector
    {
        public Encoding GetFileEncoding(string filePath)
        {
            using (FileStream file = File.OpenRead(filePath))
            {
                return GetFileEncoding(file);
            }
        }

        public Encoding GetFileEncoding(Stream file)
        {
            Encoding enc = null;

            try
            {
                if (file.CanSeek)
                {
                    byte[] bom = new byte[4]; // Get the byte-order mark, if there is one
                    _ = file.Read(bom, 0, 4);
                    byte bomStart = bom[0];

                    if (bomStart == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                    {
                        enc = Encoding.UTF8;
                    }
                    else if (
                        // BE
                        (bomStart == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
                        // LE
                        || (bomStart == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0))
                    {
                        // ucs-4
                        enc = Encoding.UTF32;
                    }
                    else if (
                        // ucs-2le, ucs-4le, and ucs-16le
                        (bomStart == 0xff && bom[1] == 0xfe)
                        // utf-16 and ucs-2
                        || (bomStart == 0xfe && bom[1] == 0xff))
                    {
                        enc = Encoding.Unicode;
                    }
                    else
                    {
                        enc = Encoding.ASCII;
                    }

                    // Now reposition the file cursor back to the start of the file
                    file.Seek(0, System.IO.SeekOrigin.Begin);
                }
                else
                {
                    // The file cannot be randomly accessed, so you need to decide what to set the default to
                    // based on the data provided. If you're expecting data from a lot of older applications,
                    // default your encoding to Encoding.ASCII. If you're expecting data from a lot of newer
                    // applications, default your encoding to Encoding.Unicode. Also, since binary files are
                    // single byte-based, so you will want to use Encoding.ASCII, even though you'll probably
                    // never need to use the encoding then since the Encoding classes are really meant to get
                    // strings from the byte array that is the file.
                    enc = Encoding.ASCII;
                }

                return enc;
            }
            catch
            {
                return enc;
            }
        }
    }
}
