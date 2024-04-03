using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

namespace MapGeneration {
    public class MeasureChunk {
        private long startingTimestamp;
        private long endingTimestamp;
        private long measureLength;
        private short timeDivision;
        private int bpm;

        private SortedDictionary<double, MapEvent> allMapEvents;
        private SortedDictionary<double, MapEvent> parsedEvents;

        // flag to say whether or not measures were parsed with a difficulty
        private bool chunkParsed = false;

        public MeasureChunk(long startingTimestamp, long endingTimestamp, short timeDivision, int bpm) {
            this.startingTimestamp = startingTimestamp;
            this.endingTimestamp = endingTimestamp;
            this.timeDivision = timeDivision;
            this.bpm = bpm;
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
            SortedDictionary<double, MapEvent>.ValueCollection values =
                 chunkParsed ? parsedEvents.Values : allMapEvents.Values;

            foreach (MapEvent mapEvent in values) {
                eventList.AddLast(mapEvent);
            }
        }

        public void Print() {
            Debug.LogFormat("[{0} - {1}]: {2} notes", startingTimestamp, endingTimestamp, GetNoteCount());
        }

        public void ParseMeasure(MapDifficulty difficulty) {
            parsedEvents = new SortedDictionary<double, MapEvent>();
            Debug.LogFormat("PARSING CHUNK {0} to {1}", startingTimestamp, endingTimestamp);
            chunkParsed = true;

            bool lastDownbeat = false;

            double[] measureTicks = allMapEvents.Keys.ToArray();

            for (int noteIndex = 0; noteIndex < measureTicks.Length; noteIndex++) {
                double timestamp = measureTicks[noteIndex];

                MapEvent mapEvent;

                if (allMapEvents.TryGetValue(timestamp, out mapEvent)) {
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
                                MapEvent nextEvent = getAdjacentEvent(noteIndex, true);

                                // check if the next note is a downbeat
                                if((nextEvent != null) && (nextEvent.GetMeasureTick() % 320 == 0)) {                                                                         
                                    Debug.LogFormat("Adding QTrip {0} nextTick {1}", measureTick,nextEvent.GetMeasureTick()); // NEXT PARSING SPRINT PROBLEM WOOHOO // HAHA fixed it
                                    beatParsed = true;
                                    lastDownbeat = false;   
                                }
                                                                                    
                            } else if (CompareBeat(measureTick, ValidRhythm.Upbeat, timeSignature)) { // allow upbeats iff there is not a note on the downbeat
                                MapEvent lastEvent = getAdjacentEvent(noteIndex, false);
                                
                                if (lastEvent != null) {
                                    if (Math.Abs(measureTick - lastEvent.GetMeasureTick()) >= timeDivision) {
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
                                if (MapGenerator.CompareDoubles((beat - 1) % 3, 0) || MapGenerator.CompareDoubles(beat, 1)) { // consider beats only divisible by 3
                                    Debug.LogFormat("Adding beat {0} to 3/X time signature", beat);
                                    beatParsed = true;
                                }
                            } else { 
                                if (timeSignature.Numerator % 2 == 0) {
                                    if (MapGenerator.CompareDoubles(beat % 2, 1)) { // consider only odd beats (1/3/5/etc)
                                        Debug.LogFormat("Adding beat {0} to even/8 time signature", beat);
                                        beatParsed = true;
                                    }
                                } else { // 5/8 7/8 11/8
                                    if (MapGenerator.CompareDoubles(beat, 1) || (beat > 3 && MapGenerator.CompareDoubles(beat % 2, 0))) { // use beat 1 always and even beats from [4, end) 
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
                                    MapEvent lastEvent = getAdjacentEvent(noteIndex, false);

                                    if (lastEvent != null) { // only allow sixteenth beats for dotted eighth - dotted eighth - eighth
                                        if (Math.Abs(measureTick - lastEvent.GetMeasureTick()) >= (timeDivision / 2)) {
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
                        parsedEvents.Add(mapEvent.GetTimestamp(), mapEvent);
                    }
                }
            }
        }

        private MapEvent getAdjacentEvent(int startingIndex, bool forwards) {
            int newIndex = forwards ? startingIndex + 1 : startingIndex - 1;

            if (newIndex >= 0 && newIndex < allMapEvents.Keys.Count) {
                MapEvent mapEvent;

                if (allMapEvents.TryGetValue(allMapEvents.Keys.ToArray()[newIndex], out mapEvent)) {
                    return mapEvent;
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
                    return MapGenerator.CompareDoubles(beat % tempDivision, 0);
                case ValidRhythm.Upbeat:
                    return MapGenerator.CompareDoubles(beat % (tempDivision / 2), 0);
                case ValidRhythm.Sixteenth:
                    return MapGenerator.CompareDoubles(beat % (tempDivision / 4), 0);
                case ValidRhythm.Thirty_Second:
                    return MapGenerator.CompareDoubles(beat % (tempDivision / 8), 0);
                case ValidRhythm.Quarter_Triplet:
                    return MapGenerator.CompareDoubles(beat % (tempDivision / 1.5), 0);
                case ValidRhythm.Eighth_Triplet:
                    return MapGenerator.CompareDoubles(beat % (tempDivision / 3), 0);
                case ValidRhythm.Sixteenth_Triplet:
                    return MapGenerator.CompareDoubles(beat % (tempDivision / 6), 0);
                default:
                    return false;
            }
        }
    }
}