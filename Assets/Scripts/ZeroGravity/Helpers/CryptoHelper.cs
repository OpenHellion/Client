using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace ZeroGravity.Helpers
{
	internal static class CryptoHelper
	{
		public static byte[] EncryptRSA(string PublicKey, byte[] plain)
		{
			using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(2048))
			{
				rSACryptoServiceProvider.FromXmlString(PublicKey);
				return rSACryptoServiceProvider.Encrypt(plain, false);
			}
		}

		public static byte[] DecryptRSA(string PrivateKey, byte[] cipher)
		{
			using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(2048))
			{
				rSACryptoServiceProvider.FromXmlString(PrivateKey);
				return rSACryptoServiceProvider.Decrypt(cipher, false);
			}
		}

		public static byte[] EncryptRijndael(byte[] data, byte[] key, out byte[] iv)
		{
			MemoryStream memoryStream = new MemoryStream();
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.KeySize = 256;
			rijndaelManaged.BlockSize = 256;
			rijndaelManaged.Padding = PaddingMode.Zeros;
			rijndaelManaged.Mode = CipherMode.CBC;
			rijndaelManaged.GenerateIV();
			iv = rijndaelManaged.IV;
			ICryptoTransform transform = rijndaelManaged.CreateEncryptor(key, rijndaelManaged.IV);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			cryptoStream.Write(data, 0, data.Length);
			cryptoStream.FlushFinalBlock();
			byte[] result = memoryStream.ToArray();
			memoryStream.Close();
			cryptoStream.Close();
			return result;
		}

		public static byte[] DecryptRijndael(byte[] data, byte[] key, byte[] iv)
		{
			int num = 0;
			MemoryStream memoryStream = new MemoryStream(data);
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.KeySize = 256;
			rijndaelManaged.BlockSize = 256;
			rijndaelManaged.Padding = PaddingMode.Zeros;
			rijndaelManaged.Mode = CipherMode.CBC;
			ICryptoTransform transform = rijndaelManaged.CreateDecryptor(key, iv);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
			byte[] array = new byte[memoryStream.Length];
			num = cryptoStream.Read(array, 0, array.Length);
			byte[] array2 = new byte[num];
			Array.Copy(array, 0, array2, 0, num);
			return array2;
		}

		/// <summary>
		/// 	Gets the public key of the supplied stream.
		/// </summary>
		public static string ExchangePublicKeys(NetworkStream stream, string publicKey)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(publicKey);
			uint value = (uint)bytes.Length;
			stream.Write(BitConverter.GetBytes(value), 0, 4);
			stream.Write(bytes, 0, bytes.Length);
			stream.Flush();
			bytes = readBytes(stream, 4);
			value = BitConverter.ToUInt32(bytes, 0);
			if (value == 0)
			{
				throw new Exception("Exchange public keys error.");
			}

			bytes = readBytes(stream, (int)value);
			return Encoding.UTF8.GetString(bytes);
		}

		public static byte[] ReadRequest(NetworkStream stream, string privateKey)
		{
			byte[] value = readBytes(stream, 4);
			uint num = BitConverter.ToUInt32(value, 0);
			if (num == 0)
			{
				throw new Exception("Read request error.");
			}

			value = readBytes(stream, (int)num);
			byte[] key = DecryptRSA(privateKey, value);
			value = readBytes(stream, 4);
			num = BitConverter.ToUInt32(value, 0);
			if (num == 0)
			{
				throw new Exception("Read request error.");
			}

			value = readBytes(stream, (int)num);
			byte[] iv = DecryptRSA(privateKey, value);
			value = readBytes(stream, 4);
			num = BitConverter.ToUInt32(value, 0);
			if (num == 0)
			{
				throw new Exception("Read request error.");
			}

			value = readBytes(stream, (int)num);
			return DecryptRijndael(value, key, iv);
		}

		public static byte[] ReadResponse(NetworkStream stream, string privateKey)
		{
			byte[] value = readBytes(stream, 4);
			uint num = BitConverter.ToUInt32(value, 0);
			if (num == 0)
			{
				throw new Exception("Read main server response error.");
			}

			value = readBytes(stream, (int)num);
			byte[] key = DecryptRSA(privateKey, value);
			value = readBytes(stream, 4);
			num = BitConverter.ToUInt32(value, 0);
			if (num == 0)
			{
				throw new Exception("Read main server response error.");
			}

			value = readBytes(stream, (int)num);
			byte[] iv = DecryptRSA(privateKey, value);
			value = readBytes(stream, 4);
			num = BitConverter.ToUInt32(value, 0);
			if (num == 0)
			{
				throw new Exception("Read main server response error.");
			}

			value = readBytes(stream, (int)num);
			return DecryptRijndael(value, key, iv);
		}

		public static void WriteRequest(NetworkStream stream, byte[] data, string remotePublicKey)
		{
			byte[] key = SymmetricAlgorithm.Create().Key;
			byte[] iv;
			byte[] array = EncryptRijndael(data, key, out iv);
			byte[] array2 = EncryptRSA(remotePublicKey, key);
			uint value = (uint)array2.Length;
			stream.Write(BitConverter.GetBytes(value), 0, 4);
			stream.Write(array2, 0, array2.Length);
			array2 = EncryptRSA(remotePublicKey, iv);
			value = (uint)array2.Length;
			stream.Write(BitConverter.GetBytes(value), 0, 4);
			stream.Write(array2, 0, array2.Length);
			value = (uint)array.Length;
			stream.Write(BitConverter.GetBytes(value), 0, 4);
			stream.Write(array, 0, array.Length);
			stream.Flush();
		}

		public static void WriteResponse(NetworkStream stream, byte[] data, string remotePublicKey)
		{
			byte[] key = SymmetricAlgorithm.Create().Key;
			byte[] iv;
			byte[] array = EncryptRijndael(data, key, out iv);
			byte[] array2 = EncryptRSA(remotePublicKey, key);
			uint value = (uint)array2.Length;
			stream.Write(BitConverter.GetBytes(value), 0, 4);
			stream.Write(array2, 0, array2.Length);
			array2 = EncryptRSA(remotePublicKey, iv);
			value = (uint)array2.Length;
			stream.Write(BitConverter.GetBytes(value), 0, 4);
			stream.Write(array2, 0, array2.Length);
			value = (uint)array.Length;
			stream.Write(BitConverter.GetBytes(value), 0, 4);
			stream.Write(array, 0, array.Length);
			stream.Flush();
		}

		public static void GenerateRSAKeys(out string publicKey, out string privateKey)
		{
			RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(2048);
			publicKey = rSACryptoServiceProvider.ToXmlString(false);
			privateKey = rSACryptoServiceProvider.ToXmlString(true);
		}

		private static byte[] readBytes(Stream stream, int size)
		{
			byte[] array = new byte[size];
			int num = 0;
			do
			{
				int num2 = stream.Read(array, num, size - num);
				if (num2 == 0)
				{
					throw new Exception("Error reading stream.");
				}

				num += num2;
			} while (num < size);

			return array;
		}
	}
}
