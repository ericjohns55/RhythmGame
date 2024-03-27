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

                MapEvent mapEvent = new MapEvent(timestamp, timeDivision, timeSignatureEvent);

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
            
            bool lastDownbeat = false;
            while (currentNode != null) { 
                MapEvent mapEvent = currentNode.Value;               
                TimeSignature timeSignature = mapEvent.GetTimeSignature();
                double measureTick = mapEvent.GetMeasureTick();

                bool beatParsed = false; // once we start looking at lots of patterns, it is possible there will be overlap. this will prevent that
                
                // if time sig denom is 8 and numerator % 3 is 0 (3/8, 6/8, 9/8, 12/8)
                // else we want just the odd numbers

                if (difficulty == MapDifficulty.Easy) { // consider X/8 time signatures 
                    if (timeSignature.Denominator <= 4) {
                        // allowing major beats and quarter note triplets
                        if (CompareBeat(measureTick, ValidRhythm.Downbeat, timeSignature)) {
                            Debug.LogFormat("Adding downbeat {0}", measureTick);
                            beatParsed = true;

                            // updates flag if there was a downbeat
                            if((measureTick == 0) || (measureTick % 320 == 0)){
                                lastDownbeat = false;
                            } else {
                                lastDownbeat = true;
                            }

                        } else if (!lastDownbeat && CompareBeat(measureTick, ValidRhythm.Quarter_Triplet, timeSignature)) {  
                            MapEvent nextEvent = currentNode.Next.Value;

                            // check if the next note is a downbeat
                            if((nextEvent != null) && (nextEvent.GetMeasureTick() % 320 == 0)) {                                                                         
                                Debug.LogFormat("Adding QTrip {0} nextTick {1}", measureTick,nextEvent.GetMeasureTick()); // NEXT PARSING SPRINT PROBLEM WOOHOO // HAHA fixed it
                                beatParsed = true;
                                lastDownbeat = false;   
                            }
                                                                                
                        } else if (CompareBeat(measureTick, ValidRhythm.Upbeat, timeSignature)) { // allow upbeats iff there is not a note on the downbeat
                            if (currentNode.Previous != null) {
                                double lastTick = currentNode.Previous.Value.GetMeasureTick();

                                if (Math.Abs(measureTick - lastTick) >= timeDivision) {
                                    Debug.LogFormat("Adding upbeat {0}", measureTick);
                                    beatParsed = true;
                                }
                            } else { // First note is an upbeat, we should play the first note
                                Debug.LogFormat("First note is an upbeat {0}", measureTick);
                                beatParsed = true;
                            }
                        }
                    } else {
                        double beat = mapEvent.GetBeatNumber();
                        if (timeSignature.Numerator % 3 == 0) {
                            if (CompareDoubles((beat - 1) % 3, 0) || CompareDoubles(beat, 1)) { // consider beats only divisible by 3
                                Debug.LogFormat("Adding beat {0} to 3/X time signature", beat);
                                beatParsed = true;
                            }
                        } else { 
                            if (timeSignature.Numerator % 2 == 0) {
                                if (CompareDoubles(beat % 2, 1)) { // consider only odd beats (1/3/5/etc)
                                    Debug.LogFormat("Adding beat {0} to even/8 time signature", beat);
                                    beatParsed = true;
                                }
                            } else { // 5/8 7/8 11/8
                                if (CompareDoubles(beat, 1) || (beat > 3 && CompareDoubles(beat % 2, 0))) { // use beat 1 always and even beats from [4, end) 
                                    Debug.LogFormat("Adding beat {0} to odd/8 time signature", beat);
                                    beatParsed = true;                                
                                }
                            }
                        }
                    }   

                                  
                } else {
                    if (CompareBeat(measureTick, ValidRhythm.Downbeat, timeSignature)) {
                        Debug.LogFormat("Allowing downbeat at {0}", measureTick);
                        beatParsed = true;
                    }

                    if (timeSignature.Denominator <= 4) {
                        if (CompareBeat(measureTick, ValidRhythm.Upbeat, timeSignature)) {
                            Debug.LogFormat("Allowing upbeat at {0}", measureTick);
                            beatParsed = true;
                        }

                        if (CompareBeat(measureTick, ValidRhythm.Sixteenth, timeSignature)) {
                            if (difficulty == MapDifficulty.Medium) {
                                if (currentNode.Previous != null) { // only allow sixteenth beats for dotted eighth - dotted eighth - eighth
                                    double lastTick = currentNode.Previous.Value.GetMeasureTick();

                                    if (Math.Abs(measureTick - lastTick) >= (timeDivision / 2)) {
                                        Debug.LogFormat("Adding sixteenth {0}", measureTick);
                                        beatParsed = true;
                                    }
                                }
                            } else { // hard mode 
                                Debug.LogFormat("Adding sixteenth {0}", measureTick); // allow all 16th notes in hard mode
                                beatParsed = true;
                            }
                        }

                        if (CompareBeat(measureTick, ValidRhythm.Eighth_Triplet, timeSignature)) {
                            Debug.LogFormat("Allowing eighth note triplet at {0}", measureTick);
                            beatParsed = true;
                        }
                    }

                    if (difficulty == MapDifficulty.Hard) {
                        if (CompareBeat(measureTick, ValidRhythm.Sixteenth, timeSignature)) {
                            Debug.LogFormat("Allowing sixteenth notes in non X/4 time signatures {0}", measureTick);
                            beatParsed = true;
                        }

                        if (CompareBeat(measureTick, ValidRhythm.Eighth_Triplet, timeSignature)) {
                            Debug.LogFormat("Allowing eighth note triplets in non X/4 time signatures {0}", measureTick);
                            beatParsed = true;
                        }

                        if (bpm <= 96) {
                            if (CompareBeat(measureTick, ValidRhythm.Sixteenth_Triplet, timeSignature)) { // allow 16th triplets on lower tempos
                                Debug.LogFormat("Allowing sixteenth triplet at {0}", measureTick);
                                beatParsed = true;
                            }

                            if (bpm <= 72) { // allow 32nds on slow scores
                                if (CompareBeat(measureTick, ValidRhythm.Thirty_Second, timeSignature)) {
                                    Debug.LogFormat("Allowing thirty-second at {0}", measureTick);
                                    beatParsed = true;
                                }
                            }
                        }
                    }
                }

                if (beatParsed) {
                    generatedMap.AddLast(mapEvent); 
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

        private bool CompareBeat(double beat, ValidRhythm rhythm, TimeSignature signature) {
            double tempDivision = timeDivision;

            if (signature.Denominator > 4) {
                tempDivision = timeDivision * (4.0 / signature.Denominator);
            }

            switch (rhythm) {
                case ValidRhythm.Downbeat:
                    return CompareDoubles(beat % tempDivision, 0);
                case ValidRhythm.Upbeat:
                    return CompareDoubles(beat % (tempDivision / 2), 0);
                case ValidRhythm.Sixteenth:
                    return CompareDoubles(beat % (tempDivision / 4), 0);
                case ValidRhythm.Thirty_Second:
                    return CompareDoubles(beat % (tempDivision / 8), 0);
                case ValidRhythm.Quarter_Triplet:
                    return CompareDoubles(beat % (tempDivision / 1.5), 0);
                case ValidRhythm.Eighth_Triplet:
                    return CompareDoubles(beat % (tempDivision / 3), 0);
                case ValidRhythm.Sixteenth_Triplet:
                    return CompareDoubles(beat % (tempDivision / 6), 0);
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
            return Math.Abs(double1 - double2) < 0.001;
        }
    }
}