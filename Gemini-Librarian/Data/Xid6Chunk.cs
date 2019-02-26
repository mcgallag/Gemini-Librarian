using System.IO;

namespace Gemini_Librarian.Data
{
    /// <summary>
    /// Encapsulates a "Chunk" as specified in the Extended ID666 tag format
    /// </summary>
    /// Reference: http://snesmusic.org/files/spc_file_format.txt
    public class Xid6Chunk
    {
        public enum Xid6Type
        {
            Xid6Integer, Xid6String, Xid6Length
        }

        /// <summary>
        /// Readable description of ID number
        /// </summary>
        public string Description
        {
            get
            {
                switch (id)
                {
                    case 0x01:
                        return "Song Name";
                    case 0x02:
                        return "Game Name";
                    case 0x03:
                        return "Artist Name";
                    case 0x04:
                        return "Dumper Name";
                    case 0x05:
                        return "Dumping Date";
                    case 0x06:
                        return "Emulator Used";
                    case 0x07:
                        return "Comments";
                    case 0x10:
                        return "Official Soundtrack Title";
                    case 0x11:
                        return "OST Disc Number";
                    case 0x12:
                        return "OST Track Number";
                    case 0x13:
                        return "Publisher Name";
                    case 0x14:
                        return "Copyright Year";
                    case 0x30:
                        return "Introduction Length (ticks)";
                    case 0x31:
                        return "Loop Length (ticks)";
                    case 0x32:
                        return "End Length (ticks)";
                    case 0x33:
                        return "Fade length (ticks)";
                    case 0x34:
                        return "Muted Channels (one bit set per channel muted)";
                    case 0x35:
                        return "Number of Times to Loop";
                    case 0x36:
                        return "Amplification Value (65536 = Normal)";
                    default:
                        return "Unknown ID";
                }
            }
        }

        /// <summary>
        /// ID value of the Chunk data (see SPC file spec)
        /// </summary>
        public int Id
        {
            get => id;
            set
            {
                id = value;
                Type = GetTypeFromID(value);
            }
        }
        private int id;

        /// <summary>
        /// Type of the Chunk data
        /// </summary>
        public Xid6Type Type { get; private set; }

        /// <summary>
        /// Length of the chunk data in bytes
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Data stored in this chunk, cast to a string
        /// </summary>
        public string Data
        {
            get
            {
                return (Type == Xid6Type.Xid6String) ? stringData : intData.ToString();
            }
            set
            {
                if (Type == Xid6Type.Xid6String)
                    stringData = value;
                else
                    intData = int.Parse(value);
            }
        }

        private int intData;
        private string stringData;

        /// <summary>
        /// Determines chunk data type based on ID, lifted directly from Xid6 specification
        /// </summary>
        /// <param name="idValue">ID value</param>
        /// <returns></returns>
        private static Xid6Type GetTypeFromID(int idValue)
        {
            switch (idValue)
            {
                case 0x01: // song name
                case 0x02: // game name
                case 0x03: // artist name
                case 0x04: // dumper's name
                case 0x07: // comments
                case 0x10: // official soundtrack title
                case 0x13: // publisher's name
                    return Xid6Type.Xid6String;
                case 0x05: // date song was dumped YYYYMMDD
                case 0x30: // Introduction length
                case 0x31: // Loop length
                case 0x32: // End length
                case 0x33: // Fade length
                case 0x36: // amplification value
                    return Xid6Type.Xid6Integer;
                case 0x06: // emulator used
                case 0x11: // OST disc
                case 0x12: // OST track (upper byte is 0-99, lower byte is an optional ASCII char)
                case 0x14: // copyright year
                case 0x34: // muted channels (a bit is set for each channel that's muted)
                case 0x35: // number of times to loop the loop section
                    return Xid6Type.Xid6Length;
            }
            throw new FileFormatException($"Invalid Xid6 ID number {idValue}");
        }
    }
}
