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


#### Copying

The MIT License (MIT)

Copyright (c) 2014 PBXg33k

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

#### Change log
##### 2014/04/03
+ Fixed Exception on 0 completed galleries

##### 2014/01/29
+ Test zip option isn't hardcoded to TRUE anymore
+ Added more scanner options
+ HathZipper is now a class with all logic
+ Galleries are now classes with simple logic for individual operations
+ Improved code

##### 2014/01/22
**First commit and successful build**
+ Scan galleries
+ Compress galleries
+ Delete galleries
