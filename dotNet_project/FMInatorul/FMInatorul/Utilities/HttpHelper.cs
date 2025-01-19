using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace FMInatorul.Utilities
{
    public static class HttpHelper
    {
        public static async Task<string> GetJwtTokenAsync(string username, string password, string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // Create JSON payload for login
                    var loginPayload = new
                    {
                        username = username,
                        password = password
                    };

                    var jsonPayload = JsonConvert.SerializeObject(loginPayload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send POST request to login endpoint
                    var response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                        // Extract the JWT token
                        if (responseJson.TryGetValue("token", out var jwtToken))
                        {
                            return jwtToken;
                        }
                    }

                    Debug.WriteLine($"Failed to get JWT token. Status Code: {response.StatusCode}");
                    return null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error during JWT token retrieval: {ex.Message}");
                    return null;
                }
            }
        }

        // Uploads the PDF file to a Flask API.
        public static async Task<HttpResponseMessage> UploadPdfToFlaskApiAsync(IFormFile file, string url, string jwtToken)
        {
            using (var client = new HttpClient())
            {
                // Create a multipart content for the file upload
                using (var multipartContent = new MultipartFormDataContent())
                {
                    var fileStream = file.OpenReadStream();

                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    multipartContent.Add(fileContent, "file", file.FileName);

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    try
                    {
                        var response = await client.PostAsync(url, multipartContent);
                        return response;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Debug.WriteLine($"Error uploading file: {ex.Message}");
                        throw;
                    }
                    finally
                    {
                        // Ensure the file stream is properly disposed
                        fileStream.Dispose();
                    }
                }
            }
        }
    }
}
