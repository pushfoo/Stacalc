# Stacalc

A tiny stack calculator with some
[Forth](https://en.wikipedia.org/wiki/Forth_(programming_language))
words added for convenience. Written in C# for .NET 7.0 to refresh my skills.

## Stack Calculators tl;dr

### Why?

1. I want to focus on learning C# without adding a second complicated grammar
2. Postfix is easier and less ambiguous than other syntax
3. It's a step towards building a concatenative language

### Postfix: Math, but not as most know it.

The usual way of writing math is called infix notation. It puts operators like
`+` between two operands. Postfix puts them afterward. For example:

| Infix (normal)      | Postfix         |
| ------------------- | --------------- |
| `1 + 2`             | `1 2 +`         |
| `(8 - 2) * 3`       | `8 2 - 3 *`     |
| `(9 - 1) / (1 + 3)` | `9 1 - 1 3 + /` |

Postfix doesn't use parentheses because it doesn't need them. The order of
operations always happens in the order they appear: from left to right.

### Stacks?

Since the order of operations is always clear, we can move all the operations
to the end:

| Infix               | Postfix, operations at the end |
| ------------------- | ------------------------------ |
| `3 - 2`             | `3 2 -`                        |
| `(8 - 2) * 3`       | `8 2 3 - *`                    |
| `(9 - 1) / (1 + 3)` | `9 1 1 3 - + /`                |

This way, we can treat the numbers as a stack. Entering a number
puts it onto the stack. All arithmetic operations above do the following:

1. Take two numbers off the top of the stack
2. Do something to them
3. Put the result on top of the stack

Although it doesn't actually matter where we put the operators as long
as each has enough operands on the stack, putting them at the end is a
step towards separating the code from the data, which will let us
turn this define new operators or words, and turn this calculator into
a programming language.

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
