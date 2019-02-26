using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemini_Librarian.Data
{
    class PlayList
    {
        private readonly List<MusicTrack> Tracks = new List<MusicTrack>();

        public MusicTrack[] Entries
        {
            get
            {
                return Tracks.ToArray();
            }
        }

        public int Count
        {
            get
            {
                return Tracks.Count;
            }
        }

        public void AddTrack(MusicTrack track)
        {
            Tracks.Add(track);
        }
    }
}
