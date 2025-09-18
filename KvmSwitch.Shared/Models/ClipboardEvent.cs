using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using MessagePack;

namespace Shared
{
    [MessagePackObject]
    public class ClipboardData
    {
        [MessagePack.Key(0)]
        public string Format { get; set; } = "";
        [MessagePack.Key(1)]
        public string DataType { get; set; } = "";
        [MessagePack.Key(2)]
        public string TextData { get; set; } = "";
        [MessagePack.Key(3)]
        public byte[] BinaryData { get; set; } = [];
    }

    public class ClipboardHelper
    {
        public static byte[] Compress(byte[] data)
        {
            using var compressedStream = new MemoryStream();
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                gzipStream.Write(data, 0, data.Length);
            }
            return compressedStream.ToArray();
        }

        public static byte[] Decompress(byte[] compressedData)
        {
            using var compressedStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var decompressedStream = new MemoryStream();
            gzipStream.CopyTo(decompressedStream);
            return decompressedStream.ToArray();
        }

        public static void AnalyzeSharedInitialDataSize(SharedInitialData data)
        {
            Console.WriteLine($"Clipboard elements count: {data.CurrentClipboard.ClipboardElements.Count}");

            foreach (var element in data.CurrentClipboard.ClipboardElements)
            {
                Console.WriteLine($"Format: {element.Format}, DataType: {element.DataType}");
                if (element.DataType == "text")
                {
                    Console.WriteLine($"  Text length: {element.TextData?.Length ?? 0}");
                }
                else
                {
                    Console.WriteLine($"  Binary length: {element.BinaryData?.Length ?? 0}");
                }
            }

            // Test MessagePack size before compression
            byte[] msgPackOnly = MessagePackSerializer.Serialize(data);
            Console.WriteLine($"MessagePack before compression: {msgPackOnly.Length} bytes");

            // Test compression ratio
            byte[] compressed = Compress(msgPackOnly);
            Console.WriteLine($"After compression: {compressed.Length} bytes");
            Console.WriteLine($"Compression ratio: {(1.0 - (double)compressed.Length / msgPackOnly.Length) * 100:F1}%");
        }
        public static void AnalyzeMessagePackSize(InitialMouseData data)
        {
            Console.WriteLine($"Clipboard elements count: {data.Shared.CurrentClipboard.ClipboardElements.Count}");

            foreach (var element in data.Shared.CurrentClipboard.ClipboardElements)
            {
                Console.WriteLine($"Format: {element.Format}, DataType: {element.DataType}");
                if (element.DataType == "text")
                {
                    Console.WriteLine($"  Text length: {element.TextData?.Length ?? 0}");
                }
                else
                {
                    Console.WriteLine($"  Binary length: {element.BinaryData?.Length ?? 0}");
                }
            }

            // Test MessagePack size before compression
            byte[] msgPackOnly = MessagePackSerializer.Serialize(data);
            Console.WriteLine($"MessagePack before compression: {msgPackOnly.Length} bytes");

            // Test compression ratio
            byte[] compressed = Compress(msgPackOnly);
            Console.WriteLine($"After compression: {compressed.Length} bytes");
            Console.WriteLine($"Compression ratio: {(1.0 - (double)compressed.Length / msgPackOnly.Length) * 100:F1}%");
        }
    }

    [MessagePackObject]
    public class ClipboardEvent
    {
        public ClipboardEvent()
        {
            ClipboardElements = [];
        }
        [MessagePack.Key(0)]
        public List<ClipboardData> ClipboardElements { get; set; }

        private void OptimizeClipboardData() // Make private?
        {
            var optimized = new List<ClipboardData>();
        
            var textElements = ClipboardElements
                .Where(x => x.DataType == "text")
                .GroupBy(x => x.TextData)  // Group by actual content
                .Select(g => g.OrderBy(x => GetTextFormatPriority(x.Format)).First()) // Keep best format
                .ToList();
            optimized.AddRange(textElements);
        
            // For images, keep only the most efficient format
            var imageElements = ClipboardElements
                .Where(x => x.DataType == "binary")
                .ToList();
        
            if (imageElements.Count != 0)
            {
                var bestImage = imageElements
                    .OrderBy(x => GetImageFormatPriority(x.Format))
                    .ThenBy(x => x.BinaryData.Length)
                    .First();
            
                optimized.Add(bestImage);
            }
            ClipboardElements = optimized;
        }

        private static int GetImageFormatPriority(string format)
        {
            return format.ToLower() switch
            {
                "png" => 1,                           // Best compression
                "format17" => 2,                      // Unknown format
                "system.drawing.bitmap" => 3,         // .NET format
                "bitmap" => 4,                        // Standard bitmap
                "deviceindependentbitmap" => 5,       // Usually largest
                _ => 6
            };
        }
    
        private static int GetTextFormatPriority(string format)
        {
            return format switch
            {
                "UnicodeText" => 1,     // Preferred text format
                "Text" => 2,
                "System.String" => 3,
                _ => 4
            };
        }

        private void AddTextElement(string formatType, string data)
        {
            ClipboardElements.Add(new ClipboardData
            {
                Format = formatType,
                DataType = "text",
                TextData = data
            });
        }

        private void AddBinaryElement(string formatType, byte[] data)
        {
            ClipboardElements.Add(new ClipboardData
            {
                Format = formatType,
                DataType = "binary",
                BinaryData = data
            });
        }

        private void ClearElementsList()
        {
            ClipboardElements.Clear();
        }

        public void GetClipboardContent() // Populate ClipboardElements
        {
            ClearElementsList(); // Remove previous info
            try
            {
                var ClipboardObject = Clipboard.GetDataObject();
                if (ClipboardObject == null)
                {
                    Console.WriteLine("Clipboard was empty");
                    return;
                }
                string[] formats = ClipboardObject.GetFormats();
                foreach (string format in formats)
                {
                    try
                    {
                        var data = ClipboardObject.GetData(format);
                        if (data == null) continue;
                        if (data is string textData)
                        {
                            if (!string.IsNullOrEmpty(textData))
                                AddTextElement(format, textData);
                        }
                        else if (data is Image image)
                        {
                            using var ms = new MemoryStream();
                            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            AddBinaryElement(format, ms.ToArray());
                        }
                        else if (data is byte[] binaryData)
                        {
                            AddBinaryElement(format, binaryData);
                        }
                        else if (data is MemoryStream stream)
                        {
                            AddBinaryElement(format, stream.ToArray());
                        }
                        else if (data is string[] stringArray)
                        {
                            string combined = string.Join(Environment.NewLine, stringArray);
                            AddTextElement(format, combined);
                        }
                        else if (data is System.Collections.Specialized.StringCollection stringCollection)
                        {
                            string combined = string.Join(Environment.NewLine, stringCollection.Cast<string>());
                            AddTextElement(format, combined);
                        }
                        else if (data is IEnumerable<string> stringEnumerable)
                        {
                            string combined = string.Join(Environment.NewLine, stringEnumerable);
                            AddTextElement(format, combined);
                        }
                        else
                        {
                            string? fallbackText = data.ToString();
                            if (!string.IsNullOrEmpty(fallbackText) && fallbackText != data.GetType().ToString())
                            {
                                AddTextElement(format, fallbackText);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return;
            }
            OptimizeClipboardData();
            return;
        }

        public void SetClipboardContent()
        {
            var ClipboardObject = new DataObject();
            foreach (ClipboardData element in ClipboardElements)
            {
                try
                {
                    if (element.DataType == "text" && !string.IsNullOrEmpty(element.TextData)) // Switch to enums
                    {
                        switch (element.Format.ToLower()) // Switch to specifying the format, then setting it after it breaks
                        {
                            case "text":
                            case "unicodetext":
                            case "system.string":
                                ClipboardObject.SetText(element.TextData, TextDataFormat.UnicodeText);
                                break;
                            case "html":
                                ClipboardObject.SetText(element.TextData, TextDataFormat.Html);
                                break;
                            case "rtf":
                                ClipboardObject.SetText(element.TextData, TextDataFormat.Rtf);
                                break;
                            case "csv":
                            case "commaseparatedvalue":
                                ClipboardObject.SetText(element.TextData, TextDataFormat.CommaSeparatedValue);
                                break;
                            default:
                                ClipboardObject.SetData(element.Format, element.TextData); // Make sure this is correct
                                break;
                        }
                    }
                    else if (element.DataType == "binary" && element.BinaryData != null)
                    {
                        switch (element.Format.ToLower()) // Putting clipboard content into object only sets it to png (for now)
                        {
                            case "bitmap":
                            case "system.drawing.bitmap":
                            case "png":
                            case "jpeg":
                            case "gif":
                            case "tiff":
                                try
                                {
                                    using var ms = new MemoryStream(element.BinaryData);
                                    var image = Image.FromStream(ms);
                                    ClipboardObject.SetImage(image);
                                }
                                catch
                                {
                                    Console.WriteLine("Error: unable to set image from binary data");
                                    ClipboardObject.SetData(element.Format, element.BinaryData);
                                }
                                break;
                            default:
                                using (var ms = new MemoryStream(element.BinaryData))
                                {
                                    ClipboardObject.SetData(element.Format, ms);
                                }
                                break;
                        }
                    }
                    else if (element.DataType == "error")
                    {
                        throw new Exception("error data type detected");
                    }
                    else
                    {
                        throw new Exception("unknown data type detected");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting clipboard: {ex.Message}");
                    throw;
                }

            }
            Clipboard.SetDataObject(ClipboardObject, true);
        }
    }
}