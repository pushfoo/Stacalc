# Stacalc

A tiny stack calculator with some
[Forth](https://en.wikipedia.org/wiki/Forth_(programming_language))
words added for convenience. Written in C# for .NET 7.0 to refresh my skills.
If you already know what a stack calculator is, skip to the usage section
below.

For the really impatient:
1. Clone this repo
2. Run `dotnet run` in your terminal with the [.NET 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) installed
3. Enter something like `3 2 +` and press enter.

The supported math operators include `+`, `-`, `*`, and `/`. A full list of
supported words is located at the end of this file.

## What's a Stack Calculator & Why?

### tl;dr

1. I want to focus on learning C# without adding a second complicated grammar
2. Postfix is easier and less ambiguous than other syntax
3. It's a step towards building a concatenative language

### Postfix: Math, but not as most know it.

The usual way of writing arithmetic is called infix notation. It puts operators
like `+` between two operands. Postfix notation puts them afterward. For
example:

| Infix (normal)      | Postfix         |
| ------------------- | --------------- |
| `1 + 2`             | `1 2 +`         |
| `(8 - 2) * 3`       | `8 2 - 3 *`     |
| `(9 - 1) / (1 + 3)` | `9 1 - 1 3 + /` |

Postfix doesn't use parentheses because it doesn't need them. The order of
operations always happens in the order they appear: from left to right.

### Stacks?

Since the order of operations is always clear, we can treat the numbers as
a stack. Entering a number puts it on top, and a math operation will take two numbers and put back one.

For example, let's look at how `8 2 - 3 *` is executed.

1. `8` is pushed onto the stack
2. `2` is pushed onto the stack
2. `-` removes the top two elements and puts `6` on top of the stack
4. `3` is pushed onto the stack
5. `*` removes the top two elements and puts `18` on top of the stack

### Next Steps

With stacks, we can reorder our expressions to put the operators at the end.

1. `3` is pushed onto the stack
1. `8` is pushed onto the stack
2. `2` is pushed onto the stack
2. `-` removes the top two elements and puts `6` on top of the stack
5. `*` removes the top two elements and puts `18` on top of the stack

As long as each operator has enough operands on the stack, it doesn't matter
how far after the numbers it is. If we add some new stack reordering words,
we can begin separating our data from our code to define functions. This is
an important step toward user-defined words and turning a calculator into
a programming language.

## Usage

### Requirements

* The [.NET 7.0 C# SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* The ability to open & run commands in the terminal

### Building & Launching

Once you have the .NET 7.0 SDK installed, you can launch from the root directory as follows:
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

| Word | Minimum Stack Size | Action                                               |
|------|--------------------|------------------------------------------------------|
|`exit`| 0                  | Exit the program.                                    |
|`dup` | 1                  | Push a copy of the top element onto the stack        |
|`drop`| 1                  | Pop an element off the stack and throw it away       |
|`swap`| 2                  | Exchange the top two elements on the stack           |
|`+`   | 2                  | Pop `b`, Pop `a`, push `a + b` onto the stack        |
|`-`   | 2                  | Pop `b`, pop `a`, push `a - b` onto the stack        |
|`*`   | 2                  | Pop `b`, pop `a`, push `a * b` onto the stack        |
|`/`   | 2                  | Pop `b`, pop `a`, push `a / b` onto the stack        |
|`over`| 2                  | Copy the 2nd element onto the top: `( a b -- a b a )`|
|`rot` | 3                  | Rotate the top 3 elements: `( a b c -- b c a )`      |
