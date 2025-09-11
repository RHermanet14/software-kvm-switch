namespace Shared
{
    public class ClipboardData
    {
        public string Format { get; set; } = "";
        public string DataType { get; set; } = "";
        public string TextData { get; set; } = "";
        public byte[] BinaryData { get; set; } = [];
    }

    public class ClipboardEvent
    {
        public List<ClipboardData> ClipboardElements = [];
        public List<string> AvailableFormats { get; set; } = [];

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

        public ClipboardData GetClipboardContent()
        {
            var cd = new ClipboardData();
            try
            {
                var ClipboardObject = Clipboard.GetDataObject();
                if (ClipboardObject == null) return cd;
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
                        cd.DataType = "error";
                        cd.TextData = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                cd.DataType = "error";
                cd.TextData = ex.Message;
            }
            return cd;
        }
        public void SetClipboardContent()
        {

        }
    }
    
}