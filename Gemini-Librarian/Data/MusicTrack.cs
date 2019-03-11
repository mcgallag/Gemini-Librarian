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

    /// <summary>
    /// Encapsulates functionality of an archive of multiple music files (such as RSN)
    /// </summary>
    public class ArchiveMusicTrack : MusicTrack
    {
        /// <summary>
        /// Holds MusicTrack objects for each song in the archive
        /// </summary>
        private List<InternalMusicTrack> archivedTracks = new List<InternalMusicTrack>();

        /// <summary>
        /// Public-facing property of Archive contents
        /// </summary>
        public InternalMusicTrack[] Entries
        {
            get
            {
                return archivedTracks.ToArray();
            }
        }

        /// <summary>
        /// Instantiates a new archive object
        /// </summary>
        /// <param name="archivePath"></param>
        public ArchiveMusicTrack(string archivePath) : base(archivePath)
        {
            // ensure proper input format
            // bit of a HACK, should validate .rar identity from bytestream
            if (FileFormat != FileFormat.RSN)
                throw new FileFormatException("Attempted to make a ArchiveMusicTrack from a non-Archive!");
        }

        /// <summary>
        /// Adds an entry to the archive's track listing
        /// </summary>
        /// <param name="internalTrack"></param>
        public void AddTrack(InternalMusicTrack internalTrack)
        {
            archivedTracks.Add(internalTrack);
        }
    }

    /// <summary>
    /// A MusicTrack that is inside an archive file
    /// </summary>
    public class InternalMusicTrack : MusicTrack
    {
        /// <summary>
        /// Individual track's filename inside the archive contents
        /// </summary>
        public string InternalFileName { get; set; }

        /// <summary>
        /// Constructor for an internal archived file.
        /// </summary>
        /// TODO - flesh out a bit more, right now it just maintains a reference to the internal file name.
        /// <param name="archivePath">Path to archive</param>
        /// <param name="internalFilePath">Filename of track within the archive</param>
        public InternalMusicTrack(string archivePath, string internalFilePath) : base(archivePath)
        {
            InternalFileName = internalFilePath;
        }
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

        /// <summary>
        /// Set if this is a temporary file to be deleted.
        /// </summary>
        internal bool IsTempFile = false;

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
