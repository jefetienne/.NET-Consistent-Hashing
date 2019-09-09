# About

A fork from Burtsev Alexey's original .NET object deep-copying extension, I inserted a hashing algorithm into the program, and stripped the deep-copying components of it to improve performance.

With .NET's GetHashCode(), the value is essentially based off of the memory address of the object, which is variable for every program run, and won't have a consistent hash value. 

Using this extension, all objects will generate a consistently-hashed _ulong_ value based off of all of its properties.

The hash also takes into account the object's _Type_, so this way, a _double_ '34.5' and a _String_ "34.5" will generate different hash codes.

# License

Source code is released under the MIT license.

The MIT License (MIT)
Copyright (c) 2014 Burtsev Alexey

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
