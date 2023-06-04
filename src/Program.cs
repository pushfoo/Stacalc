﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace StackCalc {

    internal class StackCalc {

        /// <summary>
        /// Split a string into tokens.
        /// </summary> 
        private static string[] tokenize(string input) {
            // A simple but brittle implementation to get things started.
            // This will break on repeated spaces, and should be replaced
            // with something better.
            return input.Split();
        }

        /// <summary>
        /// Get user input, smoothing null returns to ""
        /// </summary>
        private static string prompt(string? promptString = "> ") {
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
        private static void echoTokens(string[] tokens) {
            for(int i = 0; i < tokens.Length; i++) {
                Console.Write(string.Format("'{0}'", tokens[i]));
                if (i < tokens.Length - 1) {
                     Console.Write(' ');
                }
            } 
            Console.WriteLine();
        }

        private class OperatorUnderflowException : Exception {
            public OperatorUnderflowException() {}
            public OperatorUnderflowException(string message) : base(message) {}
            public OperatorUnderflowException(string message, Exception inner) : base(message, inner) {}

            public OperatorUnderflowException(string op, int requires, int have)
             : base(String.Format("Operator '{0}' requires {1} operands, but only have {2}", op, requires, have)) {}
        }
        private class BadOperatorException : Exception {
            public BadOperatorException() {}
            public BadOperatorException(string message) : base(message) {}
            public BadOperatorException(string message, Exception inner) : base(message, inner) {}

        }
        /// <summary>
        /// Stack calculator VM state & helper methods
        /// </summary>
        private class VM {

            public bool debug { get; set;}
            Stack<int> stack;

            public VM(bool debug = false) {
                stack = new Stack<int>();
                this.debug = debug;
            }
            private void printStack() {
                int[] arr = stack.ToArray();
                for(int i = arr.Length - 1; i > 0; i--) {
                    Console.Write(String.Format("{0} ", arr[i]));
                }
                Console.WriteLine(String.Format("{0}", arr[0]));
            }
            private void attemptIntHandling(string token) {
                int parsed = Int32.Parse(token);
                stack.Push(parsed);

            }
            /// <summary>
            /// Make sure we have enough operands for this operator.
            /// If we do not, throw an OperatorUnderflowException.
            /// </summary>
            private void operandCheck(string token, int numTokensRequired) {
                if (stack.Count < numTokensRequired) {
                    throw new OperatorUnderflowException(token, numTokensRequired, stack.Count);
                }
            }
            private void attemptOperatorHandling(string token) {
                //TODO: see if there's a way to pack this up neatly
                // Candidates include: reflection + callables.
                int a, b, result;
                result = 0;
                bool recognized = false;
                bool haveResult = true; // set to false to disable pushing to stack

                switch(token) {
                    case "drop":
                        recognized = true;
                        operandCheck(token, 1);
                        stack.Pop();
                        haveResult = false;
                        goto default;

                    case "dup":
                        recognized = true;
                        operandCheck(token, 1);
                        result = stack.Peek();
                        goto default;

                    case "+":
                        recognized = true;
                        operandCheck(token, 2);
                        b = stack.Pop();
                        a = stack.Pop();
                        result = a + b;
                        goto default;

                    case "-":
                        recognized = true;
                        operandCheck(token, 2);
                        b = stack.Pop();
                        a = stack.Pop();
                        result = a - b;
                        goto default;

                    default:
                        if(! recognized) throw new BadOperatorException(String.Format("Invalid operator: '{0}'", token));
                        if(haveResult  ) stack.Push(result);
                        break;
                }
            }
            public void handleToken(string token) {
                try {
                    attemptIntHandling(token);
                } catch (FormatException) {
                   attemptOperatorHandling(token);
                }
            }

            public void handleInput(string input) {
                // Parse & echo tokens if debug
                string[] tokens = tokenize(input);
                if(debug) echoTokens(tokens);
                try {
                    foreach(string token in tokens) {
                        handleToken(token);
                    }
                }
                catch (BadOperatorException e) {
                    Console.WriteLine(e.Message);
                }
                catch (OperatorUnderflowException e) {
                    Console.WriteLine(e.Message);
                }
                printStack();
            }
        }
        public static void Main(string[] args) {
            Console.WriteLine("C# Basic Stack Calculator");
            Console.WriteLine("Use the exit command to quit the program.");

            string command = "";

            VM vm = new VM(true);

            // REPL environment, run until "exit" is hit
            do {
                command = prompt();
                if (command == "exit") break;
                vm.handleInput(command);

            } while  (command != "exit"); 

        }
    }
}
