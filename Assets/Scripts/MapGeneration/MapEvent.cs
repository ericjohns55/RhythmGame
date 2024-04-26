using System;
using System.Collections.Generic;
using System.Text;
using Melanchall.DryWetMidi.Interaction;

namespace MapGeneration {
    public class MapEvent
    {
        // Timestamp of this moment of time in the MIDI
        private long timestamp;

        // Timestamp of the measure
        private int measureTick;

        // Current beat of the map event
        private double beatNumber;

        // Time signature of the current Map Event
        private TimeSignature timeSignature;

        // Length of the measure this event sits in
        private double measureLength;

        // Notes that exist in this moment of time in the midi
        List<Note> notes = new List<Note>();

        // contains the indices of the tiles we want to generate on the map
        List<int> tilesToGenerate;

        // true if a map event should be a ghost note, defaulted to false otherwise
        private bool ghostNote = false;

        // Constructor
        public MapEvent(long timestamp, short timeDivision, Tuple<TimeSignature, long> timeSignatureEvent) {
            this.timestamp = timestamp;

            timeSignature = timeSignatureEvent.Item1;

            long ticksSinceTimeSigChange = timestamp - timeSignatureEvent.Item2;
            double ticksPerBeat = timeDivision * (4.0 / timeSignature.Denominator); // will halve the tick per beat with 8th note denominators
            measureLength = ticksPerBeat * timeSignature.Numerator;
            measureTick = (int) (ticksSinceTimeSigChange % measureLength); // get just the ticks in the current measure
            beatNumber = (measureTick / ticksPerBeat) + 1;

            tilesToGenerate = new List<int>();

            // UnityEngine.Debug.LogFormat("Parsed timestamp {0} at measure tick {1} [{2}] [time signature: {3}]", timestamp, measureTick, beatNumber, timeSignatureEvent.Item1);
        }

        // Returns timestamp 
        public long GetTimestamp() {
            return timestamp;
        }

        public int GetMeasureTick() {
            return measureTick;
        }

        public double GetBeatNumber() {
            return beatNumber;
        }

        public long GetMeasureLength() {
            return Convert.ToInt64(measureLength);
        }

        public TimeSignature GetTimeSignature() {
            return timeSignature;
        }
                
        public void SetGhostNote(bool ghostNote) {
            this.ghostNote = ghostNote;
        }

        public bool GetGhostNote() {
            return ghostNote;
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

        // returns the number of tiles we want to generate during playback
        public int GetNumberTilesToGenerate() {
            // ghost notes will never be in pairs
            if (ghostNote) {
                return 1;
            }

            // we never want the player to play more than two simultaneous notes
            // so if the score has two or less notes, make them play one, if more then let them play two at once
            // for piano pieces this will even out the bass clef, but if there are larger chords we will still get simultaneous input
            return notes.Count <= 2 ? 1 : 2;
        }

        // adds an index of a note to generate for this map event
        public void AddTileToGenerate(int tileID) {
            tilesToGenerate.Add(tileID);
        }

        // Generates a list of indices to generate a tile for based on the notes
        // Currently the only omitted notes are duplicated note names, but this will change based off chart difficulty
        public List<int> GetTilesToGenerate() {
            return tilesToGenerate;
        }
    }
}