using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace USBVault.Services
{
    public static class UsbKeyManager
    {
        private const string KeyFileName = "vault.key";
        private const uint MagicNumber = 0x55AAFF01;

        public static void CreateKey(string driveLetter, out byte[] aesKey, out string vaultId)
        {
            aesKey = RandomNumberGenerator.GetBytes(32);
            vaultId = Guid.NewGuid().ToString();

            using var fs = new FileStream($"{driveLetter}:\\vault.key", FileMode.Create);
            using var bw = new BinaryWriter(fs);

            bw.Write(MagicNumber);
            bw.Write((byte)1);
            bw.Write(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            bw.Write(aesKey);
            bw.Write(vaultId); // Добавляем идентификатор
        }

        public static (byte[] Key, string VaultId)? TryReadKey(string driveLetter)
        {
            var path = $"{driveLetter}:\\vault.key";
            if (!File.Exists(path)) return null;

            using var fs = new FileStream(path, FileMode.Open);
            using var br = new BinaryReader(fs);

            if (br.ReadUInt32() != MagicNumber) return null;
            br.ReadByte(); // Версия
            br.ReadInt64(); // Дата
            var key = br.ReadBytes(32);
            var vaultId = br.ReadString();

            return (key, vaultId);
        }
    }
}