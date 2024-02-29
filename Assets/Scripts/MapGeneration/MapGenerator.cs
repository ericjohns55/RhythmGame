using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;


namespace MapGeneration {
    public class MapGenerator
    {
        // Number of ticks in a quarter note based off the library (480 is library default)
        private short timeDivision = 480;

        // Number of seconds in a quarter note based off the tempo
        private double secondsPerQuarterNote = 0;

        // Dictionary holding all of our time changes with timestamps
        private SortedDictionary<long, TimeSignature> timeChanges = new SortedDictionary<long, TimeSignature>();

        // List of map events; inputted in order of timestamp so the first element will be the first note and the last element will be the last note
        private LinkedList<MapEvent> mapEvents = new LinkedList<MapEvent>();

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
            int bpm = (int) Math.Round(60.0 / secondsPerQuarterNote, 0);

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

            // Parsing binned notes into MapEvents
            foreach (long timestamp in noteMap.Keys) {
                Tuple<TimeSignature, long> timeSignatureEvent = GetTimeSignatureAtTime(timestamp);

                // calculating beats
                long ticksSinceTimeSigChange = timestamp - timeSignatureEvent.Item2;
                double ticksPerBeat = timeDivision * (4.0 / timeSignatureEvent.Item1.Denominator); // will halve the tick per beat with 8th note denominators
                double measureLength = ticksPerBeat * timeSignatureEvent.Item1.Numerator;
                double tickOfEvent = ticksSinceTimeSigChange % measureLength; // get just the ticks in the current measure
                double beatNumber = Math.Round((tickOfEvent / ticksPerBeat) + 1, 2);

                MapEvent mapEvent = new MapEvent(timestamp, beatNumber); // create a new MapEvent for this timestamp

                Debug.LogFormat("Parsed timestamp {0} at beat {1} [time signature: {2}]", timestamp, beatNumber, timeSignatureEvent.Item1);

                List<Note> notes; 
                if (noteMap.TryGetValue(timestamp, out notes)) {
                    foreach (Note note in notes) {  // load the notes into our MapEvent for this timestamp
                        mapEvent.AddNote(note);
                    }
                }

                mapEvents.AddLast(mapEvent); // add to the end of the list to preserve ordering
            }
        }

        // Returns the LinkedList of MapEvents for parsing in the main game
        public LinkedList<MapEvent> GetMapEvents() {
            return mapEvents;
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

        // Debugs the time changes detected in the program
        private void PrintTimeChanges() {
            foreach (long timeValue in timeChanges.Keys) {
                TimeSignature timeSig;

                if (timeChanges.TryGetValue(timeValue, out timeSig)) {
                    Debug.LogFormat("Time change {0} found at timestamp {1}", timeSig, timeValue);
                }
            }
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

        // Debugs the LinkedList and shows current timestamp, next timestamp, and number of notes to be played
        private void DebugLinkedList() {
            for (LinkedListNode<MapEvent> node = mapEvents.First; node != null; node = node.Next) {
                long timestamp = node.Value.GetTimestamp();
                long nextTimestamp = node.Next != null ? node.Next.Value.GetTimestamp() : -1;

                Debug.LogFormat("Map event found at timestamp {0} (next: {1}) [{2} NOTES]", timestamp, nextTimestamp, node.Value.GetNoteCount());
            }
        }
    }
}