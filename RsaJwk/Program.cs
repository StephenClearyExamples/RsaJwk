using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

// Supports reading and writing JWK-encoded RSA keys with two prime factors using OAEP padding.

namespace RsaJwk
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // https://stackoverflow.com/questions/6081946/why-does-console-readline-have-a-limit-on-the-length-of-text-it-allows/6081967#6081967
                Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));

                RSA rsa;
                RSAEncryptionPadding padding;
                if (File.Exists("key.json"))
                {
                    Console.WriteLine("Reading existing key.json...");
                    (rsa, padding) = JsonConvert.DeserializeObject<RsaJwk>(File.ReadAllText("key.json")).ToRSA();
                }
                else
                {
                    Console.WriteLine("Generating new key.json...");
                    Console.WriteLine("(You can also generate a key in Chrome and save it in key.json)");
                    /* To generate in Chrome:
                         window.crypto.subtle.generateKey(
                           {
                             name: "RSA-OAEP",
                             modulusLength: 4096,
                             publicExponent: new Uint8Array([0x01, 0x00, 0x01]),
                             hash: {name: "SHA-1"},
                           },
                           true,
                           ["encrypt", "decrypt"])
                         .then(key => window.crypto.subtle.exportKey("jwk", key.privateKey))
                         .then(keydata => console.log(JSON.stringify(keydata)))
                    */
                    rsa = RSA.Create(4096);
                    padding = RSAEncryptionPadding.OaepSHA1;
                    Console.WriteLine("Writing generated key.json...");
                    File.WriteAllText("key.json", JsonConvert.SerializeObject(rsa.ToJwk(padding, includePrivateKey: true)));
                }

                Console.WriteLine("Writing key.public.json...");
                var publicKey = JsonConvert.SerializeObject(rsa.ToJwk(padding, includePrivateKey: false));
                File.WriteAllText("key.public.json", publicKey);

                Console.WriteLine("Updating encrypt.html...");
                File.WriteAllText("encrypt.html", File.ReadAllText("encrypt.html").Replace("$$$PUBLIC_KEY$$$", publicKey));
                OpenBrowser("encrypt.html");

                while (true)
                {
                    try
                    {
                        Console.Write("Paste the encrypted, base64-encoded text: ");
                        var encryptedBase64Encoded = Console.ReadLine();
                        if (encryptedBase64Encoded == "")
                            return;
                        var encrypted = Convert.FromBase64String(encryptedBase64Encoded);
                        var decrypted = rsa.Decrypt(encrypted, padding);
                        var value = Encoding.UTF8.GetString(decrypted);
                        Console.WriteLine("Decrypted value: " + value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }

        // https://github.com/dotnet/corefx/issues/10361
        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("cmd", $"/C start {url}");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                Console.WriteLine($"Please open the {url} file in your browser.");
            }
        }
    }
}
