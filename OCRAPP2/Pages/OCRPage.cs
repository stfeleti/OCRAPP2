using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using OCRAPP2.Models;
using OCRAPP2.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OCRAPP2.Pages
{
    public partial class OCRPage : ComponentBase
    {
        [Inject]
        public OCRService OCRService { get; set; }

        protected string DetectedTextLanguage;
        protected string imagePreview;
        protected bool loading = false;

        byte[] imageFileBytes;

        const string DefaultStatus = "Maximum size: 4 MB";

        protected string FileWarning = DefaultStatus;

        protected OcrResultDTO Result = new OcrResultDTO();

        private AvailableLanguage availableLanguages;
        Dictionary<string, LanguageDetails> TranslationList;

        const int MaxFileSize = 4 * 1024 * 1024; // 4MB 

        protected override async Task OnInitializedAsync()
        {
            availableLanguages = await OCRService.GetAvailableLanguages();
            TranslationList = availableLanguages.Translation;
        }

        protected async Task ViewImage(IFileListEntry[] files)
        {
            var file = files.FirstOrDefault();
            if (file == null)
            {
                return;
            }
            else if (file.Size > MaxFileSize)
            {
                FileWarning = $"The file size is {file.Size} bytes, this is more than the allowed limit of {MaxFileSize} bytes.";
                return;
            }
            else if (!file.Type.Contains("image"))
            {
                FileWarning = "Please uplaod a valid image file";
                return;
            }
            else
            {
                var memoryStream = new MemoryStream();
                await file.Data.CopyToAsync(memoryStream);
                imageFileBytes = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(imageFileBytes, 0, imageFileBytes.Length);

                imagePreview = string.Concat("data:image/png;base64,", base64String);
                memoryStream.Flush();
                FileWarning = DefaultStatus;
            }
        }
        protected private async Task GetText()
        {
            if (imageFileBytes != null)
            {
                loading = true;
                Result = await OCRService.GetTextFromImage(imageFileBytes);

                //Detect Language
                if (TranslationList.ContainsKey(Result.Language))
                {
                    DetectedTextLanguage = TranslationList[Result.Language].Name;
                }
                else
                {
                    DetectedTextLanguage = "Unknown";
                }
                loading = false;
            }
        }
    }
}