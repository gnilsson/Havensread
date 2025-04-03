using System.Buffers.Text;
using System.Text;

namespace Havensread.ServiceDefaults.Misc;

public sealed class Base64Converter
{
    public static string Encode(string input)
    {
        // Get byte span of the input string
        var inputBytes = Encoding.UTF8.GetBytes(input).AsMemory();

        // Calculate the required length for the Base64 encoded output
        var base64Bytes = new byte[Base64.GetMaxEncodedToUtf8Length(inputBytes.Length)];

        // Encode the input bytes to Base64
        Base64.EncodeToUtf8(inputBytes.Span, base64Bytes, out _, out int bytesWritten);

        // Convert the Base64 encoded bytes to a string
        return Encoding.UTF8.GetString(base64Bytes.AsSpan(0, bytesWritten));
    }

    public static string Decode(string base64Input)
    {
        // Get byte span of the Base64 input string
        var base64Bytes = Encoding.UTF8.GetBytes(base64Input).AsMemory();

        // Calculate the required length for the decoded output
        var decodedBytes = new byte[Base64.GetMaxDecodedFromUtf8Length(base64Bytes.Length)];

        // Decode the Base64 encoded bytes
        Base64.DecodeFromUtf8(base64Bytes.Span, decodedBytes, out _, out int bytesWritten);

        // Convert the decoded bytes to a string
        return Encoding.UTF8.GetString(decodedBytes.AsSpan(0, bytesWritten));
    }
}
