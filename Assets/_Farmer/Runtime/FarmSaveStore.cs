using System;
using System.IO;
using UnityEngine;

namespace Farmer
{
    public sealed class FarmSaveStore
    {
        public string Path { get; }
        private readonly Func<FarmSnapshot, FarmModel> validate;
        public FarmSaveStore(string path, Func<FarmSnapshot, FarmModel> validator) { Path = path; validate = validator; }

        public FarmModel Load(out bool recovered)
        {
            recovered = false;
            if (!File.Exists(Path) && !File.Exists(Path + ".bak")) return null;
            if (TryRead(Path, out var model)) return model;
            if (TryRead(Path + ".bak", out model)) { recovered = true; return model; }
            throw new InvalidDataException("Save and backup are unreadable or unsupported; originals preserved.");
        }

        public void Save(FarmSnapshot snapshot)
        {
            validate(snapshot);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            string temporary = Path + ".tmp";
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(JsonUtility.ToJson(snapshot, true)); writer.Flush(); stream.Flush(true);
            }
            if (!File.Exists(Path)) { File.Move(temporary, Path); return; }
            if (TryRead(Path, out _)) File.Replace(temporary, Path, Path + ".bak");
            else
            {
                // Keep the corrupt primary for recovery and never replace the known-good backup with it.
                File.Copy(Path, Path + ".corrupt-" + Guid.NewGuid().ToString("N"));
                File.Replace(temporary, Path, null);
            }
        }

        private bool TryRead(string path, out FarmModel model)
        {
            model = null;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 1024 * 1024) return false;
                model = validate(JsonUtility.FromJson<FarmSnapshot>(File.ReadAllText(path)));
                return true;
            }
            catch (Exception e) when (e is IOException || e is ArgumentException || e is UnauthorizedAccessException)
            { return false; }
        }
    }
}
