using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemini_Librarian.Data
{
    public enum FileFormat
    {
        VGM, VGZ, SPC, RSN, UNK
    }
    public class MusicTrack
    {
        public string TrackNameEnglish { get; set; }
        public string TrackNameJapanese { get; set; }
        public string GameNameEnglish { get; set; }
        public string GameNameJapanese { get; set; }
        public string SystemNameEnglish { get; set; }
        public string SystemNameJapanese { get; set; }
        public string TrackAuthorEnglish { get; set; }
        public string TrackAuthorJapanese { get; set; }
        public string ReleaseDate { get; set; }
        public string VGMConverterName { get; set; }
        public string Notes { get; set; }

        public FileFormat FileFormat { get; set; }
        public string FilePath { get; set; }

        private List<Xid6Chunk> extendedTagData = new List<Xid6Chunk>();

        public MusicTrack(string path)
        {
            FilePath = path;
            FileFormat = MusicFileManager.GetFileFormat(FilePath);
        }

        public void AddChunk(Xid6Chunk chunk)
        {
            extendedTagData.Add(chunk);
        }

        public Xid6Chunk[] GetExtendedTagData()
        {
            return extendedTagData.ToArray();
        }
    }
}
