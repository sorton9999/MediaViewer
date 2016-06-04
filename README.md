# MediaViewer
A media viewer interface to the vlc media player developed in C#

This tool provides a way to access your media collection (audio only for now) from a tree structure and look at its tag
info.  Songs can be played by the VLC media player created by VideoLan (www.videolan.org).  I am in no way affiliated with
the VideoLan organization.  I just wanted to have my music collection viewable in one location and be able to play it easily.

In developing this project, I looked at a few different third-party music tag packages and decided to use taglib-sharp.
It's available on GitHub (https://github.com/mono/taglib-sharp.git).  This application also uses the excellent SQLite
database package.  It's lightweight and easy to work with and a big plus is there is no server to connect to.  This app
needs the 'SQLite.Interop' and 'System.Data.SQLite' DLLs.  Finally, I want to mention Josh Smith's excellent CodeProject
(www.codeproject.com) post, "Simplifying the WPF TreeView by Using the ViewModel Pattern" from way back in 2008.  A long
time ago by internet standards.  It really inspired me to start this project.

There are a few more things I want to add to this application.  I started very simply and it's starting to get much more
involved.  I don't like the GUI much.  It served its purpose while developing, but it could use a revamp.  I also want to
add more VLC interface functionality like adding to playlists without automatically playing, looping and repeating songs and
maybe authoring and saving playlists to be loaded and played automatically in VLC.
