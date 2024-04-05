using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

namespace MapGeneration {
    public class MeasureChunk {
        // timestamp the measure chunk begins on in the midi
        private long startingTimestamp;

        // timestamp the measure chunk ends on in the midi
        private long endingTimestamp;

        // length of the measure in ticks
        private int measureLength;

        // time division of the measure (480 is default)
        private short timeDivision;
        
        // tempo of measure
        private int bpm;

        // all map events present in the measure before parsing
        private SortedDictionary<int, MapEvent> allMapEvents;

        // all map events present in the measure after parsing
        private SortedDictionary<int, MapEvent> parsedEvents;

        // flag to say whether or not measures were parsed with a difficulty (easy/medium/hard)
        private bool chunkParsed = false;

        // ID of chunk, only set through a setter
        private int chunkID = -1;

        // will debug the parse mode if true, will not otherwise
        private bool DEBUG_PARSE = true;

        // Constructor which requires the measure timestamps, time division, and tempo of the measure
        public MeasureChunk(long startingTimestamp, long endingTimestamp, short timeDivision, int bpm) {
            this.startingTimestamp = startingTimestamp;
            this.endingTimestamp = endingTimestamp;
            this.timeDivision = timeDivision;
            this.bpm = bpm;
            measureLength = Convert.ToInt32(endingTimestamp - startingTimestamp);

            allMapEvents = new SortedDictionary<int, MapEvent>();
            parsedEvents = new SortedDictionary<int, MapEvent>();
        }

        // adds a map event to the measure based off measure tick 
        public void AddMapEvent(MapEvent mapEvent) {
            allMapEvents.Add(mapEvent.GetMeasureTick(), mapEvent);
            parsedEvents.Add(mapEvent.GetMeasureTick(), mapEvent);
        }

        // returns true of the time stamp exists within the current measure
        public bool IsValidMeasureTimestamp(long timestamp) {
            return timestamp >= startingTimestamp && timestamp < endingTimestamp;
        }

        // returns the number of notes in the measure
        public int GetNoteCount() {
            return allMapEvents.Count;
        }

        // sets a chunk ID
        public void SetChunkID(int chunkID) {
            this.chunkID = chunkID;
        }

        // adds all map events (parsed or unparsed) to the parameterized linked list
        public void AddToList(LinkedList<MapEvent> eventList) {
            SortedDictionary<int, MapEvent>.ValueCollection values =
                 chunkParsed ? parsedEvents.Values : allMapEvents.Values;

            foreach (MapEvent mapEvent in values) {
                eventList.AddLast(mapEvent);
            }
        }

        // debugs the measure chunk
        public void Print() {
            Debug.LogFormat("[{0} - {1}]: {2} notes", startingTimestamp, endingTimestamp, GetNoteCount());
        }

        // parses all map events based off the intended difficulty
        public void ParseMeasure(MapDifficulty difficulty) {
            // array of all measure ticks in the current measure
            int[] measureTicks = allMapEvents.Keys.ToArray();

            // iterates over measure ticks as a for loop instead of foreach so we can check previous and next measures
            for (int noteIndex = 0; noteIndex < measureTicks.Length; noteIndex++) {
                int timestamp = measureTicks[noteIndex];

                MapEvent mapEvent;

                // grabs map event from the current measure tick
                if (allMapEvents.TryGetValue(timestamp, out mapEvent)) {
                    TimeSignature timeSignature = mapEvent.GetTimeSignature();
                    int measureTick = mapEvent.GetMeasureTick();

                    bool removeEvent = false; // once we start looking at lots of patterns, it is possible there will be overlap. this will prevent that
                    
                    // if time sig denom is 8 and numerator % 3 is 0 (3/8, 6/8, 9/8, 12/8)
                    // else we want just the odd numbers

                    if (difficulty == MapDifficulty.Easy) { // consider X/8 time signatures 
                        if (timeSignature.Denominator <= 4) {

                            // for easy difficulty in 4/4, we want to allow all downbeats
                            if (!CompareBeat(measureTick, ValidRhythm.Downbeat, timeSignature)) {

                                // we want to remove all upbeats except for one specific edge case:
                                if (CompareBeat(measureTick, ValidRhythm.Upbeat, timeSignature)) {
                                    MapEvent lastEvent = GetAdjacentEvent(noteIndex, -1);

                                    if (lastEvent != null) {
                                        // if we have an upbeat, but it has been over a beat since the last note, we want to allow it
                                        // this will manifest in a dotted quarter - dotted quarter - quarter rhythm -> this should be valid so we will remove if not
                                        if (Math.Abs(measureTick - lastEvent.GetMeasureTick()) < timeDivision) {
                                            removeEvent = true;
                                        } else {
                                            if (DEBUG_PARSE) Debug.LogFormat("Valid Upbeat in EASY at {0} [CHUNK {1}]", measureTick, chunkID);
                                        }
                                    } else {
                                        removeEvent = true;
                                    }
                                }
                               
                                // we want to allow quarter note triplets, but only if they are in groups of three
                                else if (CompareBeat(measureTick, ValidRhythm.Quarter_Triplet, timeSignature)) {
                                    // for our purposes, a quarter note triplet can exist only within the first half of the measure or second, it cannot split it
                                    int singleQTripletLength = measureLength / 6; // 6 possible locations a quarter note triplet can land in for 4/4

                                    // there is a chance we may have a downbeat within the 
                                    bool checkRemoveOverlappingDownbeat = false;

                                    // this checks if we are in the second note of the triplet (for 4/4 this will be 320 or 1280)
                                    if (measureTick == singleQTripletLength || measureTick == 4 * singleQTripletLength) {
                                        // if we do not have all three pieces of the triplet we do not want it (so we check 1 note before and 1 note after)
                                        if (!allMapEvents.ContainsKey(measureTick - singleQTripletLength) || !allMapEvents.ContainsKey(measureTick + singleQTripletLength)) {
                                            removeEvent = true;
                                        } else {
                                            if (DEBUG_PARSE) Debug.LogFormat("Valid QTrip at {0} [CHUNK {1}]", measureTick, chunkID);

                                            // now that we have a valid quarter note triplet, we want to check if there is a downbeat in the middle and flag it for removal if so
                                            checkRemoveOverlappingDownbeat = true;
                                        }
                                    } 
                                    
                                    // the other scenario we want to check is if we are in the third note of the triplet (for 4/4 this will be 640 or 1600)
                                    else if (measureTick == singleQTripletLength * 2 || measureTick == singleQTripletLength * 5) {
                                        // once again checking if we have all 3 pieces, but now we want to check if we have the 2 earlier instead of one before and one after
                                        if (!allMapEvents.ContainsKey(measureTick - (2 * singleQTripletLength)) || !allMapEvents.ContainsKey(measureTick - singleQTripletLength)) {
                                            removeEvent = true;
                                        } else {
                                            if (DEBUG_PARSE) Debug.LogFormat("Valid QTrip at {0} [CHUNK {1}]", measureTick, chunkID);

                                            // this quarter triplet is also valid, check for a downbeat again
                                            checkRemoveOverlappingDownbeat = true;
                                        }
                                    }

                                    // in the scenario of a valid triplet, we want to check if there is a downbeat between the 2nd and 3rd notes
                                    if (checkRemoveOverlappingDownbeat) {
                                        // the first possibility for an overlapping quarter would be 1.5 * a triplet length
                                        int downbeatInTripletTimestamp = (int) (singleQTripletLength * 1.5);

                                        // if we are in the second half of the measure, we need to add half a measure length to adjust to those timestamps
                                        if (measureTick >= measureLength / 2) {
                                            downbeatInTripletTimestamp += measureLength / 2;
                                        }

                                        // notice how we are checking parsedEvents instead of allMapEvents like before, this ensures that we only remove it if it still exists 
                                        if (parsedEvents.ContainsKey(downbeatInTripletTimestamp)) {
                                            parsedEvents.Remove(downbeatInTripletTimestamp);

                                            if (DEBUG_PARSE) Debug.LogFormat("Downbeat overlapping with QTrip removed at {0} [CHUNK {1}]", downbeatInTripletTimestamp, chunkID);
                                        }
                                    }
                                }

                                // if we did not hit any of the earlier edge cases, then the MapEvent MUST be invalid and we want to mark it for removal
                                else {
                                    removeEvent = true;
                                }
                            }
                        } else if (timeSignature.Denominator <= 8) { // time signatures that do not give the quarter note the beat
                            int eighthNoteLength = timeDivision / 2;

                            if (timeSignature.Numerator % 3 == 0) { 
                                // for numerators divisible by 3, we only want every 3rd note
                                if (timestamp % (eighthNoteLength * 3) != 0) {
                                    removeEvent = true;
                                } else {
                                    if (DEBUG_PARSE) Debug.LogFormat("Allowing tick {0} in 3X/8 time signature [CHUNK {1}]", timestamp, chunkID);
                                }
                            } else if (timeSignature.Numerator % 2 == 0) {
                                // for efficiency i used timeDivision in this conditional instead of multiplying eighthNoteLength by 2 again
                                if (timestamp % timeDivision != 0) { // for even numerators NOT divisible by 3, we want every other note starting on the downbeat
                                    removeEvent = true;
                                } else {
                                    if (DEBUG_PARSE) Debug.LogFormat("Allowing tick {0} in EVEN/8 time signature [CHUNK {1}]", timestamp, chunkID);
                                }
                            } else {
                                // we always want to allow the first beat in the measure
                                if (timestamp != 0) { 
                                    if (timestamp >= eighthNoteLength * 3) {
                                        // once we are past beat 3, we only want to allow every even beat - i once again used timeDivision instead of eighthNoteLength for efficiency
                                        if ((timestamp - (eighthNoteLength * 3)) % timeDivision != 0) {
                                            removeEvent = true;
                                        } else {
                                            if (DEBUG_PARSE) Debug.LogFormat("Allowing tick {0} in ODD/8 time signature AFTER BEAT 3 [CHUNK {1}]", timestamp, chunkID);
                                        }
                                    } else {
                                        // for time signatures like 5/8, 7/8, 11/8, etc., the pattern we want is the first note, then every even beat afterwards 
                                        removeEvent = true;
                                    }
                                } else {
                                    if (DEBUG_PARSE) Debug.LogFormat("Allowing tick {0} in ODD/8 time signature [CHUNK {1}]", timestamp, chunkID);
                                }
                            }
                        } else {
                            if (timestamp != 0) { // for any time signatures with a sixteenth note denominator or lower, just allow the first beat so the map is not empty 
                                removeEvent = true;
                            }
                        }
                    }

                    // // not easy difficulty, must be medium or hard                                    
                    // } else {
                    //     // all downbeats are allowed in higher difficulties
                    //     if (CompareBeat(measureTick, ValidRhythm.Downbeat, timeSignature)) {
                    //         Debug.LogFormat("Allowing downbeat at {0}", measureTick);
                    //         removeEvent = true;
                    //     }

                    //     if (timeSignature.Denominator <= 4) {
                    //         if (CompareBeat(measureTick, ValidRhythm.Upbeat, timeSignature)) {
                    //             Debug.LogFormat("Allowing upbeat at {0}", measureTick);
                    //             removeEvent = true;
                    //         }

                    //         if (CompareBeat(measureTick, ValidRhythm.Sixteenth, timeSignature)) {
                    //             if (difficulty == MapDifficulty.Medium) {
                    //                 MapEvent lastEvent = GetAdjacentEvent(noteIndex, -1);

                    //                 if (lastEvent != null) { // only allow sixteenth beats for dotted eighth - dotted eighth - eighth
                    //                     if (Math.Abs(measureTick - lastEvent.GetMeasureTick()) >= (timeDivision / 2)) {
                    //                         Debug.LogFormat("Adding sixteenth {0}", measureTick);
                    //                         removeEvent = true;
                    //                     }
                    //                 }
                    //             } else { // hard mode 
                    //                 Debug.LogFormat("Adding sixteenth {0}", measureTick); // allow all 16th notes in hard mode
                    //                 removeEvent = true;
                    //             }
                    //         }

                    //         if (CompareBeat(measureTick, ValidRhythm.Eighth_Triplet, timeSignature)) {
                    //             Debug.LogFormat("Allowing eighth note triplet at {0}", measureTick);
                    //             removeEvent = true;
                    //         }
                    //     }

                    //     // hard difficulty is the most permissive
                    //     if (difficulty == MapDifficulty.Hard) {
                    //         if (CompareBeat(measureTick, ValidRhythm.Sixteenth, timeSignature)) {
                    //             Debug.LogFormat("Allowing sixteenth notes in non X/4 time signatures {0}", measureTick);
                    //             removeEvent = true;
                    //         }

                    //         if (CompareBeat(measureTick, ValidRhythm.Eighth_Triplet, timeSignature)) {
                    //             Debug.LogFormat("Allowing eighth note triplets in non X/4 time signatures {0}", measureTick);
                    //             removeEvent = true;
                    //         }

                    //         if (bpm <= 96) {
                    //             if (CompareBeat(measureTick, ValidRhythm.Sixteenth_Triplet, timeSignature)) { // allow 16th triplets on lower tempos
                    //                 Debug.LogFormat("Allowing sixteenth triplet at {0}", measureTick);
                    //                 removeEvent = true;
                    //             }

                    //             if (bpm <= 72) { // allow 32nds on slow scores
                    //                 if (CompareBeat(measureTick, ValidRhythm.Thirty_Second, timeSignature)) {
                    //                     Debug.LogFormat("Allowing thirty-second at {0}", measureTick);
                    //                     removeEvent = true;
                    //                 }
                    //             }
                    //         }
                    //     }
                    // }

                    if (removeEvent) {
                        if (DEBUG_PARSE) Debug.LogFormat("Removing timestamp {0} [CHUNK {1}]", timestamp, chunkID);
                        parsedEvents.Remove(mapEvent.GetMeasureTick());
                    }
                }
            }
            
            chunkParsed = true;
        }

        // returns adjacent MapEvents to the current startingIndex
        private MapEvent GetAdjacentEvent(int startingIndex, int offset) {
            int newIndex = startingIndex + offset;

            if (newIndex >= 0 && newIndex < allMapEvents.Keys.Count) {
                MapEvent mapEvent;

                if (allMapEvents.TryGetValue(allMapEvents.Keys.ToArray()[newIndex], out mapEvent)) {
                    return mapEvent;
                }
            }

            return null;
        }

        // compares whether the beat is a valid rhythm based off the time signature and time division
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