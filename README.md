# MediaViewer
A media viewer interface to the vlc media player developed in C#

This tool provides a way to access your media collection (audio only for now) from a tree structure and look at its tag info. Songs can be played by the VLC media player created by VideoLan (www.videolan.org). I am in no way affiliated with the VideoLan organization. I just wanted to have my music collection viewable in one location and be able to play it easily.

In developing this project, I looked at a few different third-party music tag packages and decided to use taglib-sharp. It's available on GitHub (https://github.com/mono/taglib-sharp.git). This application also uses the excellent SQLite database package (http://sqlite.org). It's lightweight and easy to work with and a big plus is there is no server to connect to. This app needs the 'SQLite.Interop' and 'System.Data.SQLite' DLLs. Finally, I want to mention Josh Smith's excellent CodeProject (www.codeproject.com) post, "Simplifying the WPF TreeView by Using the ViewModel Pattern" from way back in 2008. A long time ago by internet standards. It really inspired me to start this project.

V.2.0 This version got away from reusing the main VLC GUI to play audio files. MediaViewer uses the vlc api and the VLCSharp development api to play audio files w/o starting a VLC process to do it. So now, view your file collection in a tree structure, choose a file to play or a whole "CD" worth of files. They get put into a playlist, click the play button and start listening. There is rewind/fastforward/pause while playing. There is also a skip track ahead and track back.

V.2.1 There is now playlist save, delete and load functions. Altering the color of the GUI is available now. This is file driven by changing color values in a configuration file. This may be altered slightly by making different color schemes available by choice.

There's more to do. The biggest items are:

    Reorder tracks in the playlist
    Randomize tracks in a playlist
    Repeat play tracks
    Double-click a track in the playlist to start playing
    Improve look and feel of the GUI
    Add video playback

A long term goal will be to split the functionality of the player to make it a server-client architecture. Currently it is a single standalone player. The server will provide the search and store of media files and the streaming of files. The server should provide a web interface for clients.


