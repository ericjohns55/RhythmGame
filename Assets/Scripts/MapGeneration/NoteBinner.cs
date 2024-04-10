using System;
using System.Collections;
using System.Collections.Generic;

namespace MapGeneration {
    public class NoteBinner
    {
        // static lookup table for determining what user input to put a key on
        // pretty much bins the input based off note name; will be removed later
        public static Dictionary<Melanchall.DryWetMidi.MusicTheory.NoteName, int> noteLookupTable = new Dictionary<Melanchall.DryWetMidi.MusicTheory.NoteName, int>()
        {
            { Melanchall.DryWetMidi.MusicTheory.NoteName.C, 0 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.CSharp, 0 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.D, 1 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.DSharp, 1 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.E, 2 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.F, 3 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.FSharp, 3 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.G, 4 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.GSharp, 4 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.A, 5 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.ASharp, 5 },
            { Melanchall.DryWetMidi.MusicTheory.NoteName.B, 6 }
        };

        private static bool lastGeneratedOnLeft = true;

        // if any given position is more than this number of parsed MapEvents away from being used, we want to force it to be uesd
        private const int LAST_RECENTLY_USED_OVERRIDE = 5;

        // number of possible tile locations in the application
        private const int NUMBER_TILES = 8;

        // holds the number of MapEvents parsed since a given tile index was last used 
        private static Dictionary<int, int> lastUsed = new Dictionary<int, int>();

        public static int GenerateNextNoteIndex() {
            int generatedTileLocation = 0;

            // TODO: check the lastUsed dictionary to see if any value is greater than LAST_RECENTLY_USED_OVERRIDE from the second value
            // we should probably do this with a sorted dictionary by copying values over as value, key 

            lastGeneratedOnLeft = !lastGeneratedOnLeft;

            UpdateLastUsed(generatedTileLocation);

            return generatedTileLocation;
        }

        public static void Reset() {
            lastGeneratedOnLeft = true;

            lastUsed.Clear();

            for (int i = 0; i < NUMBER_TILES; i++) {
                lastUsed.Add(i, 0);
            }
        }

        private static void UpdateLastUsed(int mostRecentlyUsedLocation) {
            for (int i = 0; i < NUMBER_TILES; i++) {
                if (i != mostRecentlyUsedLocation) {
                    lastUsed[i]++;
                } else {
                    lastUsed[i] = 0;
                }
            }
        }

        private static int GenerateRandomTile() {
            Random random = new Random();
            
            // if the last note was generated on left, we want to generate on the right
            if (lastGeneratedOnLeft) {
                return random.Next(4, 8);
            } else { // last note generated on right, get a random index on the left
                return random.Next(0, 4);
            }
        }
    }
}
