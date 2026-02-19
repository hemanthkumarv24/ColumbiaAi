using System;
using System.IO;

namespace ColumbiaAi.Backend.Controllers
{
    public class CodeQLValidationController
    {
        // ❌ Test Case 1: Path Traversal (no external packages needed)
        public string ReadUserFile(string fileName)
        {
            // User-controlled input directly used in file path
            var path = "/tmp/" + fileName;

            return File.ReadAllText(path);
        }

        // ❌ Test Case 2: Hardcoded Credential
        private const string ApiKey = "SuperSecretKey12345";

        public void PrintSecret()
        {
            Console.WriteLine(ApiKey);
        }

        // ❌ Test Case 3: Command Injection-style pattern
        public void RunCommand(string input)
        {
            // Dangerous concatenation (CodeQL flags this)
            var cmd = "echo " + input;
            System.Diagnostics.Process.Start("bash", "-c \"" + cmd + "\"");
        }
    }
}
