using System;
using System.Collections.Generic;
using System.Reflection;

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

        private void executeIntLiteral(string token) {
            int parsed = Int32.Parse(token);
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

        private void executeWord(string token) {
            // TODO: see if there's a way to encapsulate this up neatly.
            // Reflection with callables or classes may be a way
            // to handle this, but I'm not yet sure how to idiomatically
            // annotate and implement handling an arbitrary # of arguments.
            int a, b, c, result;
            result = 0;
            bool recognized = false;
            bool haveResult = true; // set to false to disable pushing to stack

            // goto default is used because C# does not seem to support
            // fallthrough switch evaluation in the way other languages do.
            switch(token) {

                case "dup":  // ( a -- a a )
                    recognized = true;
                    operandCheck(token, 1);

                    result = stack.Peek();
                    goto default;

                case "drop":  // ( a -- )
                    recognized = true;
                    operandCheck(token, 1);

                    stack.Pop();
                    haveResult = false;
                    goto default;

                case "swap":  // ( a b -- b a )
                    recognized = true;
                    operandCheck(token, 2);
                    haveResult = false;

                    b = stack.Pop();
                    a = stack.Pop();

                    stack.Push(b);
                    stack.Push(a);
                    goto default;

                case "+":  // ( a b -- sum )
                    recognized = true;
                    operandCheck(token, 2);

                    b = stack.Pop();
                    a = stack.Pop();

                    result = a + b;
                    goto default;

                case "-":  // ( a b -- difference )
                    recognized = true;
                    operandCheck(token, 2);

                    b = stack.Pop();
                    a = stack.Pop();

                    result = a - b;
                    goto default;

                case "*":  // ( a b -- product )
                    recognized = true;
                    operandCheck(token, 2);

                    b = stack.Pop();
                    a = stack.Pop();

                    result = a * b;
                    goto default;

                case "/":  // ( a b -- quotient )
                    recognized = true;
                    operandCheck(token, 2);

                    b = stack.Pop();
                    a = stack.Pop();

                    result = a / b;
                    goto default;

                case "over":  // ( a b -- a b a )
                    recognized = true;
                    operandCheck(token, 2);
                    haveResult = false;

                    b = stack.Pop();
                    a = stack.Pop();

                    stack.Push(a);
                    stack.Push(b);
                    stack.Push(a);

                    goto default;

                case "rot":  // rotate, ( a b c -- b c a )
                    recognized = true;
                    operandCheck(token, 3);
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
                    if(! recognized) {
                        throw new VMUnrecognizedWordException(
                            String.Format("Unrecognized word: '{0}'", token)
                        );
                    }
                    if(haveResult  ) stack.Push(result);
                    break;
            }
        }

        public void executeSingleToken(string token) {
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
        public void executeTokens(string[] tokens) {
            foreach(string token in tokens) {
                executeSingleToken(token);
            }
        }
    }

    public class StacalcProgram {

        /// <summary>
        /// Split a string into tokens.
        /// </summary> 
        public static string[] tokenize(string input) {
            // A simple but brittle implementation to get things started.
            // TODO: Replace this with something which doesn't break on repeated
            // spaces.
            return input.Split();
        }

        /// <summary>
        /// Get user input, smoothing null returns to ""
        /// </summary>
        public static string prompt(string? promptString = "> ") {
            if (promptString != null) {
                Console.Write(promptString);
            }

            // Get input & smooth nulls to ""
            string? result = Console.ReadLine();
            if (result == null) {
                return "";
            }
            return result;
        }

        /// <summary>
        /// Echo quoted tokens to the console.
        /// </summary>
        public static void echoItems<T>(T[] items, string sep = " ") {
            // TODO: Decide whether leaving an unhandled null here is acceptable.
            Console.WriteLine(String.Join(
                sep, items.Select(item => item.ToString()))
            );
        }

        public static void Main(string[] args) {
            Console.WriteLine("Stacalc: a tiny stack calculator in C#");
            Console.WriteLine("Use the exit command to quit the program");

            VM       vm          = new VM();
            string   rawCommand  = "";
            string[] tokens;
            int[]    stackState;

            // REPL environment, run until "exit" is hit
            do {
                rawCommand = prompt();
                tokens = tokenize(rawCommand);
                if (rawCommand == "exit") break;

                try {
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

                stackState = vm.getStackState();
                echoItems(stackState);

            } while  (rawCommand != "exit");

        }
    }
}
