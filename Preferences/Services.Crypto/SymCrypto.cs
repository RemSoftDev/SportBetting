using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Preferences.Services.Crypto
{
	internal class SymCrypto
	{
		private Rijndael aes;

		public SymCrypto()
		{
			this.InitSymmetricCrypto("PrefTexFilePass1234....");
		}

		public string DecryptString(string toDecrypt)
		{
			byte[] numArray = new byte[toDecrypt.Length];
			byte[] numArray1 = Convert.FromBase64String(toDecrypt);
			MemoryStream memoryStream = new MemoryStream(numArray1);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, this.aes.CreateDecryptor(), CryptoStreamMode.Read);
			int num = cryptoStream.Read(numArray, 0, (int)numArray.Length);
			memoryStream.Close();
			cryptoStream.Close();
			string str = Encoding.UTF8.GetString(numArray, 0, num);
			return str;
		}

		public static object DeserializeCryptedObject(string fileName, BinaryFormatter formatter, SymCrypto crypto)
		{
			object obj = null;
			FileInfo fileInfo = new FileInfo(fileName);
			bool exists = !fileInfo.Exists;
			if (!exists)
			{
				FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				try
				{
					obj = formatter.Deserialize(crypto.WrapDecryptStream(fileStream));
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					Console.WriteLine(string.Concat("error deserializing:", exception.StackTrace));
				}
				fileStream.Close();
			}
			object obj1 = obj;
			return obj1;
		}

		public string EncryptString(string toEncrypt)
		{
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, this.aes.CreateEncryptor(), CryptoStreamMode.Write);
			byte[] bytes = Encoding.UTF8.GetBytes(toEncrypt);
			cryptoStream.Write(bytes, 0, (int)bytes.Length);
			cryptoStream.FlushFinalBlock();
			byte[] array = memoryStream.ToArray();
			memoryStream.Close();
			cryptoStream.Close();
			string base64String = Convert.ToBase64String(array);
			return base64String;
		}

		public void InitSymmetricCrypto(string passwd)
		{
			PasswordDeriveBytes passwordDeriveByte = new PasswordDeriveBytes(passwd, new byte[0]);
			this.aes = Rijndael.Create();
			this.aes.Key = passwordDeriveByte.GetBytes(32);
			this.aes.IV = passwordDeriveByte.GetBytes(16);
			this.aes.Mode = CipherMode.ECB;
			this.aes.Padding = PaddingMode.Zeros;
			this.aes.BlockSize = 0x100;
		}

		public static void SerializeCryptedObject(object o, string fileName, BinaryFormatter formatter, SymCrypto crypto)
		{
			MemoryStream memoryStream = new MemoryStream();
			formatter.Serialize(memoryStream, o);
			Stream stream = crypto.WrapEncryptStream(new FileStream(fileName, FileMode.Create));
			memoryStream.WriteTo(stream);
			stream.Close();
		}

		public CryptoStream WrapDecryptStream(Stream aStream)
		{
			CryptoStream cryptoStream = new CryptoStream(aStream, this.aes.CreateDecryptor(), CryptoStreamMode.Read);
			return cryptoStream;
		}

		public CryptoStream WrapEncryptStream(Stream aStream)
		{
			CryptoStream cryptoStream = new CryptoStream(aStream, this.aes.CreateEncryptor(), CryptoStreamMode.Write);
			return cryptoStream;
		}
	}
}