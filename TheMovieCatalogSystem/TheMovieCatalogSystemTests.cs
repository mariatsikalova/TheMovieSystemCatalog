
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Text.Json;
using TheMovieCatalogSystem.Models;

namespace TheMovieCatalogSystem
{
    [TestFixture]
    public class Tests
        
    {
        private RestClient client;
        private static string lastCreateadMovieId;
        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI4NTM5NjhmMi01ZjQ1LTQ5MzctODZjNy1hN2E1NGZkMDQ0YzMiLCJpYXQiOiIwNC8xOC8yMDI2IDA4OjI1OjIyIiwiVXNlcklkIjoiM2I4NmY2ODQtNThkNS00NDQ1LTYzOGItMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJtaW5pQGFidi5jb20iLCJVc2VyTmFtZSI6Im1pbmkyMDI2IiwiZXhwIjoxNzc2NTIyMzIyLCJpc3MiOiJNb3ZpZUNhdGFsb2dfQXBwX1NvZnRVbmkiLCJhdWQiOiJNb3ZpZUNhdGFsb2dfV2ViQVBJX1NvZnRVbmkifQ.CDdmRGgMFOUAXbAbQ6tRUtJlmbQJxV6qgrY2FlPAniY";
        private const string LoginPassword = "mini1234";
        private const string LoginEmail = "mini@abv.com";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }


        [Order(1)]
        [Test]
        public void CreateMovie_WithRequiredFields_ShouldReturnSuccess()
        {
            var movieData = new MovieDTO
            {
                Title = "Test Movie",
                Description = "This is a test movie description."
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");

            Assert.That(createResponse.Msg, Is.EqualTo("Movie created successfully!"));

            
            Assert.That(createResponse.Movie, Is.Not.Null);

            Assert.That(createResponse.Movie.Id, Is.Not.Null.And.Not.Empty);

            lastCreateadMovieId = createResponse.Movie.Id;
        }
        [Order(2)]
        [Test]
        public void EditExistingMovie_ShouldReturnSuccess()
        {
            var editRequestData = new MovieDTO
            {
                Title = "Edited Movie",
                Description = "This is an edited movie description."
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);

            request.AddQueryParameter("movieId", lastCreateadMovieId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");

            
            Assert.That(editResponse.Msg, Is.EqualTo("Movie edited successfully!"));
        }
        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Movie/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);
        }


            [Order(4)]
            [Test]
            public void DeleteMovie_ShouldReturnSuccess()
            {
                var request = new RestRequest("/api/Movie/Delete", Method.Delete);
                request.AddQueryParameter("movieId", lastCreateadMovieId);

                var response = this.client.Execute(request);

                var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
                Assert.That(deleteResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }
        [Order(5)]
        [Test]
        public void CreateMovie_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            var movieData = new MovieDTO
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
                "Expected status code 400 Bad Request.");
        }
        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "9999999";

            var editRequestData = new MovieDTO
            {
                Title = "Edited Movie",
                Description = "This is an edited movie description."
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
                "Expected status code 400 Bad Request.");

            Assert.That(responseData.Msg,
                Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }
        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "9999999";

            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", nonExistingMovieId);

            var response = this.client.Execute(request);

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
                "Expected status code 400 Bad Request.");

            Assert.That(responseData.Msg,
                Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
    
