# DownloadTracker
A .net core implementation of how to check if a download (browser) is still in progress or interrupted.

Based from this: https://www.codeproject.com/Articles/74654/File-Download-in-ASP-NET-and-Tracking-the-Status-o

### Note  
You might want to replace "test.zip" with a larger file (>=500mb) because downloading in a local server (test) is fast, getting the status at first call might have the file already downloaded.