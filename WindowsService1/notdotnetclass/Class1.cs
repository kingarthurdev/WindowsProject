using System;
using System.Security.Cryptography;

public class MyClass
{
    public static (string publicKey, string privateKey) GenerateRSAKeys()
    {
        using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
        {
            // Export the key information to an RSAParameters object.
            // Pass false to export the public key information or pass
            // true to export public and private key information.
            RSAParameters RSAParams = RSA.ExportParameters(true);

            // Assuming RSA.ExportRSAPublicKeyPem() and RSA.ExportRSAPrivateKeyPem() are custom methods
            string publicKey = RSA.ExportRSAPrivateKeyPem();
            string privateKey = RSA.ExportRSAPrivateKeyPem();

            // Console output for testing purposes
            Console.WriteLine("Public Key:");
            Console.WriteLine(publicKey);
            Console.WriteLine("Private Key:");
            Console.WriteLine(privateKey);

            // Return the keys as a tuple
            return (publicKey, privateKey);
        }
    }

    public static void Main()
    {
        var keys = GenerateRSAKeys();
        // Use keys.publicKey and keys.privateKey as needed
    }
}
