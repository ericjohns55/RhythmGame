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

        // time signature of measure chunk
        private TimeSignature timeSignature;

        // ID of chunk, only set through a setter
        private int chunkID = -1;

        // will debug the parse mode if true, will not otherwise
        private bool DEBUG_PARSE = false;

        // Constructor which requires the measure timestamps, time division, and tempo of the measure
        public MeasureChunk(long startingTimestamp, long endingTimestamp, short timeDivision, int bpm, TimeSignature timeSignature) {
            this.startingTimestamp = startingTimestamp;
            this.endingTimestamp = endingTimestamp;
            this.timeDivision = timeDivision;
            this.timeSignature = timeSignature;
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

        // debugs the measure chunk in terms of notes
        public void Print() {
            Debug.LogFormat("[{0} - {1}]: {2} notes", startingTimestamp, endingTimestamp, GetNoteCount());
        }

        // prints out all map events in the chunk
        public void PrintGeneratedMap() {
            foreach (int timestamp in parsedEvents.Keys) {
                Debug.LogFormat("Timestamp {0} [CHUNK {1}]", parsedEvents[timestamp].GetMeasureTick(), chunkID);
            }
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
                    int measureTick = mapEvent.GetMeasureTick();

                    bool removeEvent = false; // once we start looking at lots of patterns, it is possible there will be overlap. this will prevent that
                    
                    // if time sig denom is 8 and numerator % 3 is 0 (3/8, 6/8, 9/8, 12/8)
                    // else we want just the odd numbers

                    if (difficulty == MapDifficulty.Easy) { // consider X/8 time signatures 
                        if (timeSignature.Denominator <= 4) {

                            // for easy difficulty in 4/4, we want to allow all downbeats
                            if (!CompareBeat(measureTick, ValidRhythm.Downbeat)) {

                                // we want to remove all upbeats except for one specific edge case:
                                if (CompareBeat(measureTick, ValidRhythm.Upbeat)) {
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
                                else if (CompareBeat(measureTick, ValidRhythm.Quarter_Triplet)) {
                                    // for our purposes, a quarter note triplet can exist only within the first half of the measure or second, it cannot split it
                                    int singleQTripletLength = measureLength / 6; // 6 possible locations a quarter note triplet can land in for 4/4

                                    // there is a chance we may have a downbeat within the 
                                    bool checkRemoveOverlappingDownbeat = false;

                                    // this checks if we are in the second note of the triplet (for 4/4 this will be 320 or 1280)
                                    if (measureTick == singleQTripletLength || measureTick == 4 * singleQTripletLength) {
                                        // if we do not have all three pieces of the triplet we do not want it (so we check 1 note before and 1 note after)
                                        if (!allMapEvents.ContainsKey(measureTick - singleQTripletLength) || !allMapEvents.ContainsKey(measureTick + singleQTripletLength)) {
                                            // for easy mode parsing we could possibly have a half note triplet though, so check for that before we commit to removing the event
                                            // since we are checking if we are in the second note of a triplet, this could only be the third note of the half note triplet
                                            if (!allMapEvents.ContainsKey(measureTick - (2 * singleQTripletLength)) || !allMapEvents.ContainsKey(measureTick - (4 * singleQTripletLength))) {
                                                removeEvent = true;
                                            } else {
                                                if (DEBUG_PARSE) Debug.LogFormat("Valid HalfTrip at {0} [CHUNK {1}]", measureTick, chunkID);
                                            }
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
                                            // it is once again possible that we have a half note triplet; since the tick could be 640 or 1600, it would have to be the second note in the half note triplet
                                            if (!allMapEvents.ContainsKey(measureTick - (2 * singleQTripletLength)) || !allMapEvents.ContainsKey(measureTick + (2 * singleQTripletLength))) {
                                                removeEvent = true;
                                            } else {
                                                if (DEBUG_PARSE) Debug.LogFormat("Valid HalfTrip at {0} [CHUNK {1}]", measureTick, chunkID);
                                            }
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
                    } else if (difficulty == MapDifficulty.Medium) {
                        // for medium difficulty, all downbeats are valid
                        if (!CompareBeat(measureTick, ValidRhythm.Downbeat)) {
                            // for time signatures with a 4 or lower time denominator, we want to allow all upbeats and eighth note triplets
                            if (timeSignature.Denominator <= 4) {
                                if (!CompareBeat(measureTick, ValidRhythm.Upbeat)) {                                    
                                    // we want to remove all sixteenth notes except for one specific edge case:
                                    if (CompareBeat(measureTick, ValidRhythm.Sixteenth)) {
                                        MapEvent lastEvent = GetAdjacentEvent(noteIndex, -1);

                                        if (lastEvent != null) {
                                            // if we have a sixteenth, but it has been over half a beat since the last note, we want to allow it
                                            // this will manifest in a dotted eighth - dotted eighth - eighth rhythm -> this should be valid so we will remove if not
                                            // we divide the timeDivision by two so it counts as half of a beat instead of twice a beat
                                            if (Math.Abs(measureTick - lastEvent.GetMeasureTick()) < timeDivision / 2) {
                                                removeEvent = true;
                                            } else {
                                                if (DEBUG_PARSE) Debug.LogFormat("Valid Sixteenth in MEDIUM at {0} [CHUNK {1}]", measureTick, chunkID);
                                            }
                                        } else {
                                            removeEvent = true;
                                        }
                                    } else if (CompareBeat(measureTick, ValidRhythm.Eighth_Triplet)) { // TODO: consider tempo here (especially for testing ShortSong1)
                                        // we split the eighth note triplet check into a helper method because it will be repeated in the hard mode parsing
                                        if (!CheckValidEighthTriplet(measureTick)) {
                                            removeEvent = true;
                                        }
                                    } else {
                                        // we are not an upbeat, eighth triplet, or sixteenth -> remove.
                                        removeEvent = true;
                                    }
                                }
                            } else {
                                // if the time signature is greater than 4 (8, 16, etc.) we only want to allow downbeats since theyre twice as fast 
                                removeEvent = true;
                            }
                        }
                    } else { // not easy or medium difficulty, must be hard
                        // allow downbeats and upbeats regardless of time signature
                        if (!CompareBeat(measureTick, ValidRhythm.Downbeat) && !CompareBeat(measureTick, ValidRhythm.Upbeat)) {
                            // hard mode parsing is essentially all about allowing complex rhythms depending on tempo
                            // the tempos will seem arbitrarily, but i tested them with a metronome until i felt they were reasonable

                            if (CompareBeat(measureTick, ValidRhythm.Sixteenth)) {
                                // faster than 132 bpm is too fast to be reasonable on a keyboard for sixteenths
                                if (bpm > 132) {
                                    removeEvent = true;
                                }
                            }

                            // only allow sixteenth note triplets in X/4 or X/2 time signatures because they are too fast regardless of tempo otherwise
                            else if (CompareBeat(measureTick, ValidRhythm.Sixteenth_Triplet)) {
                                // faster than 96 bpm is too fast to be reasonable
                                if (bpm > 96) {
                                    // sixteenth triplet check will envelope eighth note
                                    // if we are too fast for a sixteenth, we want to see if there is a valid eighth triplet we can use instead
                                    if (CompareBeat(measureTick, ValidRhythm.Eighth_Triplet)) {
                                        // if we do not have a valid eighth triplet, remove; if so, the overlapping upbeat will be removed
                                        if (!CheckValidEighthTriplet(measureTick)) { 
                                            removeEvent = true;
                                        } else {
                                            // here we want to start removing sixteenth note triplets for time signatures where eighth note or faster gets the beat 
                                            if (timeSignature.Denominator > 4) {
                                                // to do so we grab the next map event and make sure there is more than a sixteenth note triplet length between them
                                                MapEvent nextEvent = GetAdjacentEvent(noteIndex, 1);

                                                if (nextEvent != null) {
                                                    // there are 6 places for a sixteenth triplet to be, we want to remove this event if the time is less than or equal to a sixteenth triplet length
                                                    if (Math.Abs(measureTick - nextEvent.GetMeasureTick()) <= timeDivision / 6) {
                                                        removeEvent = true;
                                                    } else {
                                                        if (DEBUG_PARSE) Debug.LogFormat("Valid eighth note triplet in HARD at {0} [CHUNK {1}]", measureTick, chunkID);
                                                    }
                                                } else {
                                                    removeEvent = true;
                                                }
                                            }
                                        }
                                    } else {
                                        removeEvent = true;
                                    }
                                }
                            }

                            // we only want to allow 32nd notes in the case of a X/4 or X/2 time signature, too unreasonable otherwise
                            else if (CompareBeat(measureTick, ValidRhythm.Thirty_Second) && timeSignature.Denominator <= 4) {
                                // please note this is not exactly half of the 132 bpm allowed for sixteenths, 72bpm is just a more standard tempo
                                if (bpm > 72) {
                                    // here we check if we have a dotted sixteenth - dotted sixteenth - sixteenth rhythm, we want to allow this specific pattern
                                    MapEvent lastEvent = GetAdjacentEvent(noteIndex, -1);

                                    if (lastEvent != null) {
                                        // if we have an upbeat, but it has been over a beat since the last note, we want to allow it
                                        // this will manifest in a dotted quarter - dotted quarter - quarter rhythm -> this should be valid so we will remove if not
                                        // we divide the timeDivision by two so it counts as quarter of a beat instead of four times a beat
                                        if (Math.Abs(measureTick - lastEvent.GetMeasureTick()) < timeDivision / 4) {
                                            removeEvent = true;
                                        } else {
                                            if (DEBUG_PARSE) Debug.LogFormat("Valid thirty second in HARD at {0} [CHUNK {1}]", measureTick, chunkID);
                                        }
                                    } else {
                                        removeEvent = true;
                                    }
                                }
                            }

                            // remove any rhythm not covered by the other cases
                            else {
                                removeEvent = true;
                            }
                        }
                    }

                    if (removeEvent) {
                        if (DEBUG_PARSE) Debug.LogFormat("Removing timestamp {0} [CHUNK {1}]", timestamp, chunkID);
                        parsedEvents.Remove(mapEvent.GetMeasureTick());
                    }
                }
            }
            
            chunkParsed = true;
        }

        private bool CheckValidEighthTriplet(int measureTick) {
            // we want to allow eighth note triplets, but if an upbeat divides them we have to account for it, here we check these cases
            int singleEighthTripLength = measureLength / 12; // 12 possible locations

            // if an upbeat divides an eighth triplet, we will want to remove it 
            bool checkRemoveOverlappingUpbeat = false;
            int overlappingUpbeatTimestamp = -1;

            // here we are checking if we are in the second position of an eighth note triplet
            // we offset the measureTick by an eighth trip length to see if it would land on a downbeat for any measure
            if ((measureTick - singleEighthTripLength) % timeDivision == 0) {
                if (!allMapEvents.ContainsKey(measureTick - singleEighthTripLength) || !allMapEvents.ContainsKey(measureTick + singleEighthTripLength)) {
                    // we do not have a valid eighth note triplet, however it is possible we may have a valid quarter note triplet so we want to check for that too
                    // since we are in the second position of an eighth triplet, we want to check four positions and two positions backwards
                    if (!allMapEvents.ContainsKey(measureTick - (4 * singleEighthTripLength)) || !allMapEvents.ContainsKey(measureTick - (2 * singleEighthTripLength))) {
                        // now that we do not have an eighth or quarter note triplet, we need to last check if we have a half note triplet
                        // we are in the second position of a quarter note triplet, which means for a half note triplet it is only possible to be the second note
                        // a half note triplet consists of 3 half notes each equating to the length of 4 single eighth note triplets, so we check the timestamps of 4 trips before and after
                        if (!allMapEvents.ContainsKey(measureTick - (4 * singleEighthTripLength)) || !allMapEvents.ContainsKey(measureTick + (4 * singleEighthTripLength))) {
                            return false;
                        } else {
                            if (DEBUG_PARSE) Debug.LogFormat("Valid HalfTrip at {0} [CHUNK] {1}]", measureTick, chunkID);
                        }
                    }
                } else {
                    if (DEBUG_PARSE) Debug.LogFormat("Valid EighthTrip at {0} [CHUNK {1}]", measureTick, chunkID);

                    // we now know that we have a valid eighth triplet, so we want to check for a conflicting upbeat and flag for removal if so
                    checkRemoveOverlappingUpbeat = true;

                    // we know that we we are in the second position of an eighth note triplet, so we offset to get to the downbeat,
                    // then add half of the time division to calculate the upbeat location
                    overlappingUpbeatTimestamp = (measureTick - singleEighthTripLength) + (timeDivision / 2);
                }
            }

            // the other scenario we have to check is if we are in the third position in an eighth note triplet, now we offset by singleEighthTripLength * 2 to calculate it
            else if ((measureTick - (2 * singleEighthTripLength)) % timeDivision == 0) {
                // once again checking if all pieces of the triplet exist and marking for removal if not
                if (!allMapEvents.ContainsKey(measureTick - (2 * singleEighthTripLength)) || !allMapEvents.ContainsKey(measureTick - singleEighthTripLength)) {
                    // we do not have a valid eighth note triplet, but it is once again possible we have a quarter note triplet
                    // since we are in the third position of an eighth triplet, we want to check two positions backwards and two positions forwards
                    if (!allMapEvents.ContainsKey(measureTick - (2 * singleEighthTripLength)) || !allMapEvents.ContainsKey(measureTick + (2 * singleEighthTripLength))) {
                        // no valid quarter trip, check if we have a valid half note triplet
                        // we are in the third position of an eighth trip, which means that we could only be the third note in the half note triplet
                        // since a half note triplet has 3 half notes equating to 4 single eighth note triplets, we check 4 positions back and 8 positions back
                        if (!allMapEvents.ContainsKey(measureTick - (4 * singleEighthTripLength)) || !allMapEvents.ContainsKey(measureTick - (8 * singleEighthTripLength))) {
                            return false;
                        } else {
                            if (DEBUG_PARSE) Debug.LogFormat("Valid HalfTrip at {0} [CHUNK] {1}]", measureTick, chunkID);
                        }
                    }
                } else {
                    if (DEBUG_PARSE) Debug.LogFormat("Valid EighthTrip at {0} [CHUNK {1}]", measureTick, chunkID);

                    // another valid eighth triplet, check for a downbeat again
                    checkRemoveOverlappingUpbeat = true;

                    // we know that we we are in the third position of an eighth note triplet, so we offset twice to get to the downbeat,
                    // then add half of the time division to calculate the upbeat location
                    overlappingUpbeatTimestamp = (measureTick - (2 * singleEighthTripLength)) + (timeDivision / 2);
                }
            }

            // if true we have a valid eighth note triplet, so we need to see if theres an upbeat that divides it and remove if so
            if (checkRemoveOverlappingUpbeat && overlappingUpbeatTimestamp != -1) {
                // check if the upbeat already made it through the parse and remove if so
                if (parsedEvents.ContainsKey(overlappingUpbeatTimestamp)) {
                    parsedEvents.Remove(overlappingUpbeatTimestamp);

                    if (DEBUG_PARSE) Debug.LogFormat("Upbeat overlapping with EighthTrip removed at {0} [CHUNK {1}]", overlappingUpbeatTimestamp, chunkID);
                }
            }

            return true;
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
        private bool CompareBeat(double beat, ValidRhythm rhythm) {
            double tempDivision = timeDivision;

            if (timeSignature.Denominator > 4) {
                tempDivision = timeDivision * (4.0 / timeSignature.Denominator);
            }

            // only use tempDivision for Downbeat and Upbeat, we want to check every other rhythm like normal for the rest
            switch (rhythm) {
                case ValidRhythm.Downbeat:
                    return MapGenerator.CompareDoubles(beat % tempDivision, 0);
                case ValidRhythm.Upbeat:
                    return MapGenerator.CompareDoubles(beat % (tempDivision / 2), 0);
                case ValidRhythm.Sixteenth:
                    return MapGenerator.CompareDoubles(beat % (timeDivision / 4), 0);
                case ValidRhythm.Thirty_Second:
                    return MapGenerator.CompareDoubles(beat % (timeDivision / 8), 0);
                case ValidRhythm.Quarter_Triplet:
                    return MapGenerator.CompareDoubles(beat % (timeDivision / 1.5), 0);
                case ValidRhythm.Eighth_Triplet:
                    return MapGenerator.CompareDoubles(beat % (timeDivision / 3), 0);
                case ValidRhythm.Sixteenth_Triplet:
                    return MapGenerator.CompareDoubles(beat % (timeDivision / 6), 0);
                default:
                    return false;
            }
        }
    }
}