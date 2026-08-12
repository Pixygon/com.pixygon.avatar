using System;
using System.IO;
using System.Text;

namespace Pixygon.Avatar.Editor {
    /// <summary>
    /// Bakes an <see cref="ItemManifest"/> into a .glb's JSON chunk as
    /// <c>asset.extras.pixygonItem</c> — the step that makes one file BE the whole item
    /// (README: the portable item convention). Pure C#: byte-level GLB surgery, no glTF
    /// library needed. Idempotent: a file already carrying pixygonItem is left alone.
    /// </summary>
    public static class GlbManifestBaker {
        private const uint Magic = 0x46546C67;     // "glTF"
        private const uint JsonChunk = 0x4E4F534A; // "JSON"

        /// <summary>Inject the manifest. Returns false (with reason) instead of throwing.</summary>
        public static bool Bake(string glbPath, string manifestJson, out string error) {
            error = null;
            try {
                var bytes = File.ReadAllBytes(glbPath);
                if (bytes.Length < 20 || BitConverter.ToUInt32(bytes, 0) != Magic) {
                    error = "not a GLB (bad magic)";
                    return false;
                }
                uint version = BitConverter.ToUInt32(bytes, 4);
                uint chunk0Len = BitConverter.ToUInt32(bytes, 12);
                uint chunk0Type = BitConverter.ToUInt32(bytes, 16);
                if (chunk0Type != JsonChunk) {
                    error = "first chunk is not JSON";
                    return false;
                }
                var json = Encoding.UTF8.GetString(bytes, 20, (int)chunk0Len);
                if (json.Contains("pixygonItem")) return true; // already baked

                // "extras" must live inside "asset" — glTF guarantees an asset object exists.
                int assetIdx = json.IndexOf("\"asset\"", StringComparison.Ordinal);
                if (assetIdx < 0) {
                    error = "no asset object in glTF JSON";
                    return false;
                }
                int brace = json.IndexOf('{', assetIdx);
                if (brace < 0) {
                    error = "malformed asset object";
                    return false;
                }
                // manifestJson is {"pixygonItem":{...}} — exactly the extras object's content.
                string injected = json.Insert(brace + 1, "\"extras\":" + manifestJson + ",");

                var newJson = Encoding.UTF8.GetBytes(injected);
                int pad = (4 - newJson.Length % 4) % 4; // spec: JSON chunks pad with spaces
                var padded = new byte[newJson.Length + pad];
                Buffer.BlockCopy(newJson, 0, padded, 0, newJson.Length);
                for (int i = newJson.Length; i < padded.Length; i++) padded[i] = 0x20;

                int restOffset = 20 + (int)chunk0Len;      // any BIN (and further) chunks, verbatim
                int restLength = bytes.Length - restOffset;
                var outBytes = new byte[20 + padded.Length + restLength];
                BitConverter.GetBytes(Magic).CopyTo(outBytes, 0);
                BitConverter.GetBytes(version).CopyTo(outBytes, 4);
                BitConverter.GetBytes((uint)outBytes.Length).CopyTo(outBytes, 8);
                BitConverter.GetBytes((uint)padded.Length).CopyTo(outBytes, 12);
                BitConverter.GetBytes(JsonChunk).CopyTo(outBytes, 16);
                padded.CopyTo(outBytes, 20);
                if (restLength > 0) Buffer.BlockCopy(bytes, restOffset, outBytes, 20 + padded.Length, restLength);

                File.WriteAllBytes(glbPath, outBytes);
                return true;
            } catch (Exception e) {
                error = e.Message;
                return false;
            }
        }
    }
}
