# Stacalc

A tiny stack calculator with some
[Forth](https://en.wikipedia.org/wiki/Forth_(programming_language)
words added for convenience. Written in C# for .NET 7.0 to refresh my skills.

## Usage

### Requirements

* The [NET 7.0 C# SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* The ability to open & run commands in the terminal

### Building & Launching

If you have the .NET 7.0 SDK installed, you can launch from the root directory as follows:
```console
$ dotnet run
Stacalc: a tiny stack calculator in C#
Use the exit command to quit the program
>
```

### Usage Example

Like a Forth, Stacalc treats input as one of two things:

* Signed 32-bit integers in decimal format
* Words, which are commands to do something such as add or exit

```console
$ dotnet run
Stacalc: A tiny stack calculator in C#
Use the exit command to quit the program
> 9 5 -2 
9 5 -2
> +
9 3
> /
3
> exit
```

### Supported Words

A word is anything other than a number literal. These include mathematical operators as
well as stack manipulation.

| Word |  \# Operands Required) | Action                                               |
|------|------------------------|------------------------------------------------------|
|`exit`| 0                      | Exit the program.                                    |
|`dup` | 1                      | Push a copy of the top element onto the stack        |
|`drop`| 1                      | Pop an element off the stack and throw it away       |
|`swap`| 2                      | Exchange the top two elements on the stack           |
|`+`   | 2                      | Pop `b`, Pop `a`, push `a + b` onto the stack        |
|`-`   | 2                      | Pop `b`, pop `a`, push `a - b` onto the stack        |
|`*`   | 2                      | Pop `b`, pop `a`, push `a * b` onto the stack        |
|`/`   | 2                      | Pop `b`, pop `a`, push `a / b` onto the stack        |
|`over`| 2                      | Copy the 2nd element onto the top: `( a b -- a b a )`|
|`rot` | 3                      | Rotate the top 3 elements: `( a b c -- b c a )`      |
