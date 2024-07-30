using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using EncryptionDecryptionLibrary;
using dotNetClassLibrary;
using System.Text.RegularExpressions;
using System.Net.Sockets;

public static class EncryptionDecryption2
{
    public static byte[] validXMLDataResponse = new byte[] { 20, 28, 46, 72, 225, 175, 220, 8, 97, 99, 107, 3, 0, 0, 0, 60, 88, 77, 76, 68, 105, 115, 112, 108, 97, 121, 68, 97, 116, 97, 32, 78, 97, 109, 101, 61, 34, 77, 111, 99, 107, 68, 105, 115, 112, 108, 97, 121, 34, 32, 80, 97, 116, 104, 61, 34, 71, 114, 97, 112, 104, 105, 99, 115, 46, 68, 105, 115, 112, 108, 97, 121, 115, 34, 32, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 61, 34, 49, 46, 49, 46, 49, 46, 50, 34, 62, 13, 10, 32, 32, 60, 77, 101, 116, 97, 100, 97, 116, 97, 62, 13, 10, 32, 32, 32, 32, 60, 76, 97, 115, 116, 69, 100, 105, 116, 85, 115, 101, 114, 62, 68, 86, 65, 68, 77, 73, 78, 60, 47, 76, 97, 115, 116, 69, 100, 105, 116, 85, 115, 101, 114, 62, 13, 10, 32, 32, 32, 32, 60, 76, 97, 115, 116, 69, 100, 105, 116, 84, 105, 109, 101, 62, 50, 48, 50, 52, 45, 48, 55, 45, 50, 57, 84, 49, 53, 58, 49, 53, 58, 50, 57, 46, 51, 50, 56, 50, 52, 50, 57, 45, 48, 53, 58, 48, 48, 60, 47, 76, 97, 115, 116, 69, 100, 105, 116, 84, 105, 109, 101, 62, 13, 10, 32, 32, 60, 47, 77, 101, 116, 97, 100, 97, 116, 97, 62, 13, 10, 32, 32, 60, 80, 111, 108, 121, 103, 111, 110, 32, 78, 97, 109, 101, 61, 34, 67, 111, 110, 116, 114, 111, 108, 80, 111, 108, 121, 103, 111, 110, 34, 62, 13, 10, 32, 32, 32, 32, 60, 72, 101, 105, 103, 104, 116, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 51, 50, 48, 56, 57, 54, 56, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 72, 101, 105, 103, 104, 116, 62, 13, 10, 32, 32, 32, 32, 60, 87, 105, 100, 116, 104, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 51, 50, 57, 49, 54, 50, 53, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 87, 105, 100, 116, 104, 62, 13, 10, 32, 32, 32, 32, 60, 88, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 52, 49, 49, 49, 54, 55, 55, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 88, 62, 13, 10, 32, 32, 32, 32, 60, 89, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 49, 54, 53, 53, 51, 57, 54, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 89, 62, 13, 10, 32, 32, 60, 47, 80, 111, 108, 121, 103, 111, 110, 62, 13, 10, 32, 32, 60, 79, 112, 101, 114, 97, 116, 105, 110, 103, 80, 101, 114, 99, 101, 110, 116, 97, 103, 101, 62, 56, 57, 60, 47, 79, 112, 101, 114, 97, 116, 105, 110, 103, 80, 101, 114, 99, 101, 110, 116, 97, 103, 101, 62, 13, 10, 32, 32, 60, 85, 110, 114, 101, 115, 111, 108, 118, 101, 100, 73, 115, 115, 117, 101, 115, 62, 49, 48, 60, 47, 85, 110, 114, 101, 115, 111, 108, 118, 101, 100, 73, 115, 115, 117, 101, 115, 62, 13, 10, 60, 47, 88, 77, 76, 68, 105, 115, 112, 108, 97, 121, 68, 97, 116, 97, 62 };
    
    
    
    
    static void Main(string[] args)
    {
        byte[] key = EncryptionDecryption.generateRandomAESKey();
        byte[] iv = EncryptionDecryption.generateRandomAESIV();

        byte[] original = validXMLDataResponse;//new byte[] { 108, 195, 164, 209, 222, 175, 220, 8, 97, 99, 107, 1, 0, 0, 0, 60, 88, 77, 76, 68, 105, 115, 112, 108, 97, 121, 68, 97, 116, 97, 32, 78, 97, 109, 101, 61, 34, 77, 111, 99, 107, 68, 105, 115, 112, 108, 97, 121, 34, 32, 80, 97, 116, 104, 61, 34, 71, 114, 97, 112, 104, 105, 99, 115, 46, 68, 105, 115, 112, 108, 97, 121, 115, 34, 32, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 61, 34, 49, 46, 49, 46, 49, 46, 50, 34, 62, 13, 10, 32, 32, 60, 77, 101, 116, 97, 100, 97, 116, 97, 62, 13, 10, 32, 32, 32, 32, 60, 76, 97, 115, 116, 69, 100, 105, 116, 85, 115, 101, 114, 62, 68, 86, 65, 68, 77, 73, 78, 60, 47, 76, 97, 115, 116, 69, 100, 105, 116, 85, 115, 101, 114, 62, 13, 10, 32, 32, 32, 32, 60, 76, 97, 115, 116, 69, 100, 105, 116, 84, 105, 109, 101, 62, 50, 48, 50, 52, 45, 48, 55, 45, 50, 57, 84, 49, 52, 58, 53, 55, 58, 53, 49, 46, 52, 56, 54, 52, 51, 55, 50, 45, 48, 53, 58, 48, 48, 60, 47, 76, 97, 115, 116, 69, 100, 105, 116, 84, 105, 109, 101, 62, 13, 10, 32, 32, 60, 47, 77, 101, 116, 97, 100, 97, 116, 97, 62, 13, 10, 32, 32, 60, 80, 111, 108, 121, 103, 111, 110, 32, 78, 97, 109, 101, 61, 34, 67, 111, 110, 116, 114, 111, 108, 80, 111, 108, 121, 103, 111, 110, 34, 62, 13, 10, 32, 32, 32, 32, 60, 72, 101, 105, 103, 104, 116, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 50, 53, 51, 49, 50, 56, 48, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 72, 101, 105, 103, 104, 116, 62, 13, 10, 32, 32, 32, 32, 60, 87, 105, 100, 116, 104, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 50, 57, 57, 51, 55, 55, 48, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 87, 105, 100, 116, 104, 62, 13, 10, 32, 32, 32, 32, 60, 88, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 51, 55, 53, 57, 49, 53, 57, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 88, 62, 13, 10, 32, 32, 32, 32, 60, 89, 62, 13, 10, 32, 32, 32, 32, 32, 32, 60, 86, 97, 108, 117, 101, 62, 49, 53, 50, 54, 48, 49, 49, 60, 47, 86, 97, 108, 117, 101, 62, 13, 10, 32, 32, 32, 32, 60, 47, 89, 62, 13, 10, 32, 32, 60, 47, 80, 111, 108, 121, 103, 111, 110, 62, 13, 10, 32, 32, 60, 79, 112, 101, 114, 97, 116, 105, 110, 103, 80, 101, 114, 99, 101, 110, 116, 97, 103, 101, 62, 57, 57, 60, 47, 79, 112, 101, 114, 97, 116, 105, 110, 103, 80, 101, 114, 99, 101, 110, 116, 97, 103, 101, 62, 13, 10, 32, 32, 60, 85, 110, 114, 101, 115, 111, 108, 118, 101, 100, 73, 115, 115, 117, 101, 115, 62, 49, 57, 60, 47, 85, 110, 114, 101, 115, 111, 108, 118, 101, 100, 73, 115, 115, 117, 101, 115, 62, 13, 10, 60, 47, 88, 77, 76, 68, 105, 115, 112, 108, 97, 121, 68, 97, 116, 97, 62};
        foreach (byte b in original) Console.Write(b + " "); Console.WriteLine("\n");
        byte[] encrypted = EncryptionDecryption.AESEncrypt(original, key, iv);
        byte[] decrypted = EncryptionDecryption.AESDecrypt(encrypted, key, iv);
        foreach (byte b in decrypted) Console.Write(b + " ");




    }
    static void Main2(string[] args)
    {

        uint count = 0;
        byte[] AESKey = EncryptionDecryption.generateRandomAESKey();


        //start of encrypted timed pinger. 
        byte[] sendBytes = ProcessContent.convertToTimestampedBytes(count, ';', "Request For Data");

        byte[] AESIV = EncryptionDecryption.generateRandomAESIV();
        sendBytes = EncryptionDecryption.AESEncrypt(sendBytes, AESKey, AESIV);

        //first 16 bytes reserved to add the IV
        byte[] final = new byte[sendBytes.Length + 16];
        Buffer.BlockCopy(AESIV, 0, final, 0, AESIV.Length);
        Buffer.BlockCopy(sendBytes, 0, final, 16, sendBytes.Length);
        //end of timed pinger, result is the byte[] final. 




        //start of listening program
        byte[] bytes = final; 

        byte[] IV = new byte[16];

        //Encrypted byte[] message
        byte[] byteMessage = new byte[bytes.Length - IV.Length];

        //todo: there's gotta be a better way to do this...
        Buffer.BlockCopy(bytes, 0, IV, 0, IV.Length);
        Buffer.BlockCopy(bytes, IV.Length, byteMessage, 0, byteMessage.Length);
        

        //Note: doing a new var for decryptedBytes is important because byteMessage and DecryptedBytes are of different sizes -- I think...???
        byte[] decryptedBytes = EncryptionDecryption.AESDecrypt(byteMessage, AESKey, IV);

        (uint num, char delim, string message, DateTime sendTime) = ProcessContent.convertFromTimestampedBytes(decryptedBytes);

        //ProcessContent.sendACK(decryptedBytes, "127.0.0.1", AESKey, IV);
        //end of listening program, goes to send ack. 




        //start of send ack function
        byte[] recieved = decryptedBytes;
        byte[] iv = IV;
        byte[] key = AESKey;


        var parsed = ProcessContent.convertFromTimestampedBytes(recieved);
        byte[] uintbytes = BitConverter.GetBytes(parsed.Item1);
        byte[] xmlBytes = ProcessContent.genXMLBytes();
        byte[] ack = Encoding.ASCII.GetBytes("ack");

        byte[] final2 = new byte[ack.Length + 8 + 4 + xmlBytes.Length]; //8 comes from timestamp, 4 comes from uint

        //this format so far: timestamp, "ack", uint message #, xml message --> 8, 3, 4, possibly a bunch of bytes
        byte[] timestampReformatted = BitConverter.GetBytes(parsed.Item4.Ticks);
        Buffer.BlockCopy(timestampReformatted, 0, final2, 0, 8); //index 0-7            -- This is supposed to send over the original timestamp 

        Buffer.BlockCopy(ack, 0, final2, 8, ack.Length); //index 8-10
        Buffer.BlockCopy(uintbytes, 0, final2, 8 + ack.Length, uintbytes.Length); //index 11-14
        Buffer.BlockCopy(xmlBytes, 0, final2, 8 + 3 + 4, xmlBytes.Length); //index 15 - 15+xmlLength


        Console.WriteLine("Before encryption:");
        foreach (byte thing in final2) Console.Write(thing + ", ");
        Console.WriteLine("\n\n");


        (uint a, char b, string c, DateTime d) = ProcessContent.convertFromTimestampedBytes(final2);
        Console.WriteLine(a + " " + b + " " + c + " " + d);


        /*
        Console.WriteLine("\n\n\n");
        foreach (byte b in final2) Console.Write(b+" "); // this should be the unencrypted message without IV
        Console.WriteLine("\n\n\n");
        */

        byte[] encryptedMessage = EncryptionDecryption.AESEncrypt(final2, key, iv);
        /*
        byte[] attemptedDecrypt = EncryptionDecryption.AESDecrypt(encryptedMessage, key, iv);

        Console.WriteLine("\n\n\n");
        foreach (byte b in attemptedDecrypt) Console.Write(b + " "); // this should be the unencrypted message without IV
        Console.WriteLine("\n\n\n");
        */










        byte[] finalWithIVPlainText = new byte[encryptedMessage.Length + 16];

        iv = EncryptionDecryption.generateRandomAESIV();



        Buffer.BlockCopy(iv, 0, finalWithIVPlainText, 0, iv.Length);
        Buffer.BlockCopy(encryptedMessage, 0, finalWithIVPlainText, iv.Length, encryptedMessage.Length);


        Console.WriteLine("Encrypted Response Sent.");
        byte[] test = EncryptionDecryption.parseAndDecrypt(finalWithIVPlainText, AESKey);


        Console.WriteLine("After encryption:");
        foreach (byte thing in test) Console.Write(thing + " ");
        Console.WriteLine("\n\n");



        (uint e, char f, string g, DateTime h)= ProcessContent.convertFromTimestampedBytes(test);
        Console.WriteLine(e+" "+f+" "+g+" "+h);





        //Notes: interesting. the parse and decrypt doesn't seem to work properly because the decrypted bytes are different for the first 16 bytes than the orig unencrypted. will try isolating prob further. 
        //Note: found prob. decryption doesn't return orig for some reason. 







        /*
        byte[] AESKey = EncryptionDecryption.generateRandomAESKey();
        byte[] IV = EncryptionDecryption.generateRandomAESIV();
        byte[] final = Encoding.ASCII.GetBytes("Login Failed!");
        byte[] encryptedMessage = EncryptionDecryption.AESEncrypt(final, AESKey, IV);

        byte[] finalWithIVPlainText = new byte[encryptedMessage.Length + 16];

        Buffer.BlockCopy(IV, 0, finalWithIVPlainText, 0, IV.Length);
        Buffer.BlockCopy(encryptedMessage, 0, finalWithIVPlainText, IV.Length, encryptedMessage.Length);

        byte[] after = EncryptionDecryption.parseAndDecrypt(finalWithIVPlainText, AESKey);
        Console.WriteLine(Encoding.ASCII.GetString(after));*/
    }

    /*byte[] final = Encoding.ASCII.GetBytes("Login Failed!");
    byte[] encryptedMessage = EncryptionDecryption.AESEncrypt(final, AESKey, IV);

    byte[] finalWithIVPlainText = new byte[encryptedMessage.Length + 16];

    byte[] iv = EncryptionDecryption.generateRandomAESIV();

    Buffer.BlockCopy(iv, 0, finalWithIVPlainText, 0, iv.Length);
    Buffer.BlockCopy(encryptedMessage, 0, finalWithIVPlainText, iv.Length, encryptedMessage.Length);
    */
}

