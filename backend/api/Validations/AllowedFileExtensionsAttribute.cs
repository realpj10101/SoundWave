using System.ComponentModel.DataAnnotations;

namespace api.Validations;

public class AllowedFileExtensionsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file is not null)
        {
            if (!IsFileValid(file))
            {
                // get only allowed extensions to show
                string? keys = null;
                foreach (var key in _fileSignatures.Keys)
                {
                    keys += key + ", ";
                }

                return new ValidationResult($"File type is not allowed. These extensions are allowed only: {keys}");
            }
        }

        return ValidationResult.Success;
    }


    public static bool IsFileValid(IFormFile file)
    {
        using var reader = new BinaryReader(file.OpenReadStream());
        var signatures = _fileSignatures.Values.SelectMany(x => x).ToList(); // flatten all signatures to single list
        var headerBytes = reader.ReadBytes(_fileSignatures.Max(m => m.Value.Max(n => n.Length)));
        bool result = signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
        return result;
    }

    public static Dictionary<string, List<byte[]>> _fileSignatures = new()
    {
        {
            ".jpeg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
            }
        },
        {
            ".jpg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
            }
        },
        {
            ".jpeg2000",
            new List<byte[]> { new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A } }
        },
        { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        {
            ".webp", new List<byte[]>
            {
                new byte[] { 0x52, 0x49, 0x46, 0x46 }, // RIFF
                new byte[] { 0x57, 0x45, 0x42, 0x50 }, // WEBP
            }
        },
        {
            ".avif", new List<byte[]>
            {
                new byte[] { 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66 }, // ftypavif
            }
        },
    };
}