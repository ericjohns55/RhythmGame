using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

namespace MapGeneration {
    public class MeasureChunk {
        private long startingTimestamp;
        private long endingTimestamp;
        private long measureLength;

        private SortedDictionary<double, MapEvent> allMapEvents;
        private SortedDictionary<double, MapEvent> parsedEvents;

        // flag to say whether or not measures were parsed with a difficulty
        private bool chunkParsed = false;

        public MeasureChunk(long startingTimestamp, long endingTimestamp) {
            this.startingTimestamp = startingTimestamp;
            this.endingTimestamp = endingTimestamp;
            measureLength = endingTimestamp - startingTimestamp;

            allMapEvents = new SortedDictionary<double, MapEvent>();
        }

        public void AddMapEvent(MapEvent mapEvent) {
            allMapEvents.Add(mapEvent.GetMeasureTick(), mapEvent);
        }

        public bool IsValidMeasureTimestamp(long timestamp) {
            return timestamp >= startingTimestamp && timestamp < endingTimestamp;
        }

        public int GetNoteCount() {
            return allMapEvents.Count;
        }

        public void AddToList(LinkedList<MapEvent> eventList) {
            if (chunkParsed) {
                foreach (MapEvent mapEvent in parsedEvents.Values) {
                    eventList.AddLast(mapEvent);
                }
            } else {
                foreach (MapEvent mapEvent in allMapEvents.Values) {
                    eventList.AddLast(mapEvent);
                }
            }
        }

        public void Print() {
            Debug.LogFormat("[{0} - {1}]: {2} notes", startingTimestamp, endingTimestamp, GetNoteCount());
        }

        public void ParseMeasure(MapDifficulty difficulty) {
            // parsedEvents = new SortedDictionary<double, MapEvent>();
            Debug.LogFormat("PARSING CHUNK {0} to {1}", startingTimestamp, endingTimestamp);
            // chunkParsed = true;
        }
    }
}