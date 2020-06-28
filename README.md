# MediaViewer
A media player using the vlc media API developed in C#

This tool provides a way to access your media collection (audio only for now) from a tree structure and look at its tag info. Songs can be played by the VLC media player created by VideoLan (www.videolan.org). I am in no way affiliated with the VideoLan organization. I just wanted to have my music collection viewable in one location and be able to play it easily.

In developing this project, I looked at a few different third-party music tag packages and decided to use taglib-sharp. It's available on GitHub (https://github.com/mono/taglib-sharp.git). This application also uses the excellent SQLite database package (http://sqlite.org). It's lightweight and easy to work with and a big plus is there is no server to connect to. This app needs the 'SQLite.Interop' and 'System.Data.SQLite' DLLs. Finally, I want to mention Josh Smith's excellent CodeProject (www.codeproject.com) post, "Simplifying the WPF TreeView by Using the ViewModel Pattern" from way back in 2008. A long time ago by internet standards. It really inspired me to start this project.

V.2.0 This version got away from reusing the main VLC GUI to play audio files. MediaViewer uses the vlc api and the VLCSharp development api to play audio files w/o starting a VLC process to do it. So now, view your file collection in a tree structure, choose a file to play or a whole "CD" worth of files. They get put into a playlist, click the play button and start listening. There is rewind/fastforward/pause while playing. There is also a skip track ahead and track back.

V.2.1 There is now playlist save, delete and load functions. Altering the color of the GUI is available now. This is file driven by changing color values in a configuration file. This may be altered slightly by making different color schemes available by choice.

**MediaViewer v2.1**
![MediaViewerv2 1](https://user-images.githubusercontent.com/8380677/85955868-ac26cb80-b94f-11ea-8df4-aa562bf39a15.PNG?raw=true "MediaViewer v2.1")

There's more to do. The biggest items are:

    Reorder tracks in the playlist
    Repeat play tracks
    Double-click a track in the playlist to start playing
    Improve look and feel of the GUI [Started]
    Add video playback

V.2.2 The GUI has a slightly improved interface for player controls.  The controls are in a round style instead of the old mix of round and blocky buttons.  The progress bar is curved instead of straight to better match the round buttons.  The progress bar design was lifted from the excellent Material Design in XAML Toolkit offered by the design house Material Design In XAML (https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) with a few tweaks done to make it specific for use in this project.

**MediaViewer v2.2 Tree view**
![MediaViewerv2 2-opentree](https://user-images.githubusercontent.com/8380677/85210266-7f016a00-b30c-11ea-9155-b881de04f779.PNG?raw=true "MediaViewer v2.2 - Tree view")

**MediaViewer v2.2 Playlist view**
![MediaViewv2 2-playlist-2](https://user-images.githubusercontent.com/8380677/85955897-dbd5d380-b94f-11ea-8a0d-bc2c2afe5b06.PNG?raw=true "MediaViewer v2.2 - Playlist view")


A long term goal is to split the functionality of this player to make it a server-client architecture. Currently it is a single standalone player. The server will provide the search and store of media files and the streaming of files. This could possibly be a good candidate for a Blazor server which will provide hooks for web-based clients, possibly Electron to keep it looking more desktop looking.



