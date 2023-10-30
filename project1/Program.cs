//      @author: Xin Yu Jiang
//      
//      Summary: A simplified version of du that returns the total number of
//              files, bytes, and folders give a working directory path. The program can
//              can be ran single threaded, parallel threaded, or both modes
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;


namespace Project1 {

    /// <summary>
    /// main program of the project
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
        public static void singleThread( DirectoryInfo pathName ){

            try {  
        
                foreach (DirectoryInfo subdirectory in pathName.GetDirectories() ) {   // returns subdirectories 
                        totalFolders += 1;    // add to folder count
                        singleThread( subdirectory );
                }

                foreach(var file in pathName.GetFiles() ){     // for each file in files
                    try {

                        totalFiles += 1;    // add to file count
                        totalBytes += file.Length;  // add to total byte count
                    }
                    catch ( UnauthorizedAccessException ) { } // catch and skip over files that cant be accessed
                }
            }
            catch ( UnauthorizedAccessException ) { }         // catch and skip over directories that cant be accessed

        }

        /// <summary>
        /// Searches through the directory recursively in single threaded mode
        /// </summary>
        /// <param name="pathName"></param>
        public static void parallelThread( DirectoryInfo pathName ){

            try{ 
                
                // for number of subdirectories in a directory
                var subdirectories = pathName.GetDirectories();

                // variables created for each instance of a thread
                var tempFolder = subdirectories.Length; 

                Parallel.ForEach(subdirectories, parallelThread );

                // files = array length from GetFiles
                var files = pathName.GetFiles();
                var tempFile = files.Length;
               
                Parallel.ForEach(files, file => {           // for each file in files
                    try{
                        var fileSize = file.Length;
                        Interlocked.Add(ref totalBytes, fileSize); 
                       
                    }
                    catch ( UnauthorizedAccessException ) {}             // catch and skip over files that cant be accessed
                } );
                
                // directory to add to the total count instead of the files  
                Interlocked.Add(ref totalFiles, tempFile);
                Interlocked.Add(ref totalFolders, tempFolder);
            }
            catch (  UnauthorizedAccessException ){}               // catch and skip over directories that cant be accessed
        }

        /// <summary>
        /// print result of method calls
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="timer"></param>
        public static void printResults(double timer, string method){
            Console.WriteLine( method + "Calculated in: " + timer + "s");
            Console.WriteLine( totalFolders.ToString("#,##0") + " folders, " + totalFiles.ToString("#,##0") + " files, " + totalBytes.ToString("#,##0") + " bytes");
        }

        /// <summary>
        /// starts the threading process
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
              
                Stopwatch sw = new Stopwatch();
                sw.Start();

                singleThread( new DirectoryInfo( pathName) );

                sw.Stop();
                var timer = sw.Elapsed.TotalSeconds;
                
                printResults( timer, "Sequential ");

            }

            // run in parallel mode
            if ( method == "-p") {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                parallelThread( new DirectoryInfo( pathName) );
              
                sw.Stop();
                var timer = sw.Elapsed.TotalSeconds;

                printResults( timer, "Parallel ");
            }

            // run in parallel mode followed by single
            if ( method == "-b") {

                // method for parallel thread
                Stopwatch sw = new Stopwatch();
                sw.Start();

                parallelThread( new DirectoryInfo( pathName) );

                sw.Stop();
                var timer = sw.Elapsed.TotalSeconds;

                printResults( timer, "Parallel ");
                Console.WriteLine(" ");

                resetVariables();

                // method for single thread
                Stopwatch sw2 = new Stopwatch();
                sw2.Start();

                singleThread(  new DirectoryInfo( pathName) );

                sw2.Stop();
                var timer2 = sw2.Elapsed.TotalSeconds; // is that fast enough tho

                printResults( timer2, "Sequential ");
            }

        }
    }    

}