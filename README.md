# Rhythm Game
A project for Computer Science 480
Developers: Eric Johns, Juan Aleman IV, Calvin Slusser, Vanessa Brouse, Zachary Hunter, Carlos I. Oquendo-Pagan

### **Background**

The Rhythm Game will be developed with Unity using C# as our development language. This will allow the game to be run on any platform, including Windows, Mac, Linux, or even an Android tablet. The game will be played by attempting to hit specific buttons on the keyboard in time with notes falling from the top of the screen. Players will be scored based on timing and the correct keys pressed.

The Fast Fourier transform algorithm may be implemented to determine the offset between the user device’s audio output and the processing of the arbitrary, user supplied MIDI data which will aid in the proper rendering of notes on the screen.

Since this game will be a new piece of software, no prior work was completed, and we will be starting at square zero implementing all features from the ground up.

### **Requirements**

The user will be able to play the game on any computer that has a keyboard. The keyboard will be used as the input method for the game, and there will be 8 buttons corresponding to any eight positions that the notes could fall down from.

The user will provide a song in MIDI format to the application which will then generate a map and notes for the level taking into account three different difficulty types - easy, medium, and hard.

The game will also support a local co-op mode, where two players can use a singular keyboard and compete against each other for a higher score. The scores will be determined based on correctness of inputs. The player with the highest score will be declared the winner.

The application will be required to account for input latency between the keyboard and the audio and visual of the game. This means that when the player sees a note hit the bottom of the screen, they should be able to hit the keyboard in time without the game telling them they are wrong if there is a bit of input lag. The game will also account for keyboard ghosting, and ensure that there will not be more notes on the map than a keyboard can process at any given time. This will mainly apply to co-op mode, since there will be more simultaneous input.

### **Core Feature Set**

**MIDI Files:** The user provides a 2-channel MIDI file for maps to be generated off of. There will be 3 different difficulties that can be generated from any given file - easy, medium, and hard. The higher you go up in difficulty, the more complex the map will become with more notes. The lower you go in difficulty, the more notes that are omitted. 

**Simultaneous Notes:** Players will be able to hit multiple buttons on the keyboard at the same time to simulate chords in the songs. They will have to hit both buttons simultaneously to get a good score for those notes.

**Microsoft GS Wavetable Synth:** The game will use the Microsoft GS Wavetable Synth for playback of the MIDI files and audio in-game. This will allow the user to submit MIDI files with different instrument types to add variety to the sound. For example, this could allow a trumpet and saxophone to play in the MIDI instead of a set piano sound.

**Scores and Feedback:** Players will be scored throughout their game based on their timing with hitting the notes. They will be given a “Perfect”, “Great”, “Good”, “Bad”, or “Awful” feedback depending on how perfect they were with their timing. Scores will be cumulative based on all notes in a level, and will have a numerical value attributed to each description of individual notes. At the end of a map, they will be shown a score based on how well they performed, and will also be given a value for what a perfect score should be, rating their performance on an A-F scale.

**Progress Bar:** A progress bar will be displayed at the top of the screen that shows how far into a level the user is. It will constantly be updating in real-time so they always know how close to the end they are.

**Input Latency Calibrator:** A calibrator will be present in the game that will calculate how much input lag there is between the screen and the keyboard input. This will generate a value that the game will account for when calculating how close your timing was, and make sure that input is counted as expected. Users will ideally use this value for more accurate game play, but a default value will also be set for users who forget.

**Map Modifiers and Obstacles:** A separate challenge option will be implemented that causes false notes to appear that users must avoid pressing lest they incur a point penalty. This is another method to add extra optional difficulty. Map modifiers such as inverse controls and flipping the screen can also be introduced by user choice to introduce another level of difficulty/challenge.

**Local Co-op:** Local cooperative multiplayer may be implemented to allow two users to play the game on the same device. Key trigger mapping will be altered due to space and hardware constraints.


### **Additional Features**

**Larger MIDIs:** MIDI files with more than two channels could be supported by the application. This would let the game play more audio channels at a given time while still maintaining the most important notes while generating the map.

**Competition:** Online multiplayer may be implemented to allow users to compete against other players on separate devices online. These players would be allowed to compare scores against each other and will have a winner declared once the level ends.

**Chart Editor:** The user will be able to create their own custom note charts for imported MIDI files. These custom maps will be able to be exported and imported to be shared with other players.

**Extra Controllers:** Extra controllers may be supported for gameplay use via face buttons, a D-pad, and/or shoulder/triggers. Controllers will be connected via bluetooth or a wire, and custom mapping may be added. This might support non-standard controllers such as Guitar Hero/Rock Band guitars.

**Memory Mode:** Players can enable a “Memory Mode” gamemode in which target note opacity decreases as it descends on the screen. Players will be required to learn the timing of notes as they fall. This increases the difficulty of songs without requiring additional notes to be added.

**Save Maps:** The maps automatically generated from MIDI files can be saved along with being exported and imported to easily transfer song lists or to be shared with other players. User scores will transfer along with the maps.

### **Demonstration Plan**

During the final presentation, the program can be demonstrated in the classroom, on the local machine. The user will provide a song in the form of a MIDI file and start the game. They will observe the different features of the game. Next, two students from the other team will play the game against each other. This will show the local co-op feature. Since the game will be able to be run on any machine, it will be easy to connect a computer to the projector to make gameplay visible to everyone in the room.
