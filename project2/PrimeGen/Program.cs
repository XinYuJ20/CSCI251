using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;



/// <summary>
/// Holds the main program
/// </summary>
namespace Program 
{   
    /// <summary>
    /// Extension method Primecheck
    /// </summary>
    public static class primeCheck {
        /// <summary>
        /// checks for Prime Numbers using the Miller Rabin primality test
        /// </summary>
        /// <param name="n"> number to be tested </param>
        /// <param name="k"> number of rounds of testing to be performed </param>
        /// <returns></returns>
        public static bool isProbablyPrime( this BigInteger n, int k = 10){

            var r = 0;
            var d = n - 1;
            bool flag = false;
            // write n as 2^r * d + 1 with d odd 
            while( d % 2 == 0){
                d /= 2; 
                r += 1; 
            }
            
            // witnessLoop: repeat k times
            for (int i = 0; i < k ; i++)
            {
                // random int a [ 2, n - 2]
                BigInteger a = generateRandomNumber(n);
                // x <- a^d mod n
                BigInteger x = BigInteger.ModPow(a,d,n);
                // if x = 1 or x = n - 1 
                if ( x == 1 || x == n - 1){
                    // continue witnessLoop
                    continue;
                }
                //     repeat r - 1 times  
                for ( int b = 0; b < r - 1; b++){
                    // x <- x^2 mod n 
                    x = BigInteger.ModPow(x, 2, n);
                    //  if x = n - 1 then 
                    if (x == n - 1){
                        flag = true;
                        // continue witnessLoop
                        continue;
                    }
                }
                if( flag == true ){
                    continue;
                }
            // composite
            return false;
            }
            
        // probably prime
        return true;
        }

        /// <summary>
        /// generates a random number for a
        /// </summary>
        /// <param name="byteCount"> length of bytes to generate </param>
        /// <returns></returns>
        private static BigInteger generateRandomNumber(BigInteger byteCount)
        {
            BigInteger a;
            RandomNumberGenerator generate = RandomNumberGenerator.Create();
            byte[] randomNumber = new byte[byteCount.ToByteArray().LongLength];

            do {
                generate.GetBytes(randomNumber);
                a = new BigInteger(randomNumber);
            } while ( a < 2 || a >= byteCount - 2);
            return a;
        }


    }

    /// <summary>
    /// Holds the main class
    /// </summary>
    public class PrimeGen{

        /// <summary>
        /// generate random numbers to be tested for primeality
        /// </summary>
        /// <param name="byteCount"> length of the number to generate </param>
        /// <param name="primeCount"> amount of primes to generate </param>
        public void generateNumber( int byteCount, int primeCount ) {
            object printLock  = new();
            int i = 0 ;
                do{
                    Parallel.For( 0, 10, l => {
                        bool isPrime= false;
                        BigInteger number = 0;

                        byte[] test =  RandomNumberGenerator.GetBytes( byteCount );
                        number = new BigInteger( test.Concat( new byte[] {0} ).ToArray() );
                        isPrime = number.isProbablyPrime();

                        if ( isPrime ){
                            lock( printLock ) {
                                if ( i < primeCount ){
                                    Console.WriteLine( i + 1 + ": " + number);
                                    Interlocked.Increment(ref i);
                                }
                                if ( i < primeCount ){
                                    Console.WriteLine(" ");
                                }
                               
                            }
                        }
                    });
                
                }
                while( i < primeCount );
        }
    }
    
    /// <summary>
    /// Holds the main function 
    /// </summary>
    public class Program {
        /// <summary>
        /// message to be printed on input error
        /// </summary>
        private const string helpMessage =  "Usage:<bits> <count> \n" +
                                            "bits - the number of bits of the prime number, this must be a " +
                                            "multiple of 8, and at least 32 bits. \n" +
                                            "count - the number of prime numbers to generate, defaults to 1 \n" ;

        /// <summary>
        /// Main function for program PrimeGen
        /// </summary>
        /// <param name="userInput"></param>
        public static void Main(string[] userInput){ 

            int byteCount;
            int primeCount = 1;
            
            //two inputs
            if ( userInput.Length == 2 ) {
                byteCount = int.Parse(userInput[0]) / 8 ;
                primeCount = int.Parse(userInput[1]);
            } 

            // one input
            else if ( userInput.Length == 1){
                 byteCount = int.Parse(userInput[0]) / 8 ;
            } 

            // incorrect inputs
            else {
            Console.WriteLine(helpMessage);
            return;
            };

            Console.WriteLine("BitLength: " +  userInput[0] + " bits");
            PrimeGen findNum = new PrimeGen();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            findNum.generateNumber( byteCount, primeCount ) ;

            sw.Stop();
            var timer = sw.Elapsed;

            Console.WriteLine("Time to Generate: {0}", timer);
        }
    }
}