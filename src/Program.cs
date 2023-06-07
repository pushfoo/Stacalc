using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;


namespace Stacalc {

    /// <summary>
    /// There were too few items on the stack
    /// </summary>
    public class VMStackUnderflowException : Exception {
        public VMStackUnderflowException() {}
        public VMStackUnderflowException(string message) : base(message) {}
        public VMStackUnderflowException(string message, Exception inner) : base(message, inner) {}

        public VMStackUnderflowException(string op, int requires, int have)
            : base(
            String.Format(
                "Word '{0}' requires {1} operand{2}, but only have {3}",
                op, requires, requires > 1 ? "s" : "", have
            )) {}
    }

    /// <summary>
    /// An unrecognized word was provided
    /// </summary>
    public class VMUnrecognizedWordException : Exception {

        public VMUnrecognizedWordException() {}
        public VMUnrecognizedWordException(string message) : base(message) {}
        public VMUnrecognizedWordException(string message, Exception inner) : base(message, inner) {}
        public VMUnrecognizedWordException(Token token) : base(
            // TODO: clean this up if it can be done elegantly
            String.Format(
                "Unrecognized word '{0}' @ line {1}, col {2}{3}",
                token.value, token.position.line, token.position.col,
                token.position.file == null ? "" : String.Format(" in {0}", token.position.file)
                )
            ) {}
    }

    /// <summary>
    /// The line & column number of a token, along with the file it's in (if any)
    /// </summary>
    public readonly record struct FilePosition {
        public readonly int     line;
        public readonly int     col;
        public readonly string? file;

        public FilePosition(int line, int col, string? file = null) {
            this.line = line;
            this.col  = col;
            this.file = file;
        }
    }

    /// <summary>
    /// An immutable token read from a file
    /// </summary>
    public readonly record struct Token {
        public readonly string       value;
        public readonly FilePosition position;

        public Token(string value, FilePosition position) {
            this.value = value;
            this.position = position;
        }

        public  Token(string value, int line, int col, string? file = null) {
            this.position = new FilePosition(line, col, file);
            this.value = value;
        }
    }

    /// <summary>
    /// A tokenizer class which concatenates input internally until reset.
    /// By default, anything which isn't a tab, space, or newline counts
    /// as a token. Configure it with a regex pattern on creation.
    /// </summary>
    public class Tokenizer {
        private Regex tokenRegex;

        /// <summary>
        /// The current line number, accounting for all data processed.
        /// </summary>
        private int   lineNumber;
        private List<Token> _tokens;

        public Token[] tokens {
            get {
                Token[] t = _tokens.ToArray();
                return t;
            }
        }

        public Tokenizer(string tokenPattern = @"[^ \n\t]+") {
            tokenRegex = new Regex(tokenPattern);
            lineNumber = 0;
            _tokens = new List<Token>();
        }

        /// <summary>
        /// Reset the internal state tracking, including tokens and line count
        /// </summary>
        public void Clear() {
            _tokens.Clear();
            lineNumber = 0;
        }

        /// <summary>
        /// Extract token data from the file to internal token list, accounting for line numbers.
        /// </summary>
        public void process(Stream stream, string? fileName = null) {

            using (StreamReader reader = new StreamReader(stream)) {
                // current line data
                string? currentLine;
                MatchCollection matches;
                Token currentToken;
                while ((currentLine = reader.ReadLine()) != null) {
                    lineNumber += 1;
                    matches     = tokenRegex.Matches(currentLine);

                    foreach(Match match in matches) {
                        currentToken = new Token(
                            match.Value, lineNumber, match.Index + 1, fileName
                        );
                        _tokens.Add(currentToken);
                    }
                }
            }
        }

        /// <summary>
        /// Wrap the string in a stream, then process it.
        /// </summary>
        public void process(string? input) {
            if (input == null) return;

            // Create & write the string to an in-memory stream
            MemoryStream memStream = new MemoryStream();
            StreamWriter tempWriter = new StreamWriter(memStream);
            tempWriter.Write(input);
            tempWriter.Flush();
            memStream.Position = 0;

            // process the resulting wrapper stream
            process(memStream);
        }

    }

    /// <summary>
    /// Stack calculator VM & helper methods.
    /// Left public as practice for future embeddable VMs.
    /// </summary>
    public class VM {

        public bool debug { get; set;}
        private Stack<int> stack;

        public VM(bool debug = false) {
            stack = new Stack<int>();
            this.debug = debug;
        }

        /// <summary>
        /// Return the stack state as an array of ints in conventional order
        /// </summary>
        public int[] getStackState() {
            int[] returnValue = stack.ToArray();
            Array.Reverse(returnValue);
            return returnValue;
        }

        private void executeIntLiteral(Token token) {
            int parsed = Int32.Parse(token.value);
            stack.Push(parsed);
        }

        /// <summary>
        /// Make sure we have enough operands for this operator.
        /// If we do not, throw an OperatorUnderflowException.
        /// </summary>
        private void operandCheck(string token, int numTokensRequired) {
            if (stack.Count < numTokensRequired) {
                throw new VMStackUnderflowException(
                    token, numTokensRequired, stack.Count
                );
            }
        }

        private void executeWord(Token token) {
            // TODO: see if there's a way to encapsulate this up neatly.
            // Reflection with callables or classes may be a way
            // to handle this, but I'm not yet sure how to idiomatically
            // annotate and implement handling an arbitrary # of arguments.
            int a, b, c, result;
            result = 0;
            bool recognized = false;
            bool haveResult = true; // set to false to disable pushing to stack
            string tokenValue = token.value;

            // goto default is used because C# does not seem to support
            // fallthrough switch evaluation in the way other languages do.
            switch(tokenValue) {

                case "dup":  // ( a -- a a )
                    recognized = true;
                    operandCheck(tokenValue, 1);

                    result = stack.Peek();
                    goto default;

                case "drop":  // ( a -- )
                    recognized = true;
                    operandCheck(tokenValue, 1);

                    stack.Pop();
                    haveResult = false;
                    goto default;

                case "swap":  // ( a b -- b a )
                    recognized = true;
                    operandCheck(tokenValue, 2);
                    haveResult = false;

                    b = stack.Pop();
                    a = stack.Pop();

                    stack.Push(b);
                    stack.Push(a);
                    goto default;

                case "+":  // ( a b -- sum )
                    recognized = true;
                    operandCheck(tokenValue, 2);

                    b = stack.Pop();
                    a = stack.Pop();

                    result = a + b;
                    goto default;

                case "-":  // ( a b -- difference )
                    recognized = true;
                    operandCheck(tokenValue, 2);

                    b = stack.Pop();
                    a = stack.Pop();

                    result = a - b;
                    goto default;

                case "*":  // ( a b -- product )
                    recognized = true;
                    operandCheck(tokenValue, 2);

                    b = stack.Pop();
                    a = stack.Pop();

                    result = a * b;
                    goto default;

                case "/":  // ( a b -- quotient )
                    recognized = true;
                    operandCheck(tokenValue, 2);

                    b = stack.Pop();
                    a = stack.Pop();

                    result = a / b;
                    goto default;

                case "over":  // ( a b -- a b a )
                    recognized = true;
                    operandCheck(tokenValue, 2);
                    haveResult = false;

                    b = stack.Pop();
                    a = stack.Pop();

                    stack.Push(a);
                    stack.Push(b);
                    stack.Push(a);

                    goto default;

                case "rot":  // rotate, ( a b c -- b c a )
                    recognized = true;
                    operandCheck(tokenValue, 3);
                    haveResult = false;

                    // get the initial ordering
                    c = stack.Pop();
                    b = stack.Pop();
                    a = stack.Pop();

                    // put it back in a new one
                    stack.Push(b);
                    stack.Push(c);
                    stack.Push(a);
                    goto default;

                default:
                    if(! recognized) throw new VMUnrecognizedWordException(token);
                    if(haveResult  ) stack.Push(result);
                    break;
            }
        }

        public void executeSingleToken(Token token) {
            try {
                executeIntLiteral(token);
            } catch (FormatException) {
                executeWord(token);
            }
        }

        /// <summary>
        /// Execute a stream of pre-parsed tokens.
        /// Remember, this is not a full Forth.
        /// </summary>
        public void executeTokens(Token[] tokens) {
            foreach(Token token in tokens) {
                executeSingleToken(token);
            }
        }
    }

    public class StacalcProgram {

        /// <summary>
        /// Get user input, including a possible null line.
        /// </summary>
        public static string? prompt(string? promptString = "> ") {
            if (promptString != null) {
                Console.Write(promptString);
            }

            string? result = Console.ReadLine();
            return result;
        }

        /// <summary>
        /// Echo items to the console, optionally allowing formatting
        /// </summary>
        public static void echoItems<T>(IEnumerable<T> items, string itemFormat = "{0}", string sep = " ") {
            Console.WriteLine(String.Join(
                sep, items.Select(item => String.Format(itemFormat, item))
            ));
        }

        public static void Main(string[] args) {

            Console.WriteLine("Stacalc: a tiny stack calculator in C#");
            Console.WriteLine("Use the exit command to quit the program");

            VM        vm          = new VM();
            Tokenizer tokenizer   = new Tokenizer();
            string?   rawCommand;
            Token[]   tokens;
            int[]     stackState;

            // REPL environment, run until "exit" is hit
            do {
                rawCommand = prompt();
                if (rawCommand == "exit") break;

                try {
                    tokenizer.process(rawCommand);
                    tokens = tokenizer.tokens;
                    vm.executeTokens(tokens);
                }
                catch (Exception e) when (
                            e is VMUnrecognizedWordException ||
                            e is VMStackUnderflowException ||
                            e is OverflowException) {
                    Console.Write("Execution Error: ");
                    Console.WriteLine(e.Message);
                }
                catch (Exception e) {
                    Console.Write("System Error: ");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }

                // Display data and get ready for additional input
                stackState = vm.getStackState();
                echoItems(stackState);
                tokenizer.Clear();

            } while  (rawCommand != "exit");

        }
    }
}
