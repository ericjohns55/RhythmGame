using System;
using System.Collections.Generic;

namespace MapGeneration {
    public class NoteBinner
    {
        // if this is true we want to generate the next note on the right, if false we want it on the left
        private static bool lastGeneratedOnLeft = true;

        // if any given position is more than this number of parsed MapEvents away from being used, we want to force it to be uesd
        private const int LAST_RECENTLY_USED_OVERRIDE = 3;

        // number of possible tile locations in the application
        private static int NUMBER_TILES = 8;

        // holds the number of MapEvents parsed since a given tile index was last used 
        private static Dictionary<int, int> lastUsed = new Dictionary<int, int>();

        // Generates the next index of the note to fall from the sky
        public static int GenerateNextNoteIndex() {
            // checks if we have a note that needs to be used before we randomly generate
            int overrideCheck = CheckForOverride();
            int generatedTileLocation;

            if (overrideCheck != -1) {
                generatedTileLocation = overrideCheck;
            } else {
                // if no note needed to be used (no least recently used) then we randomly generation a position
                generatedTileLocation = GenerateRandomTile();
            }

            // flip this variable so next note will be generated on the other side of the map
            lastGeneratedOnLeft = !lastGeneratedOnLeft;

            UpdateLastUsed(generatedTileLocation);

            return generatedTileLocation;
        }

        // checks if we have a note that has not been used in a while
        // if so we will return that value, if not we return -1 to flag that a randomly generated note is okay
        private static int CheckForOverride() {
            List<int> leastRecentlyUsedValues = new List<int>();

            // default to left side values
            int startingIndex = 0;
            int lastIndex = NUMBER_TILES / 2;

            // if the last note was generated on the left, we want to check the right side values
            if (lastGeneratedOnLeft) {
                startingIndex = NUMBER_TILES / 2;
                lastIndex = NUMBER_TILES;
            }

            // add the values from the side to be generated to a list
            for (int i = startingIndex; i < lastIndex; i++) {
                leastRecentlyUsedValues.Add(lastUsed[i]);
            }

            // sort the values and reverse so it is in descending order
            leastRecentlyUsedValues.Sort();
            leastRecentlyUsedValues.Reverse();

            // if the least recently used value is more than LAST_RECENTLY_USED_OVERRIDE, then we want
            // to force that value to be used next to ensure that random generation is not too skewed
            if (leastRecentlyUsedValues[0] - leastRecentlyUsedValues[1] >= LAST_RECENTLY_USED_OVERRIDE) {
                // find key that matches the value in leastRecentlyUsed[0]

                for (int i = startingIndex; i < lastIndex; i++) {
                    if (lastUsed[i] == leastRecentlyUsedValues[0]) {
                        return i;
                    }
                }
            }

            // if we do not need to force a value, return -1 to tell the generation function to create a random value
            return -1;
        }

        // resets the NoteBinner to the default state of nothing being rendered
        public static void Reset() {
            lastGeneratedOnLeft = true;

            // empties out the last used map
            lastUsed.Clear();

            // repopulates the last used map so every value starts at 0
            for (int i = 0; i < NUMBER_TILES; i++) {
                lastUsed.Add(i, 0);
            }
        }

        // updates the lastUsed map to reflect changes in the mostRecentlyUsedLocation
        private static void UpdateLastUsed(int mostRecentlyUsedLocation)
        {
            // default to left side of map
            int startingIndex = 0;
            int lastIndex = NUMBER_TILES / 2;

            // if last note generated was on left, use the right side of the map instead
            if (!lastGeneratedOnLeft) {
                startingIndex = NUMBER_TILES / 2;
                lastIndex = NUMBER_TILES;
            }

            // loop through the determined side of the map to update
            // we only want to update one side of the map at a time because we will always alternate values
            for (int i = startingIndex; i < lastIndex; i++) {
                // if we are not the most recently used location, add 1 to the number of map events since last rendered
                if (i != mostRecentlyUsedLocation) { 
                    lastUsed[i]++;
                } else {
                    // if we are the most recently used location, make the number of map events since last used 0
                    lastUsed[i] = 0;
                }
            }
        }

        // generates a random tile on the left or right side of the map depending on the lastGeneratedOnLeft value
        private static int GenerateRandomTile()
        {
            Random random = new Random();

            // if the last note was generated on left, we want to generate on the right
            if (lastGeneratedOnLeft) {
                return random.Next(NUMBER_TILES / 2, NUMBER_TILES);
            } else { // last note generated on right, get a random index on the left
                return random.Next(0, NUMBER_TILES / 2);
            }
        }

        // generates binned notes for a map
        public static int BinGeneratedMap(LinkedList<MapEvent> map, MapDifficulty difficulty) {
            int difficultyOffset = 0;

            if (difficulty == MapDifficulty.Easy || difficulty == MapDifficulty.Medium) {
                NUMBER_TILES = 8;
            } else {
                NUMBER_TILES = 6;
                difficultyOffset = 1;
            }

            // since we are generating a whole map, reset this class to default state
            Reset();

            LinkedListNode<MapEvent> current = map.First;
            int totalNoteCount = 0;

            // iterate over entire map
            while (current != null) {
                MapEvent mapEvent = current.Value;

                // get number of tiles to generate per map event, this is calculated in MapEvent class
                int numTilesToGenerate = mapEvent.GetNumberTilesToGenerate();

                if (!mapEvent.IsGhostNote()) {
                    totalNoteCount += numTilesToGenerate;
                }

                // generate number of tiles needed
                for (int i = 0; i < numTilesToGenerate; i++) {
                    mapEvent.AddTileToGenerate(GenerateNextNoteIndex() + difficultyOffset);
                }

                current = current.Next;
            }

            return totalNoteCount;
        }
    }
}
