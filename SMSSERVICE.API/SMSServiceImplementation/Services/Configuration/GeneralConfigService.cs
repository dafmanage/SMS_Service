using IntegratedImplementation.Interfaces.Configuration;
using IntegratedInfrustructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratedImplementation.Services.Configuration
{
    public class GeneralConfigService :IGeneralConfigService
    {
        private readonly ApplicationDbContext _dbContext;

        public GeneralConfigService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateCode(EnumList.GeneralCodeType GeneralCodeType)
        {
            var curentCode = await _dbContext.GeneralCodes.FirstOrDefaultAsync(x => x.GeneralCodeType == GeneralCodeType);
            if (curentCode != null)
            {
                var generatedCode = $"{curentCode.InitialName}-{curentCode.CurrentNumber.ToString().PadLeft(curentCode.Pad, '0')}";

                curentCode.CurrentNumber += 1;
                await _dbContext.SaveChangesAsync();
                return generatedCode;
            }
            return "";
        }

        public async Task<string> GetFiles(string path)
        {
            byte[] imageArray = await File.ReadAllBytesAsync(path);
            string imageRepresentation = Convert.ToBase64String(imageArray);
            return imageRepresentation.ToString();
        }

        public async Task<string> UploadFiles(IFormFile formFile, string Name, string FolderName)
        {

            var path = Path.Combine("wwwroot", FolderName);
            string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), path);

            if (!Directory.Exists(pathToSave))
                Directory.CreateDirectory(pathToSave);

            if (formFile != null && formFile.Length > 0)
            {
                try
                {
                    string newPath;
                    string filePath;
                    string fileName;
                    string fileExtension;
                    if (formFile.ContentType == "image/svg+xml")
                    {
                        var sanitizedSvg = await SanitizeSvgFile(formFile);

                         fileExtension = Path.GetExtension(formFile.FileName);
                         fileName = $"{Name}{fileExtension}";
                         filePath = Path.Combine(pathToSave, fileName);

                        if (File.Exists(filePath))
                            File.Delete(filePath);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await sanitizedSvg.CopyToAsync(stream);
                        }

                         newPath = Path.Combine(path, fileName);
                        return newPath;
                    }

                    fileExtension = Path.GetExtension(formFile.FileName);
                    fileName = $"{Name}{fileExtension}";
                    filePath = Path.Combine(pathToSave, fileName);

                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    newPath = Path.Combine(path, fileName);
                    return newPath;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            return "";
        }
        private async Task<IFormFile> SanitizeSvgFile(IFormFile originalFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await originalFile.CopyToAsync(memoryStream);
                var svgContent = Encoding.UTF8.GetString(memoryStream.ToArray());

                var sanitizer = new Ganss.Xss.HtmlSanitizer();
                var sanitizedSvgContent = sanitizer.Sanitize(svgContent);

                var sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedSvgContent);

                var sanitizedStream = new MemoryStream(sanitizedBytes);

                return new FormFile(
                    baseStream: sanitizedStream,
                    baseStreamOffset: 0,
                    length: sanitizedStream.Length,
                    name: originalFile.Name,
                    fileName: originalFile.FileName
                );
            }
        }
    }
}
