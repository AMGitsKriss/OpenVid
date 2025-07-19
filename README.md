#OpenVid

This app sits somewhere between YouTube, Plex, and an old-style image board in terms of usability. This app allows me to manage and watch my media library on any device within a network and remotely. It is intended for this to run on a single machine, but there's no reason it can't be configured for multiple boxes.

## Setup
Create a database, add it to the config, and Entity Framework should build what it needs on the first run. `I might have started migrating to SQL scripts and stopped halfway. Some tampering might be require for a new instance.`

The app needs a bucket-like location to store the videos. I use IIS for both the app and thr bucket. There is also an encoding app in the repo that runs as a console application. 

Movies can be imported in two ways:
1. The movie can be imported as-is using the "Import Preprocessed" functionality. The movie should be an mp4 and already be encoded for web streaming. This is the easiest way of getting media into the app, but it can't take advantage of dash and hls features, such as drm and subtitles. If the user knows the video url, they can download it. 
2. Movies can be automatically imported, encoded, and converted into Dash, HLS, and streamable MP4 formats (as configured) with the use of Handbrake, and an encoding app included in the repo. During playback, this uses the device's prefered format - if available. There are currently some issues with this pipeline - if handbrake fails the error handling is poor. This method allows for subtitles, drm, and encoding & playback in multiple resolutions. 

Configuration allows playback to be hidden behind a user login. 

## Dependencies 
- Handbrake
- Shaka

## Using The App

### Search
Videos can be tagged, and searches can be made based on those tags. Eg: "tag:stargate tag:season_01" will search for movies that match both the stargate and season_01 tags. 

To search by title, simple use the word directly. Eg. "The Fifth Race" will find all videos will the given three words in the title. 

### Playback
Playback uses Shaka media player. This is not currently setup for use with a Chromecast. The UI uses a youtube-like skin. If multiple resultuions are available is does it's best to match available bandwidth to the screen resolution. Subtitles can be selected in the settings menu. 
