# RsaJwk
JWK serialization in C# for RSA keys using OAEP padding

## JWK Serialization

The types in this sample code enable serializing and deserializing RSA keys to/from the JWK format, with these restrictions:

* Only RSA keys with 2 primes are supported.
* Only OAEP padding is supported.

The types involved are:

* `RsaJwk` - The in-memory representation of an RSA key in JWK format.
* `Base64UrlConverter` - A helper type for JSON.NET to format JWK byte arrays.
* `RsaJwkExtensions` - Converts `RsaJwk` to and from a `(RSA, RSAEncryptionPadding)` pair.

## Example Application

This project has two parts: a .NET Core console application and a web page. The web page uses a public RSA key to encrypt a value, which can then be pasted into the console application. The console application uses the corresponding private RSA key to decrypt the value.

Specifically, the console application will:

1. Deserialize an `(RSA, RSAEncryptionPadding)` pair from a JWK `key.json` file. If the file does not exist, it will create a new `RSA` key and serialize it into the `key.json` file. This file contains the **private** key and should not be shared!
1. Serializes the public RSA key into a JWK `key.public.json` file, and also writes it directly into the `encrypt.html` file.
1. Opens `encrypt.html` in a web browser.
1. Allows the user to paste encrypted, Base64-encoded text, and decodes and decrypts it.

## Algorithm Specifics

To encrypt, the web page takes the user input, [encodes it as UTF-8](https://developer.mozilla.org/en-US/docs/Web/API/TextEncoder), [encrypts it with the RSA public key](https://developer.mozilla.org/en-US/docs/Web/API/Web_Crypto_API), and [Base64-encodes](https://github.com/beatgammit/base64-js) the result.

To decrypt, the console application takes the user input, Base64-decodes it, decrypts it with the RSA private key, and UTF8-decodes the result.

## Caveats

This example of asymmetrical encryption uses the RSA keys directly for all encryption. This is slow, and should only be done on very small data values. Larger data values should generate a symmetric key, encrypt that with the RSA key, and use the symmetrical key to encrypt the large amount of data.
