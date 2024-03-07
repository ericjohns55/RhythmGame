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

            // Parsing binned notes into MapEvents
            foreach (long timestamp in noteMap.Keys) {
                Tuple<TimeSignature, long> timeSignatureEvent = GetTimeSignatureAtTime(timestamp);
                TimeSignature timeSignature = timeSignatureEvent.Item1;

                // calculating beats
                long ticksSinceTimeSigChange = timestamp - timeSignatureEvent.Item2;
                double ticksPerBeat = timeDivision * (4.0 / timeSignature.Denominator); // will halve the tick per beat with 8th note denominators
                double measureLength = ticksPerBeat * timeSignature.Numerator;
                double tickOfEvent = ticksSinceTimeSigChange % measureLength; // get just the ticks in the current measure
                double beatNumber = (tickOfEvent / ticksPerBeat) + 1;

                MapEvent mapEvent = new MapEvent(timestamp, beatNumber, timeSignature); // create a new MapEvent for this timestamp

                // Debug.LogFormat("Parsed timestamp {0} at beat {1} [time signature: {2}]", timestamp, Math.Round(beatNumber, 5), timeSignatureEvent.Item1);

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
        public LinkedList<MapEvent> GenerateMap(MapDifficulty difficulty) {
            if (difficulty == MapDifficulty.FullMidi) {
                return mapEvents;
            }

            LinkedList<MapEvent> generatedMap = new LinkedList<MapEvent>();

            LinkedListNode<MapEvent> currentNode = mapEvents.First;
            while (currentNode != null) { 
                MapEvent mapEvent = currentNode.Value;               
                TimeSignature timeSignature = mapEvent.GetTimeSignature();
                double beat = mapEvent.GetBeatNumber();

                // if time sig denom is 8 and numerator % 3 is 0 (3/8, 6/8, 9/8, 12/8)
                // else we want just the odd numbers

                if (difficulty == MapDifficulty.Easy) { // consider X/8 time signatures 
                    if (timeSignature.Denominator <= 4) {
                        // allowing major beats and quarter note triplets
                        if (CompareBeat(beat, ValidRhythm.Downbeat)) {
                            Debug.LogFormat("Adding downbeat and triplet {0}", beat);
                            generatedMap.AddLast(mapEvent);
                        }

                        if (CompareBeat(beat, ValidRhythm.Quarter_Triplet)) {
                            Debug.LogFormat("TODO"); // NEXT PARSING SPRINT PROBLEM WOOHOO
                        }

                        // allow upbeats iff there is not a note on the downbeat
                        if (CompareBeat(beat, ValidRhythm.Upbeat)) {
                            if (currentNode.Previous != null) {
                                double lastBeat = currentNode.Previous.Value.GetBeatNumber();

                                if (Math.Abs(beat - lastBeat) >= 1.0) {
                                    Debug.LogFormat("Adding upbeat {0}", beat);
                                    generatedMap.AddLast(mapEvent);
                                }
                            } else { // First note is an upbeat, we should play the first note
                                Debug.LogFormat("First note is an upbeat {0}", beat);
                                generatedMap.AddLast(mapEvent);
                            }
                        }
                    } else {
                        if (timeSignature.Numerator % 3 == 0) {
                            if (CompareDoubles((beat - 1) % 3, 0) || CompareDoubles(beat, 1)) { // consider beats only divisible by 3
                                Debug.LogFormat("Adding beat {0} to 3/X time signature", beat);
                                generatedMap.AddLast(mapEvent);
                            }
                        } else { 
                            if (timeSignature.Numerator % 2 == 0) {
                                if (CompareDoubles(beat % 2, 1)) { // consider only odd beats (1/3/5/etc)
                                    Debug.LogFormat("Adding beat {0} to even/8 time signature", beat);
                                    generatedMap.AddLast(mapEvent);
                                }
                            } else { // 5/8 7/8 11/8
                                if (CompareDoubles(beat, 1) || (beat > 3 && CompareDoubles(beat % 2, 0))) { // use beat 1 always and even beats from [4, end) 
                                    Debug.LogFormat("Adding beat {0} to odd/8 time signature", beat);
                                    generatedMap.AddLast(mapEvent);
                                
                                }
                            }
                        }
                    }                    
                } else if (difficulty == MapDifficulty.Medium) {
                    if (timeSignature.Denominator <= 4) {
                        // Allow all downbeats and upbeats

                        
                        // TODO: Allow quarter note triplets

                        if (bpm < 132) {
                            // Allow eighth note triplets
                        }
                    } else {
                        // Allow all downbeats
                        // Allow 8th note triplets
                    } 
                } else if (difficulty == MapDifficulty.Hard) {
                    // Allow all 16th notes

                    if (bpm <= 96) {
                        // Allow all sixteenth note triplets

                        if (bpm <= 72) {
                            // Allow all 32nd notes
                        }
                    }
                        
                }

                currentNode = currentNode.Next;
            }

            return generatedMap;
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

        private bool CompareBeat(double beat, ValidRhythm rhythm) {
            switch (rhythm) {
                case ValidRhythm.Downbeat:
                    return CompareDoubles(beat % 1.0, 0);
                case ValidRhythm.Upbeat:
                    return CompareDoubles(beat % 0.5, 0) && !CompareDoubles(beat % 1.0, 0);
                case ValidRhythm.Sixteenth:
                    return CompareDoubles(beat % 0.25, 0) && !CompareDoubles(beat % 0.5, 0);
                case ValidRhythm.Thirty_Second:
                    return CompareDoubles(beat % 0.125, 0) && !CompareDoubles(beat % 0.25, 0);
                case ValidRhythm.Quarter_Triplet:
                    double value = (Math.Floor(beat) % 2) + (beat - Math.Floor(beat)) - 1;
                    if (value < 0) {
                        value = Math.Abs(value) * 2;
                    }

                    return CompareDoubles(value % (2.0/3.0), 0) || CompareDoubles(value % (2.0/3.0), 2.0/3.0);
                case ValidRhythm.Eighth_Triplet:
                    return CompareDoubles(beat % (1.0/3.0), 0);
                case ValidRhythm.Sixteenth_Triplet:
                    return CompareDoubles(beat % (1.0/6.0), 0);
                default:
                    return false;
            }
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
        private void DebugLinkedList() {
            for (LinkedListNode<MapEvent> node = mapEvents.First; node != null; node = node.Next) {
                long timestamp = node.Value.GetTimestamp();
                long nextTimestamp = node.Next != null ? node.Next.Value.GetTimestamp() : -1;

                Debug.LogFormat("Map event found at timestamp {0} (next: {1}) [{2} NOTES]", timestamp, nextTimestamp, node.Value.GetNoteCount());
            }
        }

        // Compares doubles with a tolerance
        // TODO: determine if this is really needed
        private bool CompareDoubles(double double1, double double2) {
            return Math.Abs(double1 - double2) < 0.0001;
        }
    }
}