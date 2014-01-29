# HatHZipper
==========

### Brief summary
A small command-line tool written in C# (.NET 4.0) for H@H runners.

HatHZipper recursively scans the downloaded galleries folder and compresses completed galleries to a given directory.

### In the (near) future
* Delete source files after successful compression (untested)
* Add support for multiple formats
* Skip already compressed galleries
* Generate reports
* Analyse Galleries with similar names
* Monitor "downloaded" folder and compress immediately on completion

#### Credit where credit's due
This project uses third party libraries listed below.
* DotNetZip (https://github.com/haf/DotNetZip.Semverd)
* NDesk.Options (http://www.ndesk.org/Options)

#### Change log
##### 2014/01/29
+ Test zip option isn't hardcoded to true anymore
+ Added more scanner options
+ HathZipper is now a class with all logic
+ Galleries are now classes with simple logic for individual operations
+ Improved code
##### 2014/01/22
**First commit and successful build**
+ Scan galleries
+ Compress galleries
+ Delete galleries
