using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

namespace MapGeneration {
    // Enum to hold what difficulty of the map to generate
    public enum MapDifficulty {
        Easy,
        Medium,
        Hard,
        FullMidi
    }

    // Enum to check a rhythm type
    public enum ValidRhythm {
        Downbeat,
        Upbeat,
        Sixteenth,
        Thirty_Second,
        Quarter_Triplet,
        Eighth_Triplet,
        Sixteenth_Triplet

    }

    public class MapGenerator
    {
        // Number of ticks in a quarter note based off the library (480 is library default)
        private short timeDivision = 480;

        // Number of seconds in a quarter note based off the tempo
        private double secondsPerQuarterNote = 0;

        // BPM of the MidiFile
        private int bpm = -1;

        // Dictionary holding all of our time changes with timestamps
        private SortedDictionary<long, TimeSignature> timeChanges = new SortedDictionary<long, TimeSignature>();

        // List of map events; inputted in order of timestamp so the first element will be the first note and the last element will be the last note
        // private LinkedList<MapEvent> mapEvents = new LinkedList<MapEvent>();
        
        private List<MeasureChunk> measureChunks;

        // Constructor, requires the midi file to parse
        public MapGenerator(MidiFile midiFile) {
            // Parse the time division of the MidiFile
            TimeDivision division = midiFile.TimeDivision;
            if (division.GetType() == typeof(TicksPerQuarterNoteTimeDivision)) { // this can be two types, but we only care about TicksPerQuarterNote
                timeDivision = ((TicksPerQuarterNoteTimeDivision) division).TicksPerQuarterNote;
                Debug.LogFormat("Found time division: {0} ticks per quarter note.", timeDivision);
            } else {
                Debug.LogFormat("Failed to calculate time division. Using the default of {0}.", timeDivision);
            }

            // Parsing time change events
            TempoMap tempoMap = midiFile.GetTempoMap();
            
            // grab the original time signature because .GetTimeSignatureChanges() will not pick up the original
            TimeSignature original = tempoMap.GetTimeSignatureAtTime(new MetricTimeSpan(0));
            timeChanges.Add(0, original);

            foreach (ValueChange<TimeSignature> timeEvent in tempoMap.GetTimeSignatureChanges()) {
                timeChanges.Add(timeEvent.Time, timeEvent.Value); // populate our map
            }

            Debug.LogFormat("Parsed {0} time change events.", timeChanges.Count);

            // Parsing tempo
            Tempo tempo = tempoMap.GetTempoAtTime(new MetricTimeSpan(0)); // we only care about the original tempo for now (tempo changes maybe supported later)
            secondsPerQuarterNote = tempo.MicrosecondsPerQuarterNote / 1000000.0; // MicrosecondsPerQuarterNote must be divided by 10^6 to get seconds

            // calculate the BPM based off our secondsPerQuarterNote
            // the library provides a function for this, but this way we ensure our calculations are correct when debugging
            bpm = (int) Math.Round(60.0 / secondsPerQuarterNote, 0);

            Debug.LogFormat("Found tempo {0} ({1} seconds per quarter note)", bpm, secondsPerQuarterNote);
        
            // Parsing notes
            // a SortedDictionary ensures that our note map will be in order of timestamps (helps when creating the LinkedList)
            SortedDictionary<long, List<Note>> noteMap = new SortedDictionary<long, List<Note>>();
            IEnumerable<Note> allNotes = midiFile.GetNotes();

            // Binning notes based off their timestamp
            foreach (Note note in allNotes) {
                long time = note.Time;

                List<Note> noteValues;
                if (!noteMap.TryGetValue(time, out noteValues)) { // if the current timestamp does not exist in the map yet, create a bin
                    noteValues = new List<Note>();
                    noteMap.Add(time, noteValues);
                }

                noteValues.Add(note);
            }

            // Parsing binned notes into MapEvents and MeasureChunks
            measureChunks = new List<MeasureChunk>();
            MeasureChunk currentMeasure = null;

            foreach (long timestamp in noteMap.Keys) {
                Tuple<TimeSignature, long> timeSignatureEvent = GetTimeSignatureAtTime(timestamp);

                MapEvent mapEvent = new MapEvent(timestamp, timeDivision, timeSignatureEvent);

                List<Note> notes; 
                if (noteMap.TryGetValue(timestamp, out notes)) {
                    foreach (Note note in notes) {  // load the notes into our MapEvent for this timestamp
                        mapEvent.AddNote(note);
                    }
                }

                // get length of measure for chunking
                long measureLength = mapEvent.GetMeasureLength();

                // if the measure tick is 0, then we must be at the beginning of a measure - make a new chunk
                if (CompareDoubles(mapEvent.GetMeasureTick(), 0)) {
                    if (currentMeasure != null) {
                        measureChunks.Add(currentMeasure);
                    }

                    currentMeasure = new MeasureChunk(timestamp, timestamp + measureLength, timeDivision, bpm);
                } else { // otherwise, if we have a null measure or the timestamp is not in the last chunk, make a new one
                    if (currentMeasure == null || !currentMeasure.IsValidMeasureTimestamp(timestamp)) {
                        if (currentMeasure != null) {
                            measureChunks.Add(currentMeasure);
                        }

                        // calculate the measure starting tick (even if absent) by offsetting the curent measure tick
                        long startingTick = timestamp - Convert.ToInt64(mapEvent.GetMeasureTick());
                        currentMeasure = new MeasureChunk(startingTick, startingTick + measureLength, timeDivision, bpm);
                    }
                }

                currentMeasure.AddMapEvent(mapEvent);
            }

            // add the final measure to the chunks list, this will not happen in the loop
            if (currentMeasure != null) {
                measureChunks.Add(currentMeasure);
            }

            // TODO: parse measure chunks into map events

            foreach (MeasureChunk chunk in measureChunks) {
                chunk.Print();
            }            
        }

        // Returns the LinkedList of MapEvents for parsing in the main game
        public LinkedList<MapEvent> GenerateMap(MapDifficulty difficulty) {
            LinkedList<MapEvent> generatedMap = new LinkedList<MapEvent>();

            // parse and generate map based off of all measure chunks
            foreach (MeasureChunk chunk in measureChunks) {
                if (difficulty != MapDifficulty.FullMidi) {
                    chunk.ParseMeasure(difficulty);
                }

                chunk.AddToList(generatedMap);
            }

            return generatedMap;
        }

        // returns number of notes in the current map
        public int GetNoteCount(LinkedList<MapEvent> map) {
            int noteCount = 0;

            LinkedListNode<MapEvent> node = map.First;

            while (node != null) {
                noteCount++;
                node = node.Next;
            }

            return noteCount;
        }

        // Returns how many seconds into the program you are based off the midi timestamp
        public float ConvertTickToSeconds(long tickTimestamp) {
            return (float) (tickTimestamp / timeDivision * secondsPerQuarterNote);
        }

        // Calculates how many real-life seconds are between two timestamps in the midi file
        public float CalculateNextTimeStamp(long originalTimestamp, long nextTimestamp) {
            long difference = nextTimestamp - originalTimestamp;
            double quarterNoteCount = difference / (double) timeDivision;
            return (float) (quarterNoteCount * secondsPerQuarterNote);
        }

        private Tuple<TimeSignature, long> GetTimeSignatureAtTime(long timestamp) {
            for (int i = 0; i < timeChanges.Count; i++) {
                if (i + 1 != timeChanges.Count) {
                    if (timestamp >= timeChanges.ElementAt(i).Key && timestamp < timeChanges.ElementAt(i + 1).Key) {
                        return Tuple.Create(timeChanges.ElementAt(i).Value, timeChanges.ElementAt(i).Key);
                    }
                } else {
                    return Tuple.Create(timeChanges.ElementAt(i).Value, timeChanges.ElementAt(i).Key);
                }
            }

            return null;
        }

        // Debugs the time changes detected in the program
        private void PrintTimeChanges() {
            foreach (long timeValue in timeChanges.Keys) {
                TimeSignature timeSig;

                if (timeChanges.TryGetValue(timeValue, out timeSig)) {
                    Debug.LogFormat("Time change {0} found at timestamp {1}", timeSig, timeValue);
                }
            }
        }

        // Debugs the LinkedList and shows current timestamp, next timestamp, and number of notes to be played
        // private void DebugLinkedList() {
        //     for (LinkedListNode<MapEvent> node = mapEvents.First; node != null; node = node.Next) {
        //         long timestamp = node.Value.GetTimestamp();
        //         long nextTimestamp = node.Next != null ? node.Next.Value.GetTimestamp() : -1;

        //         Debug.LogFormat("Map event found at timestamp {0} (next: {1}) [{2} NOTES]", timestamp, nextTimestamp, node.Value.GetNoteCount());
        //     }
        // }

        // Compares doubles with a tolerance
        // TODO: determine if this is really needed
        public static bool CompareDoubles(double double1, double double2) {
            return Math.Abs(double1 - double2) < 0.001;
        }
    }
}