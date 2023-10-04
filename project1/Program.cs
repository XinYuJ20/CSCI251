//      @author: Xin Yu Jiang
//      
//      Summary: A simplified version of du that returns the total number of
//              files, bytes, and folders give a working directory path. The program can
//              can be ran single threaded, parallel threaded, or both modes
//

using System;
using System.Globalization;
using System.IO;


namespace Project1 {

    /// <summary>
    /// 
    /// </summary>
    public class Program{
        private const string helpMessage =  "Usage: du [-s] [-p] [-b] <path> \n" +
                                            "Summarize disk usage of the set of FILES, recursively for directories. \n" +
                                            "You MUST specify one of the parameters, -s, -p, or -b \n" +
                                            "-s Run in single threaded mode \n" +
                                            "-p Run in parallel mode (uses all available processors)\n" +
                                            "-b Run in both parallel and single threaded mode. \n" +
                                            "Runs parallel followed by sequential mode \n";
        
        /// <summary>
        /// total amount of files
        /// </summary>
        public static int totalFiles = 0;

        /// <summary>
        /// total amount of bytes
        /// </summary>
        public static long totalBytes = 0;

        /// <summary>
        /// total amount of folders
        /// </summary>
        public static int totalFolders = 0;
        
        /// <summary>
        /// resetVariable counts between methods
        /// </summary> 
        public static void resetVariables() {
            totalFiles = 0;
            totalBytes = 0;
            totalFolders = 0;
        }

        /// <summary>
        /// Searches through the directory recursively in single threaded mode
        /// </summary>
        /// <param name="args"></param> 
        public static void singleThread( string pathName ){

            try {  
        
                foreach (string subdirectory in Directory.GetDirectories(pathName)) {   // returns subdirectories 
                        totalFolders += 1;    // add to folder count
                        singleThread( subdirectory );
                }

                foreach(var file in Directory.GetFiles(pathName)){     // for each file in files
                    try {
                        var f = new FileInfo(file);

                        totalFiles += 1;    // add to file count
                        totalBytes += f.Length;  // add to total byte count
                    }
                    catch ( UnauthorizedAccessException ) { // catch and skip over files that cant be accessed
                    }
                }
            }
            catch ( UnauthorizedAccessException ) {         // catch and skip over directories that cant be accessed

            }

        }

        /// <summary>
        /// Searches through the directory recursively in single threaded mode
        /// </summary>
        /// <param name="pathName"></param>
        public static void parallelThread( string pathName ){

            try{ 
                
                // for number of subdirectories in a directory
                string[] subdirectories = Directory.GetDirectories(pathName);

                // variables created for each instance of a thread
                var tempFolder = subdirectories.Length;
                long tempByte = 0;


                Parallel.ForEach(subdirectories, parallelThread );

                // files = array length from GetFiles
                string[] files = Directory.GetFiles(pathName);
                var tempFile = files.Length;

                Parallel.ForEach(files, file => {           // for each file in files
                    try{
                        var fileSize = new FileInfo(file).Length;

                        Interlocked.Add(ref tempByte, fileSize);
                    }
                    catch ( UnauthorizedAccessException ) {             // catch and skip over files that cant be accessed
                    }
                } );
                
                // directory to add to the total count instead of the files
                Interlocked.Add(ref totalBytes, tempByte);
                Interlocked.Add(ref totalFiles, tempFile);
                Interlocked.Add(ref totalFolders, tempFolder);
            }

            catch (  UnauthorizedAccessException ){                 // catch and skip over directories that cant be accessed

            }
        }

        /// <summary>
        /// print result of method calls
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="timer"></param>
        public static void printResults(TimeSpan timer, string method){
            Console.WriteLine( method + "Calculated in: " + timer + "s");
            Console.WriteLine( totalFolders.ToString("#,##0") + " folders, " + totalFiles.ToString("#,##0") + " files, " + totalBytes.ToString("#,##0") + " bytes");
        }

        /// <summary>
        /// main program to run
        /// </summary>
        /// <param name="userInput"></param>
        public static void Main(string[] userInput)
        { 
            // error checking user input
            if ( userInput.Length != 2 ) {
                Console.WriteLine( helpMessage );
                return;
            }

            var method = userInput[0];
            var pathName = userInput[1];

            if (method != "-s" && method != "-p" && method != "-b" ) {          
                Console.WriteLine( helpMessage );
                return;
            }

            Console.WriteLine("Directory '" + pathName + "':");
            Console.WriteLine(" ");

            // run in single threaded mode
            if ( method == "-s") {
                var start = DateTime.Now;

                singleThread( pathName );

                var end = DateTime.Now; 
                var timer = end - start;

                printResults( timer, "Sequential ");

            }

            // run in parallel mode
            if ( method == "-p") {
                var start = DateTime.Now;

                parallelThread( pathName );
          
                var end = DateTime.Now; 
                var timer = end - start;

                printResults( timer, "Parallel ");
            }

            // run in parallel mode followed by single
            if ( method == "-b") {

                // method for parallel thread
                var start = DateTime.Now;
                parallelThread( pathName );
                var end = DateTime.Now; 
                var timer = end - start;
                printResults( timer, "Parallel ");
                Console.WriteLine(" ");

                resetVariables();

                // method for single thread
                var start2 = DateTime.Now;
                singleThread( pathName );
                var end2 = DateTime.Now; 
                var timer2 = end2 - start2;

                printResults( timer2, "Sequential ");
            }



        }
    }    

}