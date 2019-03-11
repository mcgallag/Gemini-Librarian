using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemini_Librarian.Data
{
    /// <summary>
    /// A playlist of music files.
    /// </summary>
    class PlayList
    {
        /// <summary>
        /// Queue that holds the MusicTrack references.
        /// </summary>
        private readonly Queue<MusicTrack> Tracks = new Queue<MusicTrack>();

        /// <summary>
        /// Set to loop the playlist back to its start.
        /// </summary>
        public bool Loop = false;

        /// <summary>
        /// Public-facing array of PlayList contents.
        /// </summary>
        public MusicTrack[] Entries
        {
            get
            {
                return Tracks.ToArray();
            }
        }

        /// <summary>
        /// Number of entries in the PlayList.
        /// </summary>
        public int Count
        {
            get
            {
                return Tracks.Count;
            }
        }

        /// <summary>
        /// Adds a MusicTrack to the playlist.
        /// </summary>
        /// <param name="track">Track to add</param>
        public void AddTrack(MusicTrack track)
        {
            Tracks.Enqueue(track);
        }

        /// <summary>
        /// Retrieves next track in the playlist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If PlayList is empty</exception>
        public MusicTrack NextTrack()
        {
            MusicTrack track = Tracks.Dequeue();
            // HACK - if we're set to loop, requeue the entry at the end of list
            if (Loop)
                AddTrack(track);
            return track;
        }
    }
}
