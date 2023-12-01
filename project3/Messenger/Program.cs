/// @author: Xin Yu Jiang
/// Program to generate big Prime numbers

using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


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
        /// <returns>true if the number is prime, false otherwise</returns>
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
        /// <returns>a random number a</returns>
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
    /// generates a prime number
    /// </summary>
    public class PrimeGen{

        /// <summary>
        /// generate random numbers to be tested for primeality
        /// </summary>
        /// <param name="byteCount"> length of the number to generate </param>
         /// <returns> a prime number a</returns>
        public BigInteger generateNumber( int byteCount ) {
            BigInteger prime = 0;
    
            int[] primeList = {2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97};

            Parallel.For( 0, int.MaxValue, (j, state) => {
                byte[] test =  RandomNumberGenerator.GetBytes( byteCount );
                BigInteger number = new BigInteger( test.Concat( new byte[] {0} ).ToArray() );

                foreach (int prime in primeList){
                    if (number % prime == 0){
                        return;
                    }
                }

                bool isPrime = number.isProbablyPrime();                

                if ( isPrime ){
                    prime = number;
                    state.Stop();
                }
            });
            return prime;
        }
    }
    
    /// <summary>
    /// Generates Key
    /// </summary>
    public class KeyGeneration{

        BigInteger n;
        BigInteger e;
        BigInteger d;

        /// <summary>
        /// Generates a public and private key of size byteCount
        /// </summary>
        /// <param name="byteCount"> the size of the key</param>
        public void KeyGen( int byteCount ){
            int half = byteCount / 2;

            double percentage = RandomNumberGenerator.GetInt32(-int.MaxValue, int.MaxValue) / ((double)int.MaxValue) * .1;
            percentage = Math.Sign(percentage) * .2 + percentage;

            if (percentage == 0)
            {
                percentage = .2;
            }

            int pLength = (int)((half * percentage) + half);
            int qLength = byteCount - pLength;

            PrimeGen findNum = new();

            var p = findNum.generateNumber(pLength);
            var q = findNum.generateNumber(qLength);

            n = p * q;
            var r = ( p - 1 ) * ( q - 1 );
            
            do{
                e = findNum.generateNumber(2);
            } while (e <= 2 && e >= r);
        

            d = modInverse(e,r);
            
            byte[] publicKey;
            byte[] privateKey;
            if (BitConverter.IsLittleEndian)
            {
                var eByteCount = BitConverter.GetBytes(e.GetByteCount()).ToArray();
                var eByte = Enumerable.Repeat<Byte>(0, 4 - eByteCount.Length).Concat(eByteCount).Reverse().ToArray();
                
                var keyE = e.ToByteArray().ToArray() ; 

                var nByteCount = BitConverter.GetBytes(n.GetByteCount());
                var nByte = Enumerable.Repeat<Byte>(0, 4 - nByteCount.Length).Concat(nByteCount).Reverse().ToArray();
                var keyN = n.ToByteArray().ToArray() ;
                
                publicKey = eByte.Concat(keyE).Concat(nByte).Concat(keyN).ToArray();

                var dByteCount = BitConverter.GetBytes(d.GetByteCount());
                var dByte = Enumerable.Repeat<Byte>(0, 4 - dByteCount.Length).Concat(dByteCount).Reverse().ToArray();
                var keyed = d.ToByteArray().ToArray() ;

                privateKey = dByte.Concat(keyed).Concat(nByte).Concat(keyN).ToArray();
                }
            else
            {
                var eByteCount = BitConverter.GetBytes(e.GetByteCount()).ToArray();
                var eByte = Enumerable.Repeat<Byte>(0, 4 - eByteCount.Length).Concat(eByteCount).ToArray();
                Console.WriteLine(e.GetByteCount());
                var keyE = e.ToByteArray().Reverse().ToArray() ; 

                var nByteCount = BitConverter.GetBytes(n.GetByteCount());
                var nByte = Enumerable.Repeat<Byte>(0, 4 - nByteCount.Length).Concat(nByteCount).ToArray();
                var keyN = n.ToByteArray().Reverse().ToArray() ;
                
                publicKey = eByte.Concat(keyE).Concat(nByte).Concat(keyN).ToArray();

                var dByteCount = BitConverter.GetBytes(d.GetByteCount());
                var dByte = Enumerable.Repeat<Byte>(0, 4 - dByteCount.Length).Concat(dByteCount).ToArray();
                var keyed = d.ToByteArray().Reverse().ToArray() ;

                privateKey = dByte.Concat(keyed).Concat(nByte).Concat(keyN).ToArray();
            }
        

            PrivateKey savePrivateKey = new(Convert.ToBase64String(privateKey));
            var privateKeyJson = JsonSerializer.Serialize(savePrivateKey);
            File.WriteAllText("private.key", privateKeyJson, Encoding.UTF8);

            PublicKey savePublicKey = new(Convert.ToBase64String(publicKey));
            var publicKeyJson = JsonSerializer.Serialize(savePublicKey);
            File.WriteAllText("public.key", publicKeyJson, Encoding.UTF8);
        }
        
        /// <summary>
        /// mod inverse function
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns>mod inverse of a mod n</returns>
        static BigInteger modInverse(BigInteger a, BigInteger n){
            BigInteger i = n, v = 0, d = 1;
            while (a>0) {
                BigInteger t = i/a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t*x;
                v = x;
                }
            v %= n;
            if (v<0) v = (v+n)%n;
            return v;
        }
    }
    
    /// <summary>
    /// Representation of a RSA private key and corresponding email
    /// </summary>
    public class PrivateKey{
        /// <summary>
        /// propery for email, type string 
        /// </summary>
        public List<string> emails {get; set;}
        /// <summary>
        /// private key
        /// </summary>
        public string key {get; set;}

        /// <summary>
        /// creates a privatekey object with email and key items
        /// </summary>
        /// <param name="key"></param>
        public PrivateKey(string key){
            emails = new List<string>();
            this.key = key;
        }
    }

    /// <summary>
    /// Representation of a RSA public key and corresponding email
    /// </summary>
    public class PublicKey{
        /// <summary>
        /// email of type string
        /// </summary>
        public string email {get; set;}
        /// <summary>
        /// key of type string
        /// </summary>
        public string key {get; set;}

        /// <summary>
        /// creates a publickey object with email and key items
        /// </summary>
        /// <param name="key"></param>
        public PublicKey(string key)
        {
            email = "";
            this.key = key;
        }
    }

    /// <summary>
    /// Representation of a message block
    /// </summary>
    public class Message{
        /// <summary>
        /// email of type string
        /// </summary>
        public string email {get; set;}
        /// <summary>
        /// message content of type string
        /// </summary>
        public string content {get; set;}

        /// <summary>
        /// Creates a message with email and content
        /// </summary>
        /// <param name="email"> email of the user </param>
        /// <param name="content"> message being sent / recieved </param>
        public Message(string email, string content)
        {
            this.email = email;
            this.content = content;
        }
    }

    /// <summary>
    /// contains all possible options a user could do
    /// </summary>
    public class MessageOptions
    {
        static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// sends the public key generated in KeyGen along with a given email
        /// </summary>
        /// <param name="email"> email to be recorded with the key </param>
        /// <returns> returns the asynchronous task state </returns>
        public async Task sendKey(string email){
            try{
                if (File.Exists("public.key") && File.Exists("private.key")){
                    string privateKeyFile = File.ReadAllText("private.key");
    
                    var saveEmail = JsonSerializer.Deserialize<PrivateKey>(privateKeyFile);
                    if (!saveEmail.emails.Contains(email)){
                        saveEmail.emails.Add(email);
                    }

                    var updateEmail = JsonSerializer.Serialize(saveEmail);
                    File.WriteAllText("private.key", updateEmail, Encoding.UTF8);

                    string publicKeyFile = File.ReadAllText("public.key");
                    var text = JsonSerializer.Deserialize<PublicKey>(publicKeyFile);
                    text.email = email;
                    var content = new StringContent(JsonSerializer.Serialize(text), Encoding.UTF8, "application/json");
                 
                    using HttpResponseMessage response = await client.PutAsync("http://kayrun.cs.rit.edu:5000/Key/" + email, content);

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Key saved");
                } else{
                    Console.WriteLine("key does not exist");
                    return;
                }
            }
            catch(HttpRequestException e){
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// retrieve public key for a particular user given their email
        /// </summary>
        /// <param name="email"> email of the user that you are retrieving a key for </param>
        /// <returns> returns the asynchronous task state </returns>
        public async Task getKey(string email)
        {
            try{
                using HttpResponseMessage response = await client.GetAsync("http://kayrun.cs.rit.edu:5000/Key/" + email);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                File.WriteAllText(email + ".key", responseBody);
            }
            catch(HttpRequestException e){
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// sends a message to the user with the corresponding email
        /// </summary>
        /// <param name="email"> email of the user you are sending a message to </param>
        /// <param name="message"> message you are sending </param>
        /// <returns> returns the asynchronous task state </returns>
        public async Task sendMsg(string email, string message)
        {
            try{
                BigInteger E;
                int e;
                BigInteger N;
                int n;

                if (File.Exists(email + ".key")){
                    string file = File.ReadAllText(email + ".key");
                    var key = JsonSerializer.Deserialize<PublicKey>(file);
                    var temp = Convert.FromBase64String(key.key);
                    if (BitConverter.IsLittleEndian) {
                        e = BitConverter.ToInt32(temp.ToList().GetRange(0, 4).ToArray().Reverse().ToArray());
                        E = new(temp.ToList().GetRange(4,e).ToArray());

                        n = BitConverter.ToInt32(temp.ToList().GetRange(4 + e, 4).ToArray().Reverse().ToArray());
                        N = new(temp.ToList().GetRange(4 + e + 4,n).ToArray());
                    }
                    else{
                        e = BitConverter.ToInt32(temp.ToList().GetRange(0, 4).ToArray());
                        E = new(temp.ToList().GetRange(4,e).ToArray().Reverse().ToArray());

                        n = BitConverter.ToInt32(temp.ToList().GetRange(4 + e,4).ToArray());
                        N = new(temp.ToList().GetRange(4 + e + 4, n).ToArray().Reverse().ToArray());
                    }
                    
                    BigInteger messageArray = new(Encoding.UTF8.GetBytes(message));
                    
                    Message sendMessage = new(email, Convert.ToBase64String( BigInteger.ModPow(messageArray, E, N).ToByteArray()));
                   
                    var content = new StringContent(JsonSerializer.Serialize(sendMessage), Encoding.UTF8, "application/json");
                    using HttpResponseMessage response = await client.PutAsync("http://kayrun.cs.rit.edu:5000/Message/" + email, content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Message written");
                }
                else{
                    Console.WriteLine("Key does not exist for " + email);
                    return;
                }
            
            }
            catch(HttpRequestException e){
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// retrieve a message from a user
        /// </summary>
        /// <param name="email"> email of the user whose message you are retrieving </param>
        /// <returns> returns the asynchronous task state </returns>
        public async Task getMsg(string email)
        {
            try{
                BigInteger N;
                int n;
                BigInteger D;
                int d;

                if (File.Exists("private.key")){
                    string file = File.ReadAllText("private.key");
                    var key = JsonSerializer.Deserialize<PrivateKey>(file);
                    if (!key.emails.Contains(email)){
                        Console.WriteLine("Key does not exist for " + email);
                        return;
                    }
                    var temp = Convert.FromBase64String(key.key);
                    if (BitConverter.IsLittleEndian) {
                        d = BitConverter.ToInt32(temp.ToList().GetRange(0, 4).ToArray().Reverse().ToArray());
                        D = new(temp.ToList().GetRange(4,d).ToArray());

                        n = BitConverter.ToInt32(temp.ToList().GetRange(4 + d, 4).ToArray().Reverse().ToArray());
                        N = new(temp.ToList().GetRange(4 + n + 4,n).ToArray());

                    }
                    else{
                        d = BitConverter.ToInt32(temp.ToList().GetRange(0, 4).ToArray());
                        D = new(temp.ToList().GetRange(4,d).ToArray().Reverse().ToArray());

                        n = BitConverter.ToInt32(temp.ToList().GetRange(4 + d,4).ToArray());
                        N = new(temp.ToList().GetRange(4 + n + 4, n).ToArray().Reverse().ToArray());
                    }

                    using HttpResponseMessage response = await client.GetAsync("http://kayrun.cs.rit.edu:5000/Message/" + email);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();   
                    
                    var cereal = JsonSerializer.Deserialize<Message>(responseBody);

                    BigInteger message = new (Convert.FromBase64String(cereal.content));
                    string decodedMessage = Encoding.UTF8.GetString( BigInteger.ModPow(message, D, N).ToByteArray());

                    Console.WriteLine(decodedMessage);
                }
                else
                {
                    Console.WriteLine("Key does not exist for " + email);
                    return;
                }
            }
            catch(HttpRequestException e){
                Console.WriteLine(e.Message);
            }
        }

    }


    /// <summary>
    /// Holds the main function 
    /// </summary>
    public class Program {
        /// <summary>
        /// message to be printed on input error
        /// </summary>
        
        private const string helpMessage =  "Usage: <options> <other arguments> \n" +
                                            "keyGen <keysize> - generates a keypair size of keysize bits \n" +
                                            "sendKey <email> - sends the public key with the email address \n" +
                                            "getKey <email> - retrieve a public key for a particular user \n" +
                                            "sendMsg <email> <message> - take a plaintext message, encrypt it, and send it to the user \n" +
                                            "getMsg <email> - retrieve a message for a particular user \n";


        /// <summary>
        /// Main function for program PrimeGen
        /// </summary>
        /// <param name="userInput"></param>
        public static async Task Main(string[] userInput){ 
            
            string options;
            string email;
            string message;

            if ( userInput.Length < 1 ){
                Console.WriteLine(helpMessage);
                return;
            }
            else{
                options = userInput[0] ; 
            }
           
            MessageOptions server = new();

            switch (options)
            {
                case "keyGen":
                    if (userInput.Length != 2)
                    {
                        Console.WriteLine(helpMessage);
                        break;
                    }
                    KeyGeneration generateKey = new();
                    int keysize = int.Parse(userInput[1]) / 8;
                    generateKey.KeyGen(keysize);
                    break;
                case "sendKey":
                    if (userInput.Length != 2)
                    {
                        Console.WriteLine(helpMessage);
                        break;
                    }
                    email = userInput[1];
                    await server.sendKey(email);
                    break;
                case "getKey":
                    if (userInput.Length != 2)
                    {
                        Console.WriteLine(helpMessage);
                        break;
                    }
                    email = userInput[1];
                    await server.getKey(email);
                    break;
                case "sendMsg":
                    if (userInput.Length != 3)
                    {
                        Console.WriteLine(helpMessage);
                        break;
                    }
                    email = userInput[1];
                    message = userInput[2];
                    await server.sendMsg(email, message);
                    break;
                case "getMsg":
                    if (userInput.Length != 2)
                    {
                        Console.WriteLine(helpMessage);
                        break;
                    }
                    email = userInput[1];
                    await server.getMsg(email);
                    break;
                default:
                    Console.WriteLine(helpMessage);
                    break;
            }
        }
    }
}