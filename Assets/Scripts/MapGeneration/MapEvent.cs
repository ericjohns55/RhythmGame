using System;
using System.Collections.Generic;
using System.Text;
using Melanchall.DryWetMidi.Interaction;

namespace MapGeneration {
    public class MapEvent
    {
        // static lookup table for determining what user input to put a key on
        // pretty much bins the input based off note name; will be removed later
        private static Dictionary<Melanchall.DryWetMidi.MusicTheory.NoteName, int> noteLookupTable = new Dictionary<Melanchall.DryWetMidi.MusicTheory.NoteName, int>()
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

        // Timestamp of this moment of time in the MIDI
        private long timestamp;

        // Timestamp of the measure
        private double measureTick;

        // Current beat of the map event
        private double beatNumber;

        // Time signature of the current Map Event
        private TimeSignature timeSignature;

        // Notes that exist in this moment of time in the midi
        List<Note> notes = new List<Note>();

        // Constructor
        public MapEvent(long timestamp, short timeDivision, Tuple<TimeSignature, long> timeSignatureEvent) {
            this.timestamp = timestamp;

            timeSignature = timeSignatureEvent.Item1;

            long ticksSinceTimeSigChange = timestamp - timeSignatureEvent.Item2;
            double ticksPerBeat = timeDivision * (4.0 / timeSignature.Denominator); // will halve the tick per beat with 8th note denominators
            double measureLength = ticksPerBeat * timeSignature.Numerator;
            measureTick = ticksSinceTimeSigChange % measureLength; // get just the ticks in the current measure
            beatNumber = (measureTick / ticksPerBeat) + 1;

            UnityEngine.Debug.LogFormat("Parsed timestamp {0} at measure tick {1} [{2}] [time signature: {3}]", timestamp, measureTick, beatNumber, timeSignatureEvent.Item1);
        }

        // Returns timestamp 
        public long GetTimestamp() {
            return timestamp;
        }

        public double GetMeasureTick() {
            return measureTick;
        }

        public double GetBeatNumber() {
            return beatNumber;
        }

        public TimeSignature GetTimeSignature() {
            return timeSignature;
        }

        // Returns the number of notes being played in the midi at this time
        public int GetNoteCount() {
            return notes.Count;
        }

        // Adds a note to the current event
        public void AddNote(Note note) {
            notes.Add(note);
        }

        // Returns a string list of the notes in the current midi
        public string GetNoteList() {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (Note note in notes) {
                stringBuilder.Append(note.NoteName);
                stringBuilder.Append(note.Octave);
                stringBuilder.Append(" ");
            }

            // remove the extra space and replace Sharp with # for better readability
            return stringBuilder.ToString().Trim().Replace("Sharp", "#");
        }

        // Generates a list of indices to generate a tile for based on the notes
        // Currently the only omitted notes are duplicated note names, but this will change based off chart difficulty
        public List<int> GetTilesToGenerate() {
            List<int> tiles = new List<int>();

            foreach (Note note in notes) {
                int noteID;

                if (noteLookupTable.TryGetValue(note.NoteName, out noteID)) {
                    if (!tiles.Contains(noteID)) { // only add the note if it is not already there (no duplicating note names allowed)
                        tiles.Add(noteID);
                    }
                }
            }

            return tiles;
        }
    }
}