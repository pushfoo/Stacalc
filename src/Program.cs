using System;
using System.Collections.Generic;

namespace StackCalc {

    internal class StackCalc {

        /// <summary>
        /// Split a string into tokens.
        /// </summary> 
        private static string[] tokenize(string input) {
            // This is a good-enough implementation for the moment, but it
            // should probably be replaced in the future.
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

        /// <summary>
        /// Stack calculator VM state & helper methods
        /// </summary>
        private class VM {
            Stack<int> stack;

            public VM() {
                stack = new Stack<int>();
            }
            private void attemptIntHandling(string token) {
                stack.Append(Int32.Parse(token));
            }
            private void attemptOperatorHandling(string token) {
                
            }
            public void handleToken(string token) {
                try {
                    attemptIntHandling(token);
                } catch (FormatException) {
                   attemptOperatorHandling(token);
                }
            }
        }
        public static void Main(string[] args) {
            Console.WriteLine("C# Basic Stack Calculator");
            Console.WriteLine("Use the exit command to quit the program.");

            string command = "";
            string[] tokens = {};

            // REPL environment, run until "exit" is hit
            do {
                command = prompt();
                if (command == "exit") break;
                tokens = StackCalc.tokenize(command);

                echoTokens(tokens);
            } while  (command != "exit"); 

        }
    }
}
