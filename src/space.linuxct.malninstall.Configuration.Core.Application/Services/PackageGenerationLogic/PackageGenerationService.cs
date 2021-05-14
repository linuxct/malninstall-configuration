using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using space.linuxct.malninstall.Configuration.Common.Enums;
using space.linuxct.malninstall.Configuration.Common.Exceptions;
using space.linuxct.malninstall.Configuration.Common.Helpers.NativeExecutor;
using space.linuxct.malninstall.Configuration.Common.Helpers.Random;
using space.linuxct.malninstall.Configuration.Common.Models;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.PackageGenerationLogic;

namespace space.linuxct.malninstall.Configuration.Core.Application.Services.PackageGenerationLogic
{
    public class PackageGenerationService : IPackageGenerationService
    {
        private readonly ILogger<PackageGenerationService> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly WordDictionary _dictionary;
        private const string ToolsBasePath = "/home/malninstall/files";
        private string _packageName;
        private string _workFolder;
        private string _applicationName;

        public PackageGenerationService(ILogger<PackageGenerationService> logger, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _dictionary = _configuration.GetSection("Dictionary").Get<WordDictionary>();
        }
        
        public async Task<PackageDetails> ProcessForPackageAsync(string packageName)
        {
            _packageName = packageName;
            try
            {
                CheckIfFileExists();
                CreateWorkFolder();
                UnpackTemplateZip();
                await ReplacePackageNameInAndroidManifest();
                await AddRandomnessToApplicationName();
                BuildApplication();
                ZipAlignApplication();
                SignApplication();
                await CopyResultToServeDirectory();
                CleanupWorkFolder();
                return CreatePackageDetailsForResult(PackageCreationStatus.Success);
            }
            catch (FileAlreadyExistsException)
            {
                CleanupWorkFolder();
                return CreatePackageDetailsForResult(PackageCreationStatus.AlreadyExists);
            }
            catch (Exception)
            {
                CleanupWorkFolder();
                return CreatePackageDetailsForResult(PackageCreationStatus.NotCreated);
            }
        }

        private void CheckIfFileExists()
        {
            var directoryForPackageName = Path.Combine(ToolsBasePath, "Downloads", _packageName);
            var directoryExists = Directory.Exists(directoryForPackageName);
            var fileExists = File.Exists(Path.Combine(directoryForPackageName, $"{_packageName}.apk"));
            if (directoryExists || fileExists)
            {
                throw new FileAlreadyExistsException($"File {_packageName}.apk already exists");
            }
        }

        private void CreateWorkFolder()
        {
            var path = Path.Combine(ToolsBasePath, "Downloads", "creation");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var fullPath = Path.Combine(ToolsBasePath, "Downloads", "creation", _packageName);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            _workFolder = fullPath;
        }
        
        private void UnpackTemplateZip()
        {
            var zipPath = Path.Combine(ToolsBasePath, "template.zip");
            NativeExecutionHelper.FireAndForgetOnHost($"unzip {zipPath} -d {ToolsBasePath}/Downloads/creation/{_packageName}/");
        }
        
        private async Task ReplacePackageNameInAndroidManifest()
        {
            var manifestPath = Path.Combine(_workFolder, "AndroidManifest.xml");
            var text = await File.ReadAllTextAsync(manifestPath);
            text = text.Replace("PACK_NAME", _packageName);
            await File.WriteAllTextAsync(manifestPath, text);
        }

        private async Task AddRandomnessToApplicationName()
        {
            var applicationNameBuilder = new StringBuilder();
            var random = new Random();
            var actionRandomPosition = random.Next(0, _dictionary.Actions.Count-1);
            applicationNameBuilder.Append(_dictionary.Actions[actionRandomPosition] + " ");
            var decomposedPackageName = _packageName.Split(".");
            foreach (var namePart in decomposedPackageName.OrderBy(x=>random.Next()))
            {
                if (RandomProvider.GetThreadRandom().NextDouble() >= 0.50)
                {
                    applicationNameBuilder.Append(namePart + " ");
                    var connectorRandomPosition = random.Next(0, _dictionary.Connectors.Count-1);
                    applicationNameBuilder.Append(_dictionary.Connectors[connectorRandomPosition] + " ");
                }
            }
            var endingRandomPosition = random.Next(0, _dictionary.Endings.Count-1);
            applicationNameBuilder.Append(_dictionary.Endings[endingRandomPosition]);
            _applicationName = applicationNameBuilder.ToString();
            var manifestPath = Path.Combine(_workFolder, "AndroidManifest.xml");
            var text = await File.ReadAllTextAsync(manifestPath);
            text = text.Replace("APP_LABEL", _applicationName);
            await File.WriteAllTextAsync(manifestPath, text);
        }

        private void BuildApplication()
        {
            NativeExecutionHelper.FireAndForgetOnHost($"cd {ToolsBasePath}/Downloads/creation/{_packageName}/ && java -jar {ToolsBasePath}/apktool.jar b .");
        }

        private void ZipAlignApplication()
        {
            NativeExecutionHelper.FireAndForgetOnHost($"cd {ToolsBasePath}/Downloads/creation/{_packageName}/ && mv dist/*.apk dist/out.apk && " +
                                                      $"{ToolsBasePath}/zipalign -f 4 dist/out.apk dist/out-za.apk");
        }

        private void SignApplication()
        {
            NativeExecutionHelper.FireAndForgetOnHost($"cd {ToolsBasePath}/Downloads/creation/{_packageName}/dist && {ToolsBasePath}/apksigner " +
                                                      "sign --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true --v4-signing-enabled " +
                                                      $"--key {ToolsBasePath}/testkey.pk8 --cert {ToolsBasePath}/testkey.x509.pem " +
                                                      "out-za.apk");
        }

        private async Task CopyResultToServeDirectory()
        {
            if (!File.Exists(Path.Combine(_workFolder, "dist", "out-za.apk")))
            {
                throw new FileNotFoundException("Missing out-za.apk file");
            }

            var serveDirectory = Path.Combine(ToolsBasePath, "Downloads", _packageName);
            if (!Directory.Exists(serveDirectory))
            {
                Directory.CreateDirectory(serveDirectory);
            }
            File.Copy(Path.Combine(_workFolder, "dist", "out-za.apk"), Path.Combine(ToolsBasePath, "Downloads", _packageName, $"{_packageName}.apk"));
            await File.WriteAllTextAsync(Path.Combine(ToolsBasePath, "Downloads", _packageName, "ServeName"), $"{_applicationName}.apk");
        }
        
        private void CleanupWorkFolder()
        {
            if (string.IsNullOrEmpty(_workFolder))
            {
                _workFolder = Path.Combine(ToolsBasePath, "Downloads", "creation", _packageName);
            }

            if (Directory.Exists(_workFolder))
            {
                Directory.Delete(_workFolder, true);
            }
        }

        private PackageDetails CreatePackageDetailsForResult(PackageCreationStatus status)
        {
            var resultFileLocation = Path.Combine(ToolsBasePath, "Downloads", _packageName, $"{_packageName}.apk");
            if (!File.Exists(resultFileLocation))
            {
                resultFileLocation = null;
            }
            return new PackageDetails
            {
                Status = status,
                FileLocation = resultFileLocation,
                ApplicationName = $"{_applicationName ?? GetApplicationNameFromDisk() ?? _packageName}.apk"
            };
        }

        private string GetApplicationNameFromDisk()
        {
            var serveNameFilePath = Path.Combine(ToolsBasePath, "Downloads", _packageName, "ServeName");
            return File.Exists(serveNameFilePath) ? File.ReadAllText(serveNameFilePath) : null;
        }
    }
}