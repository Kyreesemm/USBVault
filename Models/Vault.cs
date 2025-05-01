using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using USBVault.Services;

namespace USBVault.Models
{
    public class Vault
    {
        private const string VaultFile = "vault.dat";
        private List<Note> _notes = new List<Note>();
        private string _vaultId;

        public void AddNote(Note note, byte[] key)
        {
            _notes.Add(note);
            SaveNotes(key);
        }

        public void RemoveNote(Note note, byte[] key)
        {
            _notes.Remove(note);
            SaveNotes(key);
        }

        public List<Note> GetNotes(byte[] key)
        {
            return _notes;
        }

        public void LoadNotes(byte[] key, string vaultId)
        {
            _vaultId = vaultId;
            if (!File.Exists(VaultFile)) return;

            var encryptedData = File.ReadAllBytes(VaultFile);
            var decryptedJson = NoteCrypto.Decrypt(encryptedData, key);
            var data = JsonSerializer.Deserialize<VaultData>(decryptedJson);

            if (data.VaultId == vaultId)
                _notes = data.Notes;
            else
                _notes = new List<Note>();
        }

        private void SaveNotes(byte[] key)
        {
            var data = new VaultData { VaultId = _vaultId, Notes = _notes };
            var json = JsonSerializer.Serialize(data);
            var encryptedData = NoteCrypto.Encrypt(json, key);
            File.WriteAllBytes(VaultFile, encryptedData);
        }

        private class VaultData
        {
            public string VaultId { get; set; }
            public List<Note> Notes { get; set; }
        }
    }
}