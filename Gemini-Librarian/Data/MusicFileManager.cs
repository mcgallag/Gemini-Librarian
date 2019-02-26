using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using SharpCompress.Readers;
using SharpCompress.Readers.Rar;

namespace Gemini_Librarian.Data
{
    /// <summary>
    /// Encapsulates helper methods for extracting metadata from various formats
    /// </summary>
    class MusicFileManager
    {
        /// <summary>
        /// Retrieves MusicTrack metadata from a given file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns></returns>
        public static MusicTrack ReadMetaData(string path)
        {
            MusicTrack trackInfo = new MusicTrack(path);
            ReadMetaData(trackInfo);

            return trackInfo;
        }

        /// <summary>
        /// Retrieves MusicTrack metadata from a given file
        /// </summary>
        /// <param name="track">Metadata with FileFormat and FilePath set to target</param>
        private static void ReadMetaData(MusicTrack track)
        {
            if (!File.Exists(track.FilePath))
                throw new FileNotFoundException($"File {track.FilePath} not found.");
            switch(track.FileFormat)
            {
                case FileFormat.VGM:
                    ReadMetaDataFromVGM(track);
                    break;
                case FileFormat.SPC:
                    ReadMetaDataFromSPC(track);
                    break;
                case FileFormat.VGZ:
                    ReadMetaDataFromVGZ(track);
                    break;
                case FileFormat.RSN:
                    ReadMetaDataFromRSN(track);
                    break;
                default:
                    throw new FileFormatException("Invalid file format for ReadMetaData.");
            }
        }

        /// <summary>
        /// Determines FileFormat based on the file extension
        /// </summary>
        /// <param name="path">Path to a file</param>
        /// <returns></returns>
        public static FileFormat GetFileFormat(string path)
        {
            switch(Path.GetExtension(path).ToLower())
            {
                case ".vgm":
                    return FileFormat.VGM;
                case ".vgz":
                case ".zip":
                    return FileFormat.VGZ;
                case ".spc":
                    return FileFormat.SPC;
                case ".rsn":
                    return FileFormat.RSN;
                default:
                    return FileFormat.UNK;
            }
        }

        /// <summary>
        /// Given a MusicTrack for a .VGM format file, extract the metadata from the GD3 tag
        /// </summary>
        /// <param name="musicTrack">MusicTrack object to populate with metadata</param>
        private static void ReadMetaDataFromVGM(MusicTrack musicTrack)
        {
            using (FileStream file = new FileStream(musicTrack.FilePath, FileMode.Open))
            {
                // send the file stream to the proper handler function
                ReadMetaDataFromVGM(musicTrack, file);
            }
        }

        /// <summary>
        /// Given a Stream to VGM file data, extract the metadata from the GD3 tag
        /// </summary>
        /// <param name="musicTrack">MusicTrack object to populate with metadata</param>
        /// <param name="file">Stream of VGM file data</param>
        private static void ReadMetaDataFromVGM(MusicTrack musicTrack, Stream file)
        {
            // File specification reference:
            // http://www.smspower.org/uploads/Music/vgmspec170.txt

            // check for the ID tag at the beginning
            byte[] b = new byte[4];
            file.Read(b, 0, 4);
            string vgmFileIdent = Encoding.ASCII.GetString(b);
            if (vgmFileIdent != "Vgm ")
                throw new FileFormatException("\"Vgm \" file ident missing from file.");

            // The VGM file specification dictates that at offset 0x14 is an 32-bit value
            // of where the GD3 tag starts, relative to current position. If this offset
            // is zero, then there is no GD3 tag present
            long gd3TagOffset = 0x10;
            file.Seek(gd3TagOffset, SeekOrigin.Current);
            file.Read(b, 0, 4);
            long gd3RelativeOffset = BitConverter.ToInt32(b, 0);
            if (gd3RelativeOffset == 0)
                throw new FileFormatException("GD3 tag not present: GD3 relative offset is zero.");

            // seek to the GD3 tag
            file.Seek(gd3RelativeOffset - 4, SeekOrigin.Current);

            // read the tag from the stream
            ReadMetaDataFromGD3Tag(musicTrack, file);
        }

        /// <summary>
        /// Given a MusicTrack for a .VGZ format file, extract the metadata from the GD3 tag
        /// </summary>
        /// <param name="track">MusicTrack object to populate with metadata</param>
        private static void ReadMetaDataFromVGZ(MusicTrack track)
        {
            // VGZ is just a VGM compressed with GZIP so start by reading the archive into a buffer
            byte[] file = File.ReadAllBytes(track.FilePath);

            // set up a new MemoryStream to hold the decompressed VGM file
            using (MemoryStream vgmFile = new MemoryStream())
            {
                // instantiate a GZipStream to handle decompression
                using (GZipStream zipFile = new GZipStream(new MemoryStream(file), CompressionMode.Decompress))
                {
                    // decompress and copy the VGM file into the MemoryStream
                    const int bufferSize = 4096;
                    byte[] buffer = new byte[bufferSize];
                    while (zipFile.Read(buffer, 0, bufferSize) > 0)
                    {
                        vgmFile.Write(buffer, 0, bufferSize);
                    }
                }

                // reset the MemoryStream to the beginning
                vgmFile.Seek(0, SeekOrigin.Begin);
                // read the memory data from the extracted VGM file
                ReadMetaDataFromVGM(track, vgmFile);
            }
        }

        /// <summary>
        /// Fills GD3 tag metadata from "file" into the given MusicTrack object
        /// </summary>
        /// <param name="musicTrack"></param>
        /// <param name="file"></param>
        private static void ReadMetaDataFromGD3Tag(MusicTrack musicTrack, Stream file)
        {
            // Reference:
            // http://www.smspower.org/uploads/Music/gd3spec100.txt
            
            byte[] b = new byte[4];
            file.Read(b, 0, 4);
            string gd3TagMarker = Encoding.ASCII.GetString(b);

            // the first 4 bytes of the GD3 tag should be "Gd3 " in ASCII encoding
            // per the GD3 format specification
            if (gd3TagMarker != "Gd3 ")
                throw new FileFormatException("GD3 tag format error: \'Gd3 \' marker not present.");

            // skip the version number and length of metadata (2*4 bytes)
            file.Seek(8, SeekOrigin.Current);

            // meta data is encoded as null terminated UTF-16 strings
            musicTrack.TrackNameEnglish = ReadUTF16String(file);
            musicTrack.TrackNameJapanese = ReadUTF16String(file);
            musicTrack.GameNameEnglish = ReadUTF16String(file);
            musicTrack.GameNameJapanese = ReadUTF16String(file);
            musicTrack.SystemNameEnglish = ReadUTF16String(file);
            musicTrack.SystemNameJapanese = ReadUTF16String(file);
            musicTrack.TrackAuthorEnglish = ReadUTF16String(file);
            musicTrack.TrackAuthorJapanese = ReadUTF16String(file);
            musicTrack.ReleaseDate = ReadUTF16String(file);
            musicTrack.VGMConverterName = ReadUTF16String(file);
            musicTrack.Notes = ReadUTF16String(file);
        }

        /// <summary>
        /// Returns a UTF-16 encoded string terminated by \0 from file
        /// </summary>
        /// <param name="file">input file</param>
        /// <returns></returns>
        private static string ReadUTF16String(Stream file)
        {
            BinaryReader reader = new BinaryReader(file, Encoding.Unicode);
            char ch;
            string str = string.Empty;
            while ((ch = reader.ReadChar()) != '\0')
                str += ch;
            return str;
        }

        /// <summary>
        /// Returns a UTF-7 encoded string from "file." Forces a read of length bytes.
        /// 
        /// <para>Excess \0 characters will be discarded.</para>
        /// </summary>
        /// <param name="file">input file</param>
        /// <param name="length">number of bytes to read</param>
        /// <returns></returns>
        private static string ReadUTF7String(BinaryReader reader, int length)
        {
            string str = new string(reader.ReadChars(length));
            str = str.Replace("\0", string.Empty);
            return str;
        }

        private static void ReadMetaDataFromSPC(MusicTrack musicTrack)
        {
            using (FileStream file = new FileStream(musicTrack.FilePath, FileMode.Open))
                ReadMetaDataFromSPC(musicTrack, file);
        }

        private static void ReadMetaDataFromSPC(MusicTrack musicTrack, Stream file)
        {
            using (BinaryReader reader = new BinaryReader(file, Encoding.UTF7))
            {
                //byte[] spcHeader = new byte[33];
                //file.Read(spcHeader, 0, 33);
                //string spcHeaderString = Encoding.ASCII.GetString(spcHeader);

                // make a couple checks if the file format is valid
                // per the SPC specification
                if (ReadUTF7String(reader, 33) != "SNES-SPC700 Sound File Data v0.30")
                    throw new FileFormatException("Invalid SPC file header.");
                if (reader.ReadByte() != 26 || reader.ReadByte() != 26)
                    throw new FileFormatException("Invalid SPC file header: offset 0x23,24 must be (26,26)");

                //file.Seek(2, SeekOrigin.Current);

                // byte at offset 0x24 will be equal to 26 if there is ID666 metadata present
                if (reader.ReadByte() == 26)
                {
                    file.Seek(0xA, SeekOrigin.Current);
                    ReadMetaDataFromID666(musicTrack, reader);
                }

                // check for xid6 tag, which will be located at offset 0x10200 if it exists
                // TODO - add some error checking here if data is not present!
                //byte[] xid6Header = new byte[4];
                file.Seek(0x10200, SeekOrigin.Begin);
                //file.Read(xid6Header, 0, 4);
                //string xid6HeaderString = Encoding.ASCII.GetString(xid6Header);

                if (ReadUTF7String(reader, 4) == "xid6")
                {
                    int xid6Remaining = reader.ReadInt32();

                    while (xid6Remaining > 0)
                    {
                        Xid6Chunk chunk = new Xid6Chunk();
                        int bytesRead = ReadXid6Chunk(reader, chunk);
                        musicTrack.AddChunk(chunk);
                        xid6Remaining -= bytesRead;
                    }
                }
            }
        }

        /// <summary>
        /// Reads a single Xid6 chunk from a FileStream into a supplied Xid6Chunk.
        /// <para>Returns the number of bytes read from the file.</para>
        /// </summary>
        /// <param name="file">File from which to read</param>
        /// <param name="chunk">Chunk in which to populate the data</param>
        /// <returns>Number of bytes read</returns>
        private static int ReadXid6Chunk(BinaryReader reader, Xid6Chunk chunk)
        {
            // minimum bytes read will be 4, the size of the Xid6Chunk Header
            int bytesRead = 4;

            // TODO - just in case
            // set up a binary reader for getting the data
            // BinaryReader reader = new BinaryReader(file);

            // first byte is the ID, setting the ID also dynamically sets Xid6Chunk.Type
            chunk.Id = reader.ReadByte();

            // second byte is "type" which is inconsequential in my implementation
            int type = reader.ReadByte();

            // if the Type is type "Length" then the data is in the final 2 bytes
            // of the header as a 16-bit integer
            if (chunk.Type == Xid6Chunk.Xid6Type.Xid6Length)
            {
                // read 2 bytes from the reader for the data and we're done
                chunk.Length = 2;
                chunk.Data = reader.ReadInt16().ToString();
            } else
            {
                // if the type is of type String/Integer then the data length
                // is instead stored in the final 2 bytes of the header
                chunk.Length = reader.ReadInt16();

                if (chunk.Type == Xid6Chunk.Xid6Type.Xid6String)
                {
                    // first we have to pad the length if it is not 32-bit aligned
                    // per the Xid6 specification in the SPC specs
                    // i.e. if the text is 9 bytes, raise the length to 12 bytes
                    // it will be padded with \0 so will not affect the decoding from UTF7
                    int padding = chunk.Length % 4;
                    if (padding != 0) padding = 4 - padding;

                    bytesRead += chunk.Length + padding;

                    // read a string of given length as UTF7 encoded data
                    chunk.Data = ReadUTF7String(reader, chunk.Length + padding);
                }
                else if (chunk.Type == Xid6Chunk.Xid6Type.Xid6Integer)
                {
                    // cheating a little here, but Integer values in Xid6 spec
                    // are always 4-bytes, so just read it as an Int32
                    chunk.Data = reader.ReadInt32().ToString();
                    bytesRead += 4;
                }
            }
            return bytesRead;
        }

        /// <summary>
        /// Reads ID666 tag metadata
        /// </summary>
        /// <param name="musicTrack">Object to populate with metadata</param>
        /// <param name="reader">A file stream prepared for ID666 reading</param>
        private static void ReadMetaDataFromID666(MusicTrack musicTrack, BinaryReader reader)
        {
            musicTrack.TrackNameEnglish = ReadUTF7String(reader, 32);
            musicTrack.GameNameEnglish = ReadUTF7String(reader, 32);
            musicTrack.VGMConverterName = ReadUTF7String(reader, 16);
            musicTrack.Notes = ReadUTF7String(reader, 32);
            musicTrack.ReleaseDate = ReadUTF7String(reader, 11);
            string fadeOutSeconds = ReadUTF7String(reader, 3);
            string fadeInMilliseconds = ReadUTF7String(reader, 5);
            musicTrack.TrackAuthorEnglish = ReadUTF7String(reader, 32);
        }

        private static void ReadMetaDataFromRSN(MusicTrack rsnFile)
        {
            using (FileStream file = new FileStream(rsnFile.FilePath, FileMode.Open))
            using (RarReader reader = RarReader.Open(file))
            {
                while (reader.MoveToNextEntry())
                {
                    RarReaderEntry entry = reader.Entry;
                    if (GetFileFormat(entry.Key) == FileFormat.SPC)
                    {
                        using (MemoryStream spcFile = new MemoryStream())
                        using (var entryStream = reader.OpenEntryStream())
                        {
                            entryStream.CopyTo(spcFile);
                            spcFile.Seek(0, SeekOrigin.Begin);
                            ReadMetaDataFromSPC(rsnFile, spcFile);
                        }
                    }
                }
            }
        }
    }
}
